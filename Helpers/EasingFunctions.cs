using System;
using UnityEngine;

namespace SAIN.Helpers
{
    internal class EasingFunctions
    {
        public static float EaseInOutSine(float num)
        {
            return -(Mathf.Cos((float)Math.PI * num) - 1) / 2;
        }

        public static float EaseInSine(float num)
        {
            return 1f - Mathf.Cos((float)(num * Math.PI) / 2f);
        }
    }
}