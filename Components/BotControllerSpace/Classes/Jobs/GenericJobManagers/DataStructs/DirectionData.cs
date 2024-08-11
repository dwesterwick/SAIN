using UnityEngine;

namespace SAIN.Components
{
    public class DirectionData : AbstractJobData
    {
        public Vector3 Origin { get; private set; }
        public Vector3 Direction { get; private set; }

        public Vector3 Normal;
        public float Distance;

        public void UpdateData(Vector3 origin, Vector3 target)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            Origin = origin;
            Direction = target - origin;
            Status = EJobStatus.UnScheduled;
        }
    }
}