using EFT;
using EFT.InventoryLogic;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes
{
    public class ShootDeciderClass : BotBase, IBotClass
    {
        public event Action<Enemy> OnShootEnemy;

        public event Action<Enemy> OnTargetEnemy;

        public event Action OnEndShoot;

        public Enemy LastShotEnemy { get; private set; }

        public ShootDeciderClass(BotComponent bot) : base(bot)
        {
        }

        public void Init()
        {
            Bot.EnemyController.Events.OnEnemyRemoved += checkClearEnemy;
        }

        public void Update()
        {
            checkEndShoot();
        }

        public void Dispose()
        {
            Bot.EnemyController.Events.OnEnemyRemoved -= checkClearEnemy;
        }

        private void checkEndShoot()
        {
            if (_shooting &&
                !BotOwner.ShootData.Shooting) {
                _shooting = false;
                OnEndShoot?.Invoke();
            }
        }

        private void checkClearEnemy(string profileId, Enemy enemy)
        {
            if (LastShotEnemy != null && LastShotEnemy.EnemyProfileId == profileId) {
                LastShotEnemy = null;
            }
        }

        public void CheckAimAndFire()
        {
            var weaponManager = BotOwner.WeaponManager;
            if (weaponManager == null)
                return;

            if (weaponManager.Selector.EquipmentSlot == EquipmentSlot.Holster
                && !weaponManager.HaveBullets
                && !weaponManager.Selector.TryChangeToMain()) {
                selectWeapon();
            }

            if (!Bot.Aim.CanAim)
                return;

            if (_changeAimTimer < Time.time) {
                _changeAimTimer = Time.time + 0.5f;
                Bot.AimDownSightsController.UpdateADSstatus();
            }

            Vector3? target = getTarget(out Enemy enemy);
            //Bot.BotLight.HandleLightForEnemy(enemy);
            if (target != null &&
                enemy != null) {
                OnTargetEnemy?.Invoke(enemy);
                if (aimAtTarget(target.Value) &&
                    weaponManager.HaveBullets && BotOwner.ShootData.Shoot()) {
                    LastShotEnemy = enemy;
                    enemy.EnemyInfo?.SetLastShootTime();
                    _shooting = true;
                    OnShootEnemy?.Invoke(enemy);
                }
            }
        }

        public void AllowUnpauseMove(bool value)
        {
            _shallUnpause = value;
        }

        private bool tryPauseForShoot(bool shallUnpause)
        {
            if (ShallPauseForShoot()) {
                if (_nextPauseMoveTime < Time.time &&
                    !IsMovementPaused) {
                    _nextPauseMoveTime = Time.time + Random.Range(_pauseMoveFrequencyMin, _pauseMoveFrequencyMax);
                    Bot.Mover.PauseMovement(Random.Range(_pauseMoveDurationMin, _pauseMoveDurationMax));
                }
                if (!IsMovementPaused) {
                    BotOwner.AimingData?.LoseTarget();
                    return false;
                }
            }
            else if (IsMovementPaused && _shallUnpause) {
                //BotOwner.Mover.MovementResume();
            }
            return true;
        }

        public bool ShallPauseForShoot()
        {
            float maxPointFireDist = Bot.Info.FileSettings.Shoot.MaxPointFireDistance;
            return
                Bot.Enemy != null &&
                Bot.Enemy.RealDistance > maxPointFireDist &&
                IsAiming;
        }

        public bool IsAiming {
            get
            {
                return BotOwner?.ShootData?.ShootController?.IsAiming == true || BotOwner?.AimingData?.IsReady == true;
            }
        }

        private void selectWeapon()
        {
            EquipmentSlot optimalSlot = findOptimalWeaponForDistance(getDistance());
            if (currentSlot != optimalSlot) {
                tryChangeWeapon(optimalSlot);
            }
        }

        private EquipmentSlot currentSlot => BotOwner.WeaponManager.Selector.EquipmentSlot;

        private void tryChangeWeapon(EquipmentSlot slot)
        {
            if (_nextChangeWeaponTime < Time.time) {
                var selector = BotOwner?.WeaponManager?.Selector;
                if (selector != null) {
                    _nextChangeWeaponTime = Time.time + 1f;
                    switch (slot) {
                        case EquipmentSlot.FirstPrimaryWeapon:
                            selector.TryChangeToMain();
                            break;

                        case EquipmentSlot.SecondPrimaryWeapon:
                            selector.ChangeToSecond();
                            break;

                        case EquipmentSlot.Holster:
                            selector.TryChangeWeapon(true);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private float getDistance()
        {
            if (_nextGetDistTime < Time.time) {
                _nextGetDistTime = Time.time + 0.5f;
                Vector3? target = Bot.CurrentTargetPosition;
                if (target != null) {
                    _lastDistance = Bot.CurrentTargetDistance;
                }
            }
            return _lastDistance;
        }

        private EquipmentSlot findOptimalWeaponForDistance(float distance)
        {
            if (_nextCheckOptimalTime < Time.time) {
                _nextCheckOptimalTime = Time.time + 0.5f;

                var equipment = Bot.PlayerComponent.Equipment;

                float? primaryEngageDist = null;
                var primary = equipment.PrimaryWeapon;
                if (isWeaponDurableEnough(primary)) {
                    primaryEngageDist = primary.EngagementDistance;
                }

                float? secondaryEngageDist = null;
                var secondary = equipment.SecondaryWeapon;
                if (isWeaponDurableEnough(secondary)) {
                    secondaryEngageDist = secondary.EngagementDistance;
                }

                float? holsterEngageDist = null;
                var holster = equipment.HolsterWeapon;
                if (isWeaponDurableEnough(holster)) {
                    holsterEngageDist = holster.EngagementDistance;
                }

                float minDifference = Mathf.Abs(distance - primaryEngageDist ?? 0);
                optimalSlot = EquipmentSlot.FirstPrimaryWeapon;

                float difference = Mathf.Abs(distance - secondaryEngageDist ?? 0);
                if (difference < minDifference) {
                    minDifference = difference;
                    optimalSlot = EquipmentSlot.SecondPrimaryWeapon;
                }

                if (!BotOwner.WeaponManager.HaveBullets) {
                    difference = Mathf.Abs(distance - holsterEngageDist ?? 0);
                    if (difference < minDifference) {
                        minDifference = difference;
                        optimalSlot = EquipmentSlot.Holster;
                    }
                }
            }
            return optimalSlot;
        }

        private bool isWeaponDurableEnough(WeaponInfo info, float min = 0.5f)
        {
            return info != null &&
                info.Durability > min &&
                info.Weapon.ChamberAmmoCount > 0;
        }

        private bool aimAtTarget(Vector3 target)
        {
            var aimData = BotOwner.AimingData;
            //AimStatus aimStatus = Bot.Aim.AimStatus;
            //bool steerComplete = false;

            //if (aimStatus == AimStatus.NoTarget) {
            //    if (!Bot.FriendlyFire.CheckFriendlyFire(target)) {
            //        BotOwner.ShootData.EndShoot();
            //        return false;
            //    }
            //    steerComplete = checkSteerDirection(MIN_ANGLE_TO_START_AIM, target);
            //    if (!steerComplete) {
            //        Bot.Steering.LookToPoint(target, TURN_SPEED_START_AIM);
            //        return false;
            //    }
            //}

            aimData.SetTarget(target);
            //Vector3 aimTarget = aimData.EndTargetPoint;

            //if (!steerComplete &&
            //    !checkSteerDirection(MIN_ANGLE_TO_KEEP_AIMING, aimTarget)) {
            //    Bot.Steering.LookToPoint(aimTarget, TURN_SPEED_AIMING);
            //    return false;
            //}

            aimData.NodeUpdate();

            if (!Bot.FriendlyFire.CheckFriendlyFire(aimData.EndTargetPoint)) {
                BotOwner.ShootData.EndShoot();
                return false;
            }
            if (Bot.NoBushESP.NoBushESPActive) {
                return false;
            }

            return aimData.IsReady;
        }

        private const float MIN_ANGLE_TO_START_AIM = 20f;
        private const float MIN_ANGLE_TO_KEEP_AIMING = 50f;
        private const float TURN_SPEED_START_AIM = 200f;
        private const float TURN_SPEED_AIMING = 250f;

        private bool checkSteerDirection(float maxAngle, Vector3 toPoint)
        {
            Vector3 lookdirection = Bot.LookDirection;
            Vector3 directionToTarget = (toPoint - Bot.Transform.WeaponRoot).normalized;
            float angle = Vector3.Angle(directionToTarget, lookdirection);
            return angle <= maxAngle;
        }

        private Vector3? blindShootTarget(Enemy enemy)
        {
            Vector3? result = null;
            if (!enemy.IsVisible
                    && enemy.Status.HeardRecently
                    && enemy.InLineOfSight) {
                EnemyPlace lastKnown = enemy.KnownPlaces.LastKnownPlace;
                if (lastKnown != null && lastKnown.InLineOfSight(BotOwner.LookSensor._headPoint, LayerMaskClass.HighPolyWithTerrainMask)) {
                    result = lastKnown.Position + Vector3.up + UnityEngine.Random.onUnitSphere;
                }
            }
            return result;
        }

        private Vector3? getTarget(out Enemy enemy)
        {
            enemy = Bot.Enemy;
            Vector3? target = getAimTarget(enemy);
            if (target != null) {
                return target;
            }
            enemy = Bot.LastEnemy;
            target = getAimTarget(enemy);
            if (target != null) {
                return target;
            }
            enemy = null;
            return null;
        }

        private Vector3? getAimTarget(Enemy enemy)
        {
            if (enemy != null &&
                enemy.IsVisible &&
                enemy.CanShoot) {
                Vector3? centerMass = findCenterMassPoint(enemy);
                Vector3? partToShoot = getSAINEnemyPartToShoot(enemy) ?? getEFTEnemyPartToShoot(enemy.EnemyInfo);
                Vector3? modifiedTarget = checkYValue(centerMass, partToShoot);
                return modifiedTarget ?? partToShoot ?? centerMass;
            }
            return null;
        }

        private Vector3? checkYValue(Vector3? centerMass, Vector3? partTarget)
        {
            if (centerMass != null &&
                partTarget != null &&
                centerMass.Value.y < partTarget.Value.y) {
                Vector3 newTarget = partTarget.Value;
                newTarget.y = centerMass.Value.y;
                return new Vector3?(newTarget);
            }
            return null;
        }

        private Vector3? findCenterMassPoint(Enemy enemy)
        {
            if (enemy.IsAI) {
                return null;
            }
            if (!Bot.Info.FileSettings.Aiming.AimCenterMass) {
                return null;
            }
            if (Bot.Info.Profile.IsPMC && GlobalSettingsClass.Instance.Aiming.PMCSAimForHead) {
                return null;
            }
            return enemy.CenterMass;
        }

        private Vector3? getSAINEnemyPartToShoot(Enemy enemy)
        {
            Vector3 value;
            if (enemy.RealDistance < 6f) {
                value = enemy.Shoot.Targets.GetCenterMass() ?? enemy.EnemyInfo.GetCenterPart();
            }
            else {
                value = enemy.Shoot.Targets.GetPointToShoot() ?? enemy.EnemyInfo.GetPartToShoot();
            }
            return new Vector3?(value);
        }

        private Vector3? getEFTEnemyPartToShoot(EnemyInfo enemyInfo)
        {
            Vector3 value;
            if (enemyInfo.Distance < 6f) {
                value = enemyInfo.GetCenterPart();
            }
            else {
                value = enemyInfo.GetPartToShoot();
            }
            return new Vector3?(value);
        }

        private void tryShoot(Enemy enemy)
        {
            if (BotOwner.ShootData.Shoot()) {
                OnShootEnemy?.Invoke(enemy);
                LastShotEnemy = enemy;
                enemy.EnemyInfo?.SetLastShootTime();
                _shooting = true;
            }
        }

        private bool _shooting;
        public bool IsMovementPaused => BotOwner?.Mover.Pause == true;
        private EquipmentSlot optimalSlot;
        private float _nextCheckOptimalTime;
        private float _lastDistance;
        private float _nextGetDistTime;
        private float _nextChangeWeaponTime;
        private float _nextPauseMoveTime;
        private float _pauseMoveFrequencyMin = 2f;
        private float _pauseMoveFrequencyMax = 4f;
        private float _pauseMoveDurationMin = 0.5f;
        private float _pauseMoveDurationMax = 1f;
        private bool _shallUnpause = true;
        private float _changeAimTimer;
    }
}