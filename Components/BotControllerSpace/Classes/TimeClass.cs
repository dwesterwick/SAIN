using SAIN.Helpers;
using System;
using System.Text;
using UnityEngine;
using static EFT.SpeedTree.TreeWind;

namespace SAIN.Components.BotController
{
    public class TimeClass : SAINControllerBase
    {
        public DateTime GameDateTime { get; private set; }
        public float TimeVisionDistanceModifier { get; private set; } = 1f;
        public float TimeGainSightModifier { get; private set; } = 1f;
        public TimeSettings Settings { get; private set; }

        private float _visTime = 0f;
        private float _nextTestTime;

        private float MIN_VISION_ANGLE_NIGHT = 20f;
        private float MAX_VISION_ANGLE_DAY = 180f;
        private float NIGHT_MAX_VISION_SPEED_MULTI = 4f;

        public struct TimeSettings
        {
            public ETimeOfDay ETimeOfDay;
            public float TimeVisionDistanceModifier;
            public float TimeGainSightModifier;
            public float VisibilityPercent;
            public float MaxFieldOfViewAngle;
        }

        public TimeClass(SAINBotController botController) : base(botController)
        {
        }

        public void Update()
        {
            var bots = Bots;
            if (bots == null || bots.Count == 0) {
                return;
            }
            if (_visTime < Time.time) {
                _visTime = Time.time + 5f;
                Settings = calculateTimeSettings();
            }
        }

        private float calcGainSightMulti(float distMod0_1)
        {
            float inverse = 1f - distMod0_1;
            float result = Mathf.Lerp(1f, NIGHT_MAX_VISION_SPEED_MULTI, inverse);
            return result;
        }

        private TimeSettings calculateTimeSettings()
        {
            if (SAINPlugin.DebugMode) {
                testTime();
            }
            float time = calcTime();
            ETimeOfDay timeOfDay = getTimeEnum(time);
            float timemodifier = getModifier(time, timeOfDay, out float visibilityPercent);
            return new TimeSettings {
                ETimeOfDay = timeOfDay,
                VisibilityPercent = visibilityPercent,
                TimeVisionDistanceModifier = timemodifier,
                TimeGainSightModifier = calcGainSightMulti(timemodifier),
                MaxFieldOfViewAngle = Mathf.Lerp(MIN_VISION_ANGLE_NIGHT, MAX_VISION_ANGLE_DAY, timemodifier),
            };
        }

        private void testTime()
        {
            if (_nextTestTime < Time.time) {
                StringBuilder builder = new StringBuilder();
                _nextTestTime = Time.time + 240f;
                for (int i = 0; i < 24; i++) {
                    var timeOFDay = getTimeEnum(i + 1);
                    float test = getModifier(i + 1, timeOFDay, out _);
                    builder.AppendLine($"{i + 1} {test} {timeOFDay}");
                }
                Logger.LogInfo(builder.ToString());
            }
        }

        private float calcTime()
        {
            var nightSettings = SAINPlugin.LoadedPreset.GlobalSettings.Look;
            GameDateTime = BotController.Bots.PickRandom().Value.BotOwner.GameDateTime.Calculate();
            float minutes = GameDateTime.Minute / 59f;
            float time = GameDateTime.Hour + minutes;
            time = time.Round100();
            return time;
        }

        private static float getModifier(float time, ETimeOfDay timeOfDay, out float percentage)
        {
            var nightSettings = SAINPlugin.LoadedPreset.GlobalSettings.Look.Time;
            float max = 1f;
            bool snowActive = GameWorldComponent.Instance.Location.WinterActive;
            float min = snowActive ? nightSettings.NightTimeVisionModifierSnow : nightSettings.NightTimeVisionModifier;
            float ratio;
            float difference;
            float current;
            switch (timeOfDay) {
                default:
                    percentage = 100f;
                    return max;

                case ETimeOfDay.Night:
                    percentage = 0f;
                    return min;

                case ETimeOfDay.Dawn:
                    difference = nightSettings.HourDawnEnd - nightSettings.HourDawnStart;
                    current = time - nightSettings.HourDawnStart;
                    ratio = current / difference;
                    break;

                case ETimeOfDay.Dusk:
                    difference = nightSettings.HourDuskEnd - nightSettings.HourDuskStart;
                    current = time - nightSettings.HourDuskStart;
                    ratio = 1f - current / difference;
                    break;
            }
            percentage = ratio * 100f;
            float result = Mathf.Lerp(min, max, ratio);
            return result;
        }

        private static ETimeOfDay getTimeEnum(float time)
        {
            var nightSettings = SAINPlugin.LoadedPreset.GlobalSettings.Look.Time;
            if (time <= nightSettings.HourDuskStart &&
                time >= nightSettings.HourDawnEnd) {
                return ETimeOfDay.Day;
            }
            if (time >= nightSettings.HourDuskEnd ||
                time <= nightSettings.HourDawnStart) {
                return ETimeOfDay.Night;
            }
            if (time < nightSettings.HourDawnEnd) {
                return ETimeOfDay.Dawn;
            }
            return ETimeOfDay.Dusk;
        }
    }
}