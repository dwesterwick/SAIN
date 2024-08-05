using EFT;
using EFT.Visual;
using SAIN.Components;
using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using SAIN.Components.PlayerComponentSpace;
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
            botController.StartCoroutine(findLightsLoop());
        }

        public void Update()
        {
            if (_gameEnding) return;
            IBotGame botGame = SAINGameWorld.SAINBotController?.BotGame;
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

                Logger.LogDebug($"Found {playerCount} players and {lightCount} lights. Checking Distances...");

                _lightDistanceJob = createJob(_localPlayerList, _activeLights, out int total);
                _handle = _lightDistanceJob.Schedule(total, new JobHandle());
                yield return null;
                _handle.Complete();

                filterLights(_lightDistanceJob, _lightcasts, _localPlayerList, _activeLights, playerCount, lightCount);
                _lightDistanceJob.Dispose();
                int raycastCount = _lightcasts.Count;
                if (raycastCount == 0) continue;

                Logger.LogDebug($"Found {raycastCount} lights in range. Checking LOS with raycasts...");

                LayerMask mask = LayerMaskClass.HighPolyWithTerrainMaskAI;
                _commands = new NativeArray<RaycastCommand>(raycastCount, Allocator.TempJob);
                for (int i = 0; i < raycastCount; i++) {
                    LightRaycastData data = _lightcasts[i];
                    _commands[i] = new RaycastCommand {
                        from = data.LightPosition,
                        direction = data.Direction,
                        distance = data.Distance,
                        layerMask = mask,
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
                    if (data.Player == null || data.LightComponent == null || data.LightComponent.Light == null) {
                        continue;
                    }

                    Light light = data.LightComponent.Light;
                    float range = light.range;
                    float distance = data.Distance;
                    float ratio = 1f - distance / range;
                    float illuminationLevel = Mathf.Lerp(0.1f, 1f, ratio);
                    float intensity = light.intensity / 5f;
                    intensity = Mathf.Clamp(intensity, 0.1f, 1f);
                    illuminationLevel *= intensity;

                    data.Player.Illumination.SetIllumination(illuminationLevel, time);
                    illumCount++;
                }

                if (illumCount > 0) {
                    Logger.LogDebug($"{illumCount} lights in range!");
                }
                disposeRaycast();
            }
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
                    float lightRange = light.Light.range;
                    Vector3 lightPos = light != null ? light.transform.position : Vector3.zero;
                    Vector3 directionToPlayer = directions[count];
                    float distance = distances[count];
                    if (playerPos != Vector3.zero && lightPos != Vector3.zero && distance < lightRange) {
                        LightRaycastData data = new LightRaycastData {
                            LightComponent = light,
                            Player = player,
                            Direction = directionToPlayer,
                            Distance = distance,
                            LightPosition = lightPos,
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
            public Vector3 Direction;
            public Vector3 LightPosition;
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
                    LightComponent light = _activeLights[l];
                    Vector3 directionToPlayer = playerPos - light.transform.position;
                    directions[count] = directionToPlayer;
                    count++;
                }
            }
            return new CalcDistanceJob {
                directions = directions,
                distances = new NativeArray<float>(total, Allocator.TempJob),
            };
        }

        private void findActiveLights(List<LightComponent> result)
        {
            result.Clear();
            foreach (LightComponent light in AllLights) {
                if (light != null && light.LightActive) {
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