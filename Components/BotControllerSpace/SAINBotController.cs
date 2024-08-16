using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using SAIN.BotController.Classes;
using SAIN.BotControllerSpace.Classes;
using SAIN.Components.BotController;
using SAIN.Components.BotController.PeacefulActions;
using SAIN.Components.BotControllerSpace.Classes;
using SAIN.Helpers;
using SAIN.Layers;
using SAIN.SAINComponent;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components
{
    public class SAINBotController : MonoBehaviour
    {
        public static SAINBotController Instance { get; private set; }

        public BotDictionary Bots => BotSpawnController.Bots;

        public GameWorld GameWorld => SAINGameWorld.GameWorld;

        public IBotGame BotGame => Singleton<IBotGame>.Instance;

        public BotEventHandler BotEventHandler {
            get
            {
                if (_eventHandler == null) {
                    _eventHandler = Singleton<BotEventHandler>.Instance;
                    if (_eventHandler != null) {
                        GrenadeController.Subscribe(_eventHandler);
                    }
                }
                return _eventHandler;
            }
        }

        private BotEventHandler _eventHandler;

        public GameWorldComponent SAINGameWorld { get; private set; }
        public BotsController DefaultController { get; set; }

        public BotSpawner BotSpawner {
            get
            {
                return _spawner;
            }
            set
            {
                BotSpawnController.Subscribe(value);
                _spawner = value;
            }
        }

        private BotSpawner _spawner;

        public LightFinder LightFinder { get; private set; }
        public GrenadeController GrenadeController { get; private set; }
        public BotJobsClass BotJobs { get; private set; }
        public BotExtractManager BotExtractManager { get; private set; }
        public TimeClass TimeVision { get; private set; }
        public BotController.SAINWeatherClass WeatherVision { get; private set; }
        public BotSpawnController BotSpawnController { get; private set; }
        public BotSquads BotSquads { get; private set; }
        public BotHearingClass BotHearing { get; private set; }
        public BotPeacefulActionController PeacefulActions { get; private set; }

        private readonly Dictionary<BotComponent, GUIObject> _debugObjects = new Dictionary<BotComponent, GUIObject>();
        public List<Player> DeadBots { get; private set; } = new List<Player>();
        public List<BotDeathObject> DeathObstacles { get; private set; } = new List<BotDeathObject>();
        private readonly List<int> IndexToRemove = new List<int>();
        public readonly List<string> Groups = new List<string>();

        public void BotChangedWeapon(BotOwner botOwner, IFirearmHandsController firearmController)
        {
        }

        public void PlayerEnviromentChanged(string profileID, IndoorTrigger trigger)
        {
            SAINGameWorld.PlayerTracker.GetPlayerComponent(profileID)?.AIData.PlayerLocation.UpdateEnvironment(trigger);
        }

        private void Awake()
        {
            Instance = this;
            SAINGameWorld = this.GetComponent<GameWorldComponent>();
            BotSpawnController = new BotSpawnController(this);
            BotExtractManager = new BotExtractManager(this);
            TimeVision = new TimeClass(this);
            WeatherVision = new BotController.SAINWeatherClass(this);
            BotSquads = new BotSquads(this);
            BotHearing = new BotHearingClass(this);
            PeacefulActions = new BotPeacefulActionController(this);
            BotJobs = new BotJobsClass(this);
            GrenadeController = new GrenadeController(this);
            LightFinder = new LightFinder(this);
            GameWorld.OnDispose += Dispose;
        }

        private void Start()
        {
            PeacefulActions.Init();
        }

        private void Update()
        {
            LightFinder.Update();

            if (BotGame == null ||
                BotGame.Status == GameStatus.Stopping) {
                return;
            }

            BotSquads.Update();
            BotSpawnController.Update();
            BotExtractManager.Update();
            TimeVision.Update();
            WeatherVision.Update();
            BotJobs.Update();
            PeacefulActions.Update();
        }

        private void showBotInfoDebug()
        {
            foreach (var bot in Bots.Values) {
                if (bot != null && !_debugObjects.ContainsKey(bot)) {
                    GUIObject obj = DebugGizmos.CreateLabel(bot.Position, "");
                    _debugObjects.Add(bot, obj);
                }
            }
            foreach (var obj in _debugObjects) {
                if (obj.Value != null) {
                    obj.Value.WorldPos = obj.Key.Position;
                    obj.Value.StringBuilder.Clear();
                    DebugOverlay.AddBaseInfo(obj.Key, obj.Key.BotOwner, obj.Value.StringBuilder);
                }
            }
        }

        public void BotDeath(BotOwner bot)
        {
            if (bot?.GetPlayer != null && bot.IsDead) {
                DeadBots.Add(bot.GetPlayer);
            }
        }

        public void AddNavObstacles()
        {
            if (DeadBots.Count > 0) {
                const float ObstacleRadius = 1.5f;

                for (int i = 0; i < DeadBots.Count; i++) {
                    var bot = DeadBots[i];
                    if (bot == null || bot.GetPlayer == null) {
                        IndexToRemove.Add(i);
                        continue;
                    }
                    bool enableObstacle = true;
                    Collider[] players = Physics.OverlapSphere(bot.Position, ObstacleRadius, LayerMaskClass.PlayerMask);
                    foreach (var p in players) {
                        if (p == null) continue;
                        if (p.TryGetComponent<Player>(out var player)) {
                            if (player.IsAI && player.HealthController.IsAlive) {
                                enableObstacle = false;
                                break;
                            }
                        }
                    }
                    if (enableObstacle) {
                        if (bot != null && bot.GetPlayer != null) {
                            var obstacle = new BotDeathObject(bot);
                            obstacle.Activate(ObstacleRadius);
                            DeathObstacles.Add(obstacle);
                        }
                        IndexToRemove.Add(i);
                    }
                }

                foreach (var index in IndexToRemove) {
                    DeadBots.RemoveAt(index);
                }

                IndexToRemove.Clear();
            }
        }

        private void UpdateObstacles()
        {
            if (DeathObstacles.Count > 0) {
                for (int i = 0; i < DeathObstacles.Count; i++) {
                    var obstacle = DeathObstacles[i];
                    if (obstacle?.TimeSinceCreated > 30f) {
                        obstacle?.Dispose();
                        IndexToRemove.Add(i);
                    }
                }

                foreach (var index in IndexToRemove) {
                    DeathObstacles.RemoveAt(index);
                }

                IndexToRemove.Clear();
            }
        }

        private void OnDestroy()
        {
        }

        public void Dispose()
        {
            try {
                GameWorld.OnDispose -= Dispose;
                StopAllCoroutines();
                BotJobs?.Dispose();
                BotSpawnController?.UnSubscribe();
                PeacefulActions?.Dispose();
                LightFinder?.Dispose();

                if (BotEventHandler != null) {
                    GrenadeController?.UnSubscribe(BotEventHandler);
                }
            }
            catch (Exception ex) {
                Logger.LogError($"Dispose SAIN BotController Error: {ex}");
            }

            try {
                if (Bots != null && Bots.Count > 0) {
                    foreach (var bot in Bots.Values) {
                        bot?.Dispose();
                    }
                }
                Bots?.Clear();
            }
            catch (Exception ex) {
                Logger.LogError($"Dispose All Bots Error: {ex}");
            }

            Destroy(this);
        }

        public bool GetSAIN(BotOwner botOwner, out BotComponent bot)
        {
            StringBuilder debugString = null;
            bot = BotSpawnController.GetSAIN(botOwner, debugString);
            return bot != null;
        }
    }

    public class BotDeathObject
    {
        public BotDeathObject(Player player)
        {
            Player = player;
            NavMeshObstacle = player.gameObject.AddComponent<NavMeshObstacle>();
            NavMeshObstacle.carving = false;
            NavMeshObstacle.enabled = false;
            Position = player.Position;
            TimeCreated = Time.time;
        }

        public void Activate(float radius = 2f)
        {
            if (NavMeshObstacle != null) {
                NavMeshObstacle.enabled = true;
                NavMeshObstacle.carving = true;
                NavMeshObstacle.radius = radius;
            }
        }

        public void Dispose()
        {
            if (NavMeshObstacle != null) {
                NavMeshObstacle.carving = false;
                NavMeshObstacle.enabled = false;
                GameObject.Destroy(NavMeshObstacle);
            }
        }

        public NavMeshObstacle NavMeshObstacle { get; private set; }
        public Player Player { get; private set; }
        public Vector3 Position { get; private set; }
        public float TimeCreated { get; private set; }
        public float TimeSinceCreated => Time.time - TimeCreated;
        public bool ObstacleActive => NavMeshObstacle.carving;
    }
}