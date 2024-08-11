using UnityEngine;

namespace SAIN.Components
{
    public class DistanceData : AbstractJobData
    {
        public Vector3 Origin { get; private set; }
        public Vector3 Direction { get; private set; }
        public float Distance { get; private set; }

        public void Complete(float distance)
        {
            Distance = distance;
            Status = EJobStatus.Complete;
        }

        public void Schedule()
        {
            Status = EJobStatus.Scheduled;
        }

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