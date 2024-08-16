using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct BiDirData
    {
        public BiDirData(Vector3 primaryDir, Vector3 secondDir)
        {
            Primary = new DirData(primaryDir);
            Secondary = new DirData(secondDir);
            SignedAngle = Vector3.SignedAngle(Primary.Normal, Secondary.Normal, Vector3.up);
            DotProduct = Vector3.Dot(Primary.Normal, Secondary.Normal);
        }

        public readonly DirData Primary;
        public readonly DirData Secondary;
        public readonly float SignedAngle;
        public readonly float DotProduct;
    }
}