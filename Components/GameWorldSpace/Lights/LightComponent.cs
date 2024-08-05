using EFT.Interactive;
using EFT.Visual;
using SAIN.Components.PlayerComponentSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class LightComponent : MonoBehaviour
    {
        public bool LightActive =>
            (LampController != null && LampController.Enabled) ||
            (VolumetricLight != null && VolumetricLight.enabled) ||
            (Light != null && Light.enabled);

        public LampController LampController { get; private set; }
        public VolumetricLight VolumetricLight { get; private set; }
        public Light Light { get; private set; }
        //public LightTrigger LightTrigger { get; private set; }

        //public void Init(Light light)
        //{
        //    Light = light;
        //    VolumetricLight = light.GetComponent<VolumetricLight>();
        //}

        private void Awake()
        {
            Light = this.GetComponent<Light>();
            VolumetricLight = this.GetComponent<VolumetricLight>();
        }

        public void Init(LampController lampController)
        {
            LampController = lampController;
        }

        private void Update()
        {
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