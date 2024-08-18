using SAIN.Components.BotController;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class VisionRaycastJob : SAINControllerBase
    {
        private NativeArray<RaycastHit> _hits;
        private NativeArray<RaycastCommand> _commands;
        private JobHandle _handle;
        private const int RAYCAST_CHECKS = 3;
        private readonly LayerMask _LOSMask = LayerMaskClass.HighPolyWithTerrainMask;
        private readonly LayerMask _VisionMask = LayerMaskClass.AI;
        private readonly LayerMask _ShootMask = LayerMaskClass.HighPolyWithTerrainMask;
        private int _partCount = -1;
        private readonly List<EBodyPartColliderType> _colliderTypes = new List<EBodyPartColliderType>();
        private readonly List<Vector3> _castPoints = new List<Vector3>();
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private BotDictionary _bots;
        private bool _hasJobToComplete = false;

        public VisionRaycastJob(SAINBotController botcontroller) : base(botcontroller)
        {
        }

        public void Init()
        {
            _bots = BotController.BotSpawnController.BotDictionary;
        }

        public void Update()
        {
            completeJob();
            if (BotController.BotGame?.Status == EFT.GameStatus.Stopping) {
                return;
            }
            setupJob();
        }

        private void setupJob()
        {
            if (_hasJobToComplete) {
                return;
            }

            findEnemies(_bots, _enemies);
            int enemyCount = _enemies.Count;
            if (enemyCount == 0) {
                return;
            }

            if (_partCount < 0) {
                _partCount = _enemies[0].Vision.VisionChecker.EnemyParts.PartsArray.Length;
            }
            int partCount = _partCount;

            int totalRaycasts = enemyCount * partCount * RAYCAST_CHECKS;
            _hits = new NativeArray<RaycastHit>(totalRaycasts, Allocator.TempJob);
            _commands = new NativeArray<RaycastCommand>(totalRaycasts, Allocator.TempJob);
            createCommands(_enemies, _commands, enemyCount, partCount);
            _handle = RaycastCommand.ScheduleBatch(_commands, _hits, 3);
            _hasJobToComplete = true;
        }

        private void completeJob()
        {
            if (!_hasJobToComplete) {
                return;
            }
            _handle.Complete();
            analyzeHits(_enemies, _hits, _enemies.Count, _partCount);
            _commands.Dispose();
            _hits.Dispose();
            _hasJobToComplete = false;
        }

        public void Dispose()
        {
            completeJob();
        }

        private void createCommands(List<Enemy> enemies, NativeArray<RaycastCommand> raycastCommands, int enemyCount, int partCount)
        {
            _colliderTypes.Clear();
            _castPoints.Clear();

            int commands = 0;
            for (int i = 0; i < enemyCount; i++) {
                Enemy enemy = _enemies[i];
                PersonTransformClass transform = enemy.Bot.Transform;
                Vector3 eyePosition = transform.EyePosition;
                Vector3 weaponFirePort = transform.WeaponFirePort;
                EnemyBodyPart[] parts = enemy.Vision.VisionChecker.EnemyParts.PartsArray;
                Dictionary<EBodyPart, float> partDistances = enemy.EnemyPlayerData.DistanceData.BodyPartDistances;

                for (int j = 0; j < partCount; j++) {
                    EnemyBodyPart part = parts[j];

                    BodyPartRaycast raycastData = part.GetRaycast(eyePosition, float.MaxValue, ERaycastCheck.LineofSight);
                    raycastCommands[commands] = createCommand(raycastData, partDistances[raycastData.PartType], eyePosition, _LOSMask);
                    commands++;

                    raycastData = part.GetRaycast(eyePosition, float.MaxValue, ERaycastCheck.Shoot);
                    raycastCommands[commands] = createCommand(raycastData, partDistances[raycastData.PartType], eyePosition, _VisionMask);
                    commands++;

                    raycastData = part.GetRaycast(eyePosition, float.MaxValue, ERaycastCheck.Vision);
                    raycastCommands[commands] = createCommand(raycastData, partDistances[raycastData.PartType], weaponFirePort, _ShootMask);
                    commands++;
                }
            }
            Logger.LogDebug($"Scheduled [{enemyCount * partCount * 3}] raycasts");
        }

        private RaycastCommand createCommand(BodyPartRaycast bodyPartRaycast, float partDistance, Vector3 origin, LayerMask mask)
        {
            _colliderTypes.Add(bodyPartRaycast.ColliderType);
            Vector3 castPoint = bodyPartRaycast.CastPoint;
            _castPoints.Add(castPoint);
            Vector3 eyeDir = castPoint - origin;
            return new RaycastCommand(origin, castPoint - origin, partDistance, mask);
        }

        private void analyzeHits(List<Enemy> enemies, NativeArray<RaycastHit> raycastHits, int enemyCount, int partCount)
        {
            float time = Time.time;
            int hits = 0;
            for (int i = 0; i < enemyCount; i++) {
                Enemy enemy = _enemies[i];
                EnemyVisionChecker visionChecker = enemy.Vision.VisionChecker;
                EnemyBodyPart[] parts = visionChecker.EnemyParts.PartsArray;
                visionChecker.NextCheckLOSTime = time + (enemy.IsAI ? 0.1f : 0.05f);
                enemy.Bot.Vision.TimeLastCheckedLOS = time;

                for (int j = 0; j < partCount; j++) {
                    EnemyBodyPart part = parts[j];
                    part.SetLineOfSight(_castPoints[hits], _colliderTypes[hits], raycastHits[hits], ERaycastCheck.LineofSight, time);
                    hits++;
                    part.SetLineOfSight(_castPoints[hits], _colliderTypes[hits], raycastHits[hits], ERaycastCheck.Vision, time);
                    hits++;
                    part.SetLineOfSight(_castPoints[hits], _colliderTypes[hits], raycastHits[hits], ERaycastCheck.Shoot, time);
                    hits++;
                }
            }
        }

        private static void findEnemies(BotDictionary bots, List<Enemy> result)
        {
            result.Clear();
            float time = Time.time;
            foreach (var bot in bots.Values) {
                if (bot == null || !bot.BotActive) continue;
                if (bot.Vision.TimeSinceCheckedLOS < 0.05f) continue;
                foreach (var enemy in bot.EnemyController.Enemies.Values) {
                    if (!enemy.WasValid) continue;
                    var visionChecker = enemy.Vision.VisionChecker;
                    if (enemy.RealDistance > visionChecker.AIVisionRangeLimit()) continue;
                    if (visionChecker.NextCheckLOSTime < time) {
                        result.Add(enemy);
                    }
                }
            }
        }
    }
}