using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class RaycastBatchData : AbstractBatchJob<RaycastData>
    {
        public LayerMask LayerMask { get; private set; }

        private readonly DirectionalBatchData _vectorMagnitudes = new DirectionalBatchData();

        public void RaycastBetweenVectors(Vector3[] vectors)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = _vectorMagnitudes.ScheduleCalcBetweenVectors(vectors);
            setupJob(count);
        }

        public void RaycastToPoints(Vector3[] vectors, Vector3 origin)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = _vectorMagnitudes.ScheduleCalcToPoints(vectors, origin);
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
            _vectorMagnitudes.Dispose();
            _vectorMagnitudes.OnCompleted -= onCompleteDistanceCalc;
        }

        public void UpdateMask(LayerMask mask)
        {
            LayerMask = mask;
            foreach (var data in Datas) {
                data.LayerMask = mask;
            }
        }

        public RaycastBatchData(LayerMask mask) : base(EJobType.Raycast)
        {
            LayerMask = mask;
            _vectorMagnitudes.OnCompleted += onCompleteDistanceCalc;
        }
    }
}