using UnityEngine;
using SAIN.Helpers;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct DirData
    {
        public DirData(Vector3 direction)
        {
            Direction = direction;
            Normal = Vector.Normal(direction);
            Distance = Vector.Distance(direction);
        }

        public readonly Vector3 Direction;
        public readonly Vector3 Normal;
        public readonly float Distance;
    }
}