using SAIN.Components;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINAILimit : BotBase, IBotClass
    {
        public event Action<AILimitSetting> OnAILimitChanged;

        public AILimitSetting CurrentAILimit { get; private set; }
        public float ClosestPlayerDistance { get; private set; }

        private float _checkDistanceTime;
        private float _frequency = 3f;
        private float _farDistance = 200f;
        private float _veryFarDistance = 300f;
        private float _narniaDistance = 400f;

        public SAINAILimit(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(UpdatePresetSettings);
        }

        public void Update()
        {
            checkAILimit();
        }

        public void Dispose()
        {
        }

        private void checkAILimit()
        {
            AILimitSetting lastLimit = CurrentAILimit;
            if (Bot.EnemyController.ActiveHumanEnemy) {
                CurrentAILimit = AILimitSetting.None;
                ClosestPlayerDistance = -1f;
            }
            else if (_checkDistanceTime < Time.time) {
                _checkDistanceTime = Time.time + _frequency;
                var gameWorld = GameWorldComponent.Instance;
                if (gameWorld != null &&
                    gameWorld.PlayerTracker.FindClosestHumanPlayer(out float closestMagnitude, PlayerComponent) != null) {
                    CurrentAILimit = checkDistances(closestMagnitude);
                    ClosestPlayerDistance = closestMagnitude;
                }
            }
            if (lastLimit != CurrentAILimit) {
                OnAILimitChanged?.Invoke(CurrentAILimit);
            }
        }

        private AILimitSetting checkDistances(float closestPlayerSqrMag)
        {
            if (closestPlayerSqrMag < _farDistance) {
                return AILimitSetting.None;
            }
            if (closestPlayerSqrMag < _veryFarDistance) {
                return AILimitSetting.Far;
            }
            if (closestPlayerSqrMag < _narniaDistance) {
                return AILimitSetting.VeryFar;
            }
            return AILimitSetting.Narnia;
        }

        protected void UpdatePresetSettings(SAINPresetClass preset)
        {
            var aiLimit = GlobalSettingsClass.Instance.General.AILimit;
            _frequency = aiLimit.AILimitUpdateFrequency;
            _farDistance = aiLimit.AILimitRanges[AILimitSetting.Far];
            _veryFarDistance = aiLimit.AILimitRanges[AILimitSetting.VeryFar];
            _narniaDistance = aiLimit.AILimitRanges[AILimitSetting.Narnia];
        }
    }
}