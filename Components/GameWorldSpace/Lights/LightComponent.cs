using EFT.Ballistics;
using EFT.Interactive;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.Components
{
    public class LightComponent : MonoBehaviour
    {
        public bool Active { get; private set; }
        public float Angle { get; private set; } = 180f;
        public float Range { get; private set; } = 50f;
        public float Intensity { get; private set; } = 1;
        public LightType Type { get; private set; }

        public Vector3 LightPosition {
            get
            {
                if (_collider != null) {
                    return _collider.transform.position;
                }
                if (LampController != null) {
                    return LampController.transform.position;
                }
                return Light.transform.position;
            }
        }

        public Vector3 LightPointDirection => Light.transform.forward;

        //public bool LightActive =>
        //    (LampController != null && LampController.Enabled) ||
        //    (VolumetricLight != null && VolumetricLight.enabled) ||
        //    (Light != null && Light.enabled);

        public LampController LampController { get; private set; }
        public VolumetricLight VolumetricLight { get; private set; }
        public Light Light { get; private set; }

        private GUIObject _label;

        private void Awake()
        {
            Light = this.GetComponent<Light>();
            VolumetricLight = this.GetComponent<VolumetricLight>();

            Type = Light.type;
            if (Type == LightType.Spot) {
                Angle = Light.spotAngle * 0.9f;
            }
            Intensity = Light.intensity;
            Range = Mathf.Clamp(Light.range * 0.9f, 0f, 100f);

            _label = DebugGizmos.CreateLabel(LightPosition, string.Empty, null, 1, false);
            updateLabel();

            var children = Light.gameObject.GetComponentsInChildren<BallisticCollider>();
            foreach (var child in children) {
                if (child != null) {
                    var name = child.gameObject.name.ToLower();
                    if (name.Contains("glass")) {
                        _collider = child;
                        break;
                    }
                    if (name.Contains("metal")) {
                        _collider = child;
                        break;
                    }
                }
            }
            if (_collider == null) {
                Logger.LogWarning($"Null Ballistics collider");
            }
        }

        private BallisticCollider _collider;

        private void updateLabel()
        {
            _label.StringBuilder.Clear();
            _label.WorldPos = LightPosition;
            _label.Enabled = Active;
            _label.StringBuilder.AppendLine($"{Light.name} : LampController? {LampController != null}");
            _label.StringBuilder.AppendLine($"Layer: {LayerMask.LayerToName(Light.gameObject.layer)}");
            _label.StringBuilder.AppendLine($"Type: {Type}");
            _label.StringBuilder.AppendLine($"Angle: {Angle}");
            _label.StringBuilder.AppendLine($"Intensity: {Intensity}");
            _label.StringBuilder.AppendLine($"Range: {Range}");
        }

        public void Init(LampController lampController)
        {
            LampController = lampController;
        }

        private void Update()
        {
            if (LampController != null) {
                Active = LampController.Enabled;
            }
            else {
                Active = Light != null && Light.enabled;
            }
            updateLabel();
            //if (Light == null) return;
            //
            //this.transform.rotation = Light.transform.rotation;
            //this.transform.localPosition = Light.transform.localPosition;
            //this.transform.position = Light.transform.position;
        }

        private void OnDestroy()
        {
        }
    }
}