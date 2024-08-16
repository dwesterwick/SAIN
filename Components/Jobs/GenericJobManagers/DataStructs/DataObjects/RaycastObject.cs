using UnityEngine;

namespace SAIN.Components
{
    public class RaycastObject : AbstractJobObject
    {
        public RaycastCommand Command;
        public RaycastHit Hit;

        public void Complete(RaycastHit hit)
        {
            Hit = hit;
            Status = EJobStatus.Complete;
        }

        public void Create(DistanceData distanceData, LayerMask mask)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            Command = new RaycastCommand(distanceData.Origin, distanceData.Direction, distanceData.Distance, mask);
            Hit = new RaycastHit();
            Status = EJobStatus.UnScheduled;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}