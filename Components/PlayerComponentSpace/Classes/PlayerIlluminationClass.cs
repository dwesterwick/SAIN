using EFT;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace
{
    public class PlayerIlluminationClass : PlayerComponentBase
    {
        public event Action<bool> OnPlayerIlluminationChanged;

        public bool Illuminated { get; private set; }
        public float Level { get; private set; }
        public float TimeSinceIlluminated => Time.time - _timeLastIlluminated;

        private const float ILLUMINATED_BUFFER_PERIOD = 0.25f;

        private float _timeLastIlluminated;
        private float _resetLevelTime;

        public PlayerIlluminationClass(PlayerComponent playerComponent) : base(playerComponent)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            checkIllumChanged();
        }

        public void Dispose()
        {
        }

        private void checkIllumChanged()
        {
            bool wasIllum = Illuminated;
            Illuminated = TimeSinceIlluminated < ILLUMINATED_BUFFER_PERIOD;
            if (wasIllum != Illuminated) {
                OnPlayerIlluminationChanged?.Invoke(Illuminated);
            }
        }

        public void SetIllumination(bool value, float level, LightTrigger trigger, float sqrMagnitude)
        {
        }

        public void SetIllumination(float level, float time)
        {
            if (_resetLevelTime < time) {
                _resetLevelTime = time + 0.05f;
                Level = 0;
            }
            if (level > Level) {
                Level = level;
            }
            _timeLastIlluminated = time;
        }
    }
}