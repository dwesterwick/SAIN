using EFT;
using EFT.Game.Spawning;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Components
{
    public class GameWorldComponent : MonoBehaviour
    {
        public static GameWorldComponent Instance { get; private set; }
        public GameWorld GameWorld { get; private set; }
        public PlayerSpawnTracker PlayerTracker { get; private set; }
        public SAINBotController SAINBotController { get; private set; }
        public Extract.ExtractFinderComponent ExtractFinder { get; private set; }
        public DoorHandler Doors { get; private set; }
        public LocationClass Location { get; private set; }
        public SpawnPointMarker[] SpawnPointMarkers { get; private set; }

        private void Update()
        {
            Doors.Update();
            Location.Update();
            findSpawnPointMarkers();
            //BotLightTracker.LogDictionaryInfo();
        }

        private void findSpawnPointMarkers()
        {
            if ((SpawnPointMarkers != null) || (Camera.main == null)) {
                return;
            }

            SpawnPointMarkers = UnityEngine.Object.FindObjectsOfType<SpawnPointMarker>();

            if (SAINPlugin.DebugMode)
                Logger.LogInfo($"Found {SpawnPointMarkers.Length} spawn point markers");
        }

        public IEnumerable<Vector3> GetAllSpawnPointPositionsOnNavMesh()
        {
            if (SpawnPointMarkers == null) {
                return Enumerable.Empty<Vector3>();
            }

            List<Vector3> spawnPointPositions = new List<Vector3>();
            foreach (SpawnPointMarker spawnPointMarker in SpawnPointMarkers) {
                // Try to find a point on the NavMesh nearby the spawn point
                Vector3? spawnPointPosition = NavMeshHelpers.GetNearbyNavMeshPoint(spawnPointMarker.Position, 2);
                if (spawnPointPosition.HasValue && !spawnPointPositions.Contains(spawnPointPosition.Value)) {
                    spawnPointPositions.Add(spawnPointPosition.Value);
                }
            }
            return spawnPointPositions;
        }

        private void Awake()
        {
            Instance = this;
            GameWorld = this.GetComponent<GameWorld>();
            if (GameWorld == null) {
                Logger.LogWarning("GameWorld Null, cannot Init SAIN Gameworld! Check 2. Disposing Component...");
                Dispose();
                return;
            }

            GameWorld.OnDispose += Dispose;
            PlayerTracker = new PlayerSpawnTracker(this);
            SAINBotController = this.GetComponent<SAINBotController>();
            ExtractFinder = this.GetOrAddComponent<Extract.ExtractFinderComponent>();

            Doors = new DoorHandler(this);
            Location = new LocationClass(this);

            Doors.Init();
            Location.Init();
        }

        public void Dispose()
        {
            Instance = null;
            try {
                PlayerTracker?.Dispose();
                Doors?.Dispose();
                Location?.Dispose();
            }
            catch (Exception e) {
                Logger.LogError($"Dispose GameWorld Component Class Error: {e}");
            }

            try {
                ComponentHelpers.DestroyComponent(SAINBotController);
            }
            catch (Exception e) {
                Logger.LogError($"Dispose GameWorld SubComponent Error: {e}");
            }

            Instance = null;
            GameWorld.OnDispose -= Dispose;
            Destroy(this);
            //Logger.LogDebug("SAIN GameWorld Destroyed.");
        }
    }
}