using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct DirData
    {
        public Vector3 Direction;
        public Vector3 Normal;
        public float Distance;

        public void Calculate()
        {
            Normal = Direction.normalized;
            Distance = Direction.magnitude;
        }
    }
}