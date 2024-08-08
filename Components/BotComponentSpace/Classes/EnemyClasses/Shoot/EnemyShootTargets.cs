using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyShootTargets : EnemyBase, IBotEnemyClass
    {
        public EnemyPartDataClass SelectedPart { get; private set; }
        public EnemyPartDataClass LastPart { get; private set; }
        public float TimeSinceChangedPart => Time.time - _lastChangePartTime;
        public bool CanShootHead { get; private set; }

        public EnemyShootTargets(Enemy enemy) : base(enemy)
        {
        }

        public void Init()
        {
            Enemy.Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
            Enemy.Bot.Shoot.OnShootEnemy += checkChangePart;
            Enemy.Bot.Shoot.OnEndShoot += executePartChange;
            addEnemyParts();
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            Enemy.Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;
            Enemy.Bot.Shoot.OnShootEnemy -= checkChangePart;
            Enemy.Bot.Shoot.OnEndShoot -= executePartChange;
        }

        public void OnEnemyKnownChanged(bool known, Enemy enemy)
        {
        }

        private void checkChangePart(Enemy enemy)
        {
            if (enemy != null && enemy.IsSame(Enemy) && _timeCanChange < Time.time) {
                _willChangePart = true;
                _timeCanChange = Time.time + MAX_CHANGE_FREQ;
            }
        }

        private float _timeCanChange;
        private const float MAX_CHANGE_FREQ = 0.5f;

        private void executePartChange()
        {
            if (_willChangePart) {
                _willChangePart = false;
                _changePart = true;
            }
        }

        private void checkChangePart()
        {
            if (SelectedPart != null &&
                !_changePart &&
                TimeSinceChangedPart < CHANGE_PART_FREQ) {
                return;
            }
            _changePart = false;
            changeSelectedPart();
        }

        private EnemyPartDataClass changeSelectedPart()
        {
            var enemyParts = Enemy.Vision.VisionChecker.EnemyParts.Parts;

            for (int i = 0; i < CHANGE_PART_ITERATION_ATTEMPTS; i++) {
                EBodyPart randomPart = _normalSelector.GetRandomOption();
                if (enemyParts.TryGetValue(randomPart, out EnemyPartDataClass enemyPartData) &&
                    enemyPartData?.CanShoot == true &&
                    enemyPartData.RaycastResults[ERaycastCheck.Shoot].LastSuccessPoint != null) {
                    if (SelectedPart != null &&
                        SelectedPart.BodyPart != randomPart) {
                        LastPart = SelectedPart;
                    }

                    //if (!Enemy.IsAI)
                    //    Logger.LogDebug($"Selected [{randomPart}] body part to shoot after [{i}] iterations through random selector.");

                    SelectedPart = enemyPartData;
                    return enemyPartData;
                }
            }
            return null;
        }

        private const float CHANGE_PART_FREQ = 1.5f;
        private const int CHANGE_PART_ITERATION_ATTEMPTS = 10;

        private bool _willChangePart = false;
        private bool _changePart = false;

        public Vector3? GetPointToShoot()
        {
            checkChangePart();

            var partToShoot = SelectedPart ?? changeSelectedPart();

            if (partToShoot == null) {
                Vector3? point = null;
                foreach (var part in Enemy.Vision.VisionChecker.EnemyParts.Parts.Values) {
                    if (part?.CanShoot == true) {
                        point = part.RaycastResults[ERaycastCheck.Shoot].LastSuccessPoint;
                        if (point != null) {
                            break;
                        }
                    }
                }
                return point;
            }
            else {
                return partToShoot.RaycastResults[ERaycastCheck.Shoot].LastSuccessPoint;
            }
        }

        public Vector3? GetCenterMass()
        {
            Vector3? point = null;
            Dictionary<EBodyPart, EnemyPartDataClass> parts = Enemy.Vision.VisionChecker.EnemyParts.Parts;
            if (parts.TryGetValue(EBodyPart.Chest, out EnemyPartDataClass chest)) {
                if (chest?.CanShoot == true) {
                    point = chest.RaycastResults[ERaycastCheck.Shoot].LastSuccessPoint;
                    if (point != null) {
                        return point;
                    }
                }
            }
            if (parts.TryGetValue(EBodyPart.Stomach, out EnemyPartDataClass stomach)) {
                if (stomach?.CanShoot == true) {
                    point = stomach.RaycastResults[ERaycastCheck.Shoot].LastSuccessPoint;
                    if (point != null) {
                        return point;
                    }
                }
            }
            return point;
        }

        private void addEnemyParts()
        {
            CanShootHead = _normalWeights._headWeight > 0;
            if (CanShootHead) {
                _normalSelector.AddOption(EBodyPart.Head, _normalWeights._headWeight);
            }
            _normalSelector.AddOption(EBodyPart.Chest, _normalWeights._chestWeight);
            _normalSelector.AddOption(EBodyPart.Stomach, _normalWeights._stomachWeight);
            _normalSelector.AddOption(EBodyPart.LeftArm, _normalWeights._leftArmWeight);
            _normalSelector.AddOption(EBodyPart.RightArm, _normalWeights._rightArmWeight);
            _normalSelector.AddOption(EBodyPart.LeftLeg, _normalWeights._leftLegWeight);
            _normalSelector.AddOption(EBodyPart.RightLeg, _normalWeights._rightLegWeight);
            //_selector.Test();
        }

        private int _headWeight = 0;
        private int _chestWeight = 10;
        private int _stomachWeight = 6;
        private int _leftArmWeight = 3;
        private int _rightArmWeight = 3;
        private int _leftLegWeight = 4;
        private int _rightLegWeight = 4;

        private PartWeights _normalWeights = new PartWeights {
            _headWeight = 0,
            _chestWeight = 10,
            _stomachWeight = 6,
            _leftArmWeight = 3,
            _rightArmWeight = 3,
            _leftLegWeight = 4,
            _rightLegWeight = 4,
        };

        private float _lastChangePartTime;

        private readonly WeightedRandomSelector<EBodyPart> _normalSelector = new WeightedRandomSelector<EBodyPart>();

        private struct PartWeights
        {
            public int _headWeight;
            public int _chestWeight;
            public int _stomachWeight;
            public int _leftArmWeight;
            public int _rightArmWeight;
            public int _leftLegWeight;
            public int _rightLegWeight;
        }
    }
}