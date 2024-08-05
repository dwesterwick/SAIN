using EFT;
using EFT.Interactive;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class BotLightTracker
    {
        static BotLightTracker()
        {
            GameWorld.OnDispose += dispose;
        }

        public static void AddLight(Light light, LampController lampController = null)
        {
            if (_trackedLights.ContainsKey(light)) {
                if (lampController != null)
                    _trackedLights[light].Init(lampController);
                return;
            }
            if (light.range < 0.01f || light.intensity < 0.1f) {
                //return;
            }

            //var gameObject = new GameObject($"LightComp_{_count++}");
            //gameObject.layer = LayerMaskClass.TriggersLayer;

            var component = light.gameObject.AddComponent<LightComponent>();
            if (lampController != null) {
                component.Init(lampController);
            }

            SAINBotController.Instance?.LightFinder?.AddLight(component);

            _trackedLights.Add(light, component);
        }

        public static void GetLights(List<LightComponent> result)
        {
            foreach (var light in _trackedLights.Values) {
                if (light != null) {
                    result.Add(light);
                }
            }
        }

        private static void dispose()
        {
            foreach (var light in _trackedLights) {
                GameObject.Destroy(light.Value);
            }
            _trackedLights.Clear();
        }

        public static void LogDictionaryInfo()
        {
            if (_nextlogTime > Time.time) {
                return;
            }
            _nextlogTime = Time.time + 30f;
            Logger.LogDebug($"[{_trackedLights.Count}] lights being tracked currently.");
        }

        private static float _nextlogTime;

        private static readonly Dictionary<Light, LightComponent> _trackedLights = new Dictionary<Light, LightComponent>();
    }
}