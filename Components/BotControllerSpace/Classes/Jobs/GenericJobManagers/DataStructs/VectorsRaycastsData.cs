using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class VectorsRaycastsData : AbstractBatchJob<RaycastData>
    {
        public LayerMask LayerMask { get; private set; }

        private readonly VectorsDistancesData _vectorDistances = new VectorsDistancesData();

        public void RaycastBetweenVectors(Vector3[] vectors)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = _vectorDistances.ScheduleCalcDistanceBetweenVectors(vectors);
            setupJob(count);
        }

        public void RaycastToPoints(Vector3[] vectors, Vector3 origin)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = _vectorDistances.ScheduleCalcDistanceToPoints(vectors, origin);
            setupJob(count);
        }

        private void setupJob(int count)
        {
            if (count > 0) {
                base.SetupJob(count);
                Status = EJobStatus.AwaitingOtherJob;
            }
        }

        private void onCompleteDistanceCalc(AbstractJobData data)
        {
            Status = EJobStatus.UnScheduled;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vectorDistances.Dispose();
            _vectorDistances.OnCompleted -= onCompleteDistanceCalc;
        }

        public void UpdateMask(LayerMask mask)
        {
            LayerMask = mask;
            foreach (var data in Datas) {
                data.LayerMask = mask;
            }
        }

        public VectorsRaycastsData(LayerMask mask)
        {
            LayerMask = mask;
            _vectorDistances.OnCompleted += onCompleteDistanceCalc;
        }
    }
}