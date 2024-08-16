using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct BiDirData
    {
        public BiDirData(Vector3 primaryDir, Vector3 secondDir)
        {
            Primary = new DirData {
                Direction = primaryDir,
            };
            Secondary = new DirData {
                Direction = secondDir,
            };
            SignedAngle = 0;
            Axis = Vector3.up;
            DotProduct = 0;
        }

        public DirData Primary;
        public DirData Secondary;
        public float SignedAngle;
        public Vector3 Axis;
        public float DotProduct;

        public void Calculate()
        {
            Primary.Calculate();
            Secondary.Calculate();
            SignedAngle = Vector3.SignedAngle(Primary.Normal, Secondary.Normal, Axis);
            DotProduct = Vector3.Dot(Primary.Normal, Secondary.Normal);
        }
    }
}