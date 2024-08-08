using Comfort.Common;
using EFT;
using SAIN.Components.BotComponentSpace.Classes.EnemyClasses;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System;
using UnityEngine;

namespace SAIN.Components
{
    public class GrenadeController : SAINControllerBase
    {
        public event Action<Grenade, float> OnGrenadeCollision;

        public event Action<Grenade, Vector3, string> OnGrenadeThrown;

        //public event Action<Vector3, string, bool, float, float> OnGrenadeExploded;

        public GrenadeController(SAINBotController controller) : base(controller)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public void Subscribe(BotEventHandler eventHandler)
        {
            eventHandler.OnGrenadeThrow += GrenadeThrown;
            eventHandler.OnGrenadeExplosive += GrenadeExplosion;
        }

        public void UnSubscribe(BotEventHandler eventHandler)
        {
            eventHandler.OnGrenadeThrow -= GrenadeThrown;
            eventHandler.OnGrenadeExplosive -= GrenadeExplosion;
        }

        public void GrenadeCollided(Grenade grenade, float maxRange)
        {
            OnGrenadeCollision?.Invoke(grenade, maxRange);
        }

        private void GrenadeExplosion(Vector3 explosionPosition, string playerProfileID, bool isSmoke, float smokeRadius, float smokeLifeTime)
        {
            if (!Singleton<BotEventHandler>.Instantiated || playerProfileID == null) {
                return;
            }
            Player player = GameWorldInfo.GetAlivePlayer(playerProfileID);
            if (player != null) {
                if (!isSmoke) {
                    registerGrenadeExplosionForSAINBots(explosionPosition, player, playerProfileID, 200f);
                }
                else {
                    registerGrenadeExplosionForSAINBots(explosionPosition, player, playerProfileID, 50f);

                    float radius = smokeRadius * HelpersGClass.SMOKE_GRENADE_RADIUS_COEF;
                    Vector3 position = player.Position;

                    if (BotController.DefaultController != null)
                        foreach (var keyValuePair in BotController.DefaultController.Groups())
                            foreach (BotsGroup botGroupClass in keyValuePair.Value.GetGroups(true))
                                botGroupClass.AddSmokePlace(explosionPosition, smokeLifeTime, radius, position);
                }
            }
        }

        private void registerGrenadeExplosionForSAINBots(Vector3 explosionPosition, Player player, string playerProfileID, float range)
        {
            // Play a sound with the input range.
            Singleton<BotEventHandler>.Instance?.PlaySound(player, explosionPosition, range, AISoundType.gun);

            // We dont want bots to think the grenade explosion was a place they heard an enemy, so set this manually.
            foreach (var bot in Bots.Values) {
                if (bot?.BotActive == true) {
                    float distance = (bot.Position - explosionPosition).magnitude;
                    if (distance < range) {
                        Enemy enemy = bot.EnemyController.GetEnemy(playerProfileID, true);
                        if (enemy != null) {
                            float dispersion = distance / 10f;
                            Vector3 random = UnityEngine.Random.onUnitSphere * dispersion;
                            random.y = 0;
                            Vector3 estimatedThrowPosition = enemy.EnemyPosition + random;

                            HearingReport report = new HearingReport {
                                position = estimatedThrowPosition,
                                soundType = SAINSoundType.GrenadeExplosion,
                                placeType = EEnemyPlaceType.Hearing,
                                isDanger = distance < 100f || enemy.InLineOfSight,
                                shallReportToSquad = true,
                            };
                            enemy.Hearing.SetHeard(report);
                        }
                    }
                }
            }
        }

        private void GrenadeThrown(Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            if (grenade == null) {
                return;
            }

            Player player = GameWorldInfo.GetAlivePlayer(grenade.ProfileId);
            if (player == null) {
                Logger.LogError($"Player Null from ID {grenade.ProfileId}");
                return;
            }
            if (!player.HealthController.IsAlive) {
                return;
            }

            Vector3 dangerPoint = Vector.DangerPoint(position, force, mass);
            Singleton<BotEventHandler>.Instance?.PlaySound(player, grenade.transform.position, 20f, AISoundType.gun);
            grenade.gameObject.AddComponent<GrenadeVelocityTracker>();
            OnGrenadeThrown?.Invoke(grenade, dangerPoint, grenade.ProfileId);
        }
    }
}