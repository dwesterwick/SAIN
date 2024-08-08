using Comfort.Common;
using EFT;
using SAIN.Components;
using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.BotControllerSpace.Classes
{
    public class LightFinder : SAINControllerBase
    {
        private const float LIGHTLOOP_FREQ = 0.1f;
        public readonly List<LightComponent> AllLights = new List<LightComponent>();
        private readonly List<LightComponent> _activeLights = new List<LightComponent>();

        private CalcDistanceJob _lightDistanceJob;
        private JobHandle _handle;
        private NativeArray<RaycastHit> _hits;
        private NativeArray<RaycastCommand> _commands;
        private readonly List<PlayerComponent> _localPlayerList = new List<PlayerComponent>();

        private bool _gameEnding = false;

        public LightFinder(SAINBotController botController) : base(botController)
        {
            //BotLightTracker.GetLights(AllLights);
            //botController.StartCoroutine(findLightsLoop());
        }

        public void Update()
        {
            if (_gameEnding) return;
            IBotGame botGame = Singleton<IBotGame>.Instance;
            if (botGame == null) return;
            switch (botGame.Status) {
                case GameStatus.Stopping:
                case GameStatus.Stopped:
                    _gameEnding = true;
                    break;

                default: return;
            }
        }

        private IEnumerator findLightsLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(LIGHTLOOP_FREQ);
            while (true) {
                yield return wait;

                if (_gameEnding) continue;

                var gameWorld = SAINGameWorld;
                if (gameWorld == null) continue;

                getPlayers(_localPlayerList);
                int playerCount = _localPlayerList.Count;
                if (playerCount == 0) continue;

                findActiveLights(_activeLights);
                int lightCount = _activeLights.Count;
                if (lightCount == 0) continue;

                //Logger.LogDebug($"Found {playerCount} players and {lightCount} lights. Checking Distances...");

                _lightDistanceJob = createJob(_localPlayerList, _activeLights, out int total);
                _handle = _lightDistanceJob.Schedule(total, new JobHandle());
                yield return null;
                _handle.Complete();

                filterLights(_lightDistanceJob, _lightcasts, _localPlayerList, _activeLights, playerCount, lightCount);
                _lightDistanceJob.Dispose();
                int raycastCount = _lightcasts.Count;
                if (raycastCount == 0) continue;

                Logger.LogDebug($"Found {raycastCount} lights in range. Checking LOS with raycasts...");

                LayerMask mask = LayerMaskClass.HighPolyWithTerrainMask;
                _commands = new NativeArray<RaycastCommand>(raycastCount, Allocator.TempJob);
                for (int i = 0; i < raycastCount; i++) {
                    LightRaycastData data = _lightcasts[i];
                    _commands[i] = new RaycastCommand {
                        from = data.PlayerPosition,
                        direction = data.LightPosition - data.PlayerPosition,
                        distance = data.Distance,
                        layerMask = mask,
                        maxHits = 1,
                    };
                }
                _hits = new NativeArray<RaycastHit>(raycastCount, Allocator.TempJob);

                _handle = RaycastCommand.ScheduleBatch(_commands, _hits, 5);
                yield return null;
                _handle.Complete();

                int illumCount = 0;
                float time = Time.time;
                for (int i = 0; i < raycastCount; i++) {
                    RaycastHit hit = _hits[i];
                    if (hit.collider != null) {
                        continue;
                    }

                    LightRaycastData data = _lightcasts[i];
                    if (data.Player == null || data.LightComponent == null) {
                        continue;
                    }
                    //if (!data.LightComponent.LightActive) {
                    //    continue;
                    //}
                    LightComponent light = data.LightComponent;
                    if (light == null) {
                        continue;
                    }

                    Vector3 direction = data.PlayerPosition - data.LightPosition;
                    if (light.Type == LightType.Spot &&
                        Vector3.Angle(direction, light.LightPointDirection) > light.Angle) {
                        continue;
                    }

                    float illuminationLevel = calcIllumLevel(light, data.Distance);
                    if (illuminationLevel < 0.05f) {
                        continue;
                    }
                    data.Player.Illumination.SetIllumination(illuminationLevel, time);
                    DebugGizmos.Ray(data.PlayerPosition, data.LightPosition - data.PlayerPosition, Color.red, data.Distance, 0.03f, true, 0.15f, true);
                    illumCount++;
                }

                if (illumCount > 0) {
                    Logger.LogDebug($"{illumCount} lights in range!");
                }
                disposeRaycast();
            }
        }

        private float calcIllumLevel(LightComponent light, float distance)
        {
            const float MIN = 0.66f;
            float range = light.Range;
            float ratio = 1f - distance / range;
            if (ratio <= MIN) {
                return 1f;
            }
            float num = 1f - MIN;
            float num2 = ratio - MIN;
            float ratio2 = 1f - num2 / num;
            float illuminationLevel = Mathf.Lerp(0.05f, 1f, ratio2);

            float intensity = light.Intensity / 5f;
            intensity = Mathf.Clamp(intensity, 0f, 1f);
            illuminationLevel *= intensity;

            return illuminationLevel;
        }

        private void disposeRaycast()
        {
            if (_hits.IsCreated) _hits.Dispose();
            if (_commands.IsCreated) _commands.Dispose();
        }

        private static void filterLights(CalcDistanceJob job, List<LightRaycastData> result, List<PlayerComponent> players, List<LightComponent> lights, int playerCount, int lightCount)
        {
            int count = 0;

            NativeArray<Vector3> directions = job.directions;
            NativeArray<float> distances = job.distances;

            result.Clear();
            for (int p = 0; p < playerCount; p++) {
                PlayerComponent player = players[p];
                Vector3 playerPos = player != null ? player.Transform.BodyPosition : Vector3.zero;
                for (int l = 0; l < lightCount; l++) {
                    LightComponent light = lights[l];

                    float lightRange = light != null ? light.Range : 0;
                    Vector3 lightPos = light != null ? light.LightPosition : Vector3.zero;
                    Vector3 directionToPlayer = directions[count];
                    float distance = distances[count];

                    if (player != null &&
                        light != null &&
                        distance < lightRange) {
                        LightRaycastData data = new LightRaycastData {
                            LightComponent = light,
                            Player = player,
                            Distance = distance - 0.1f,
                            LightPosition = lightPos,
                            PlayerPosition = playerPos,
                        };
                        result.Add(data);
                    }
                    count++;
                }
            }
        }

        private readonly List<LightRaycastData> _lightcasts = new List<LightRaycastData>();

        private struct LightRaycastData
        {
            public LightComponent LightComponent;
            public PlayerComponent Player;
            public float Distance;
            public Vector3 LightPosition;
            public Vector3 PlayerPosition;
        }

        private CalcDistanceJob createJob(List<PlayerComponent> players, List<LightComponent> lights, out int total)
        {
            int lightCount = lights.Count;
            int playerCount = players.Count;
            total = lightCount * playerCount;
            int count = 0;
            NativeArray<Vector3> directions = new NativeArray<Vector3>(total, Allocator.TempJob);

            for (int p = 0; p < playerCount; p++) {
                PlayerComponent player = players[p];
                Vector3 playerPos = player.Transform.BodyPosition;
                for (int l = 0; l < lightCount; l++) {
                    directions[count] = playerPos - _activeLights[l].LightPosition;
                    count++;
                }
            }

            var job = new CalcDistanceJob();
            job.Create(directions);
            return job;
        }

        private void findActiveLights(List<LightComponent> result)
        {
            result.Clear();
            foreach (LightComponent light in AllLights) {
                if (light != null && light.Active) {
                    result.Add(light);
                }
            }
        }

        public void AddLight(LightComponent light)
        {
            AllLights.Add(light);
        }

        private void getPlayers(List<PlayerComponent> result)
        {
            result.Clear();
            var players = SAINGameWorld.PlayerTracker.AlivePlayers;
            foreach (PlayerComponent player in players.Values) {
                if (player != null && !player.IsAI) {
                    result.Add(player);
                }
            }
        }

        public void Dispose()
        {
            if (!_handle.IsCompleted) _handle.Complete();
            disposeRaycast();
            _lightDistanceJob.Dispose();
        }
    }
}