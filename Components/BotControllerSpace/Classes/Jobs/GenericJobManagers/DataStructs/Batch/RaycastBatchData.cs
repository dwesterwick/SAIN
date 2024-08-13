using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class RaycastBatchData : AbstractBatchJob<RaycastObject>
    {
        public LayerMask LayerMask { get; private set; }

        private readonly DistanceBatchJob _vectorMagnitudes = new DistanceBatchJob(new ListCache<DistanceObject>("RaycastDistances"));

        public void ScheduleRaycastBetweenVectors(Vector3[] vectors)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = _vectorMagnitudes.ScheduleCalcBetweenVectors(vectors);
            setupJob(count);
        }

        public void ScheduleRaycastToPoints(Vector3[] vectors, Vector3 origin)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = _vectorMagnitudes.ScheduleCalcToPoints(vectors, origin);
            setupJob(count);
        }

        public void ScheduleRaycastToPoints(List<Vector3> vectors, Vector3 origin)
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

        private void onCompleteDistanceCalc(AbstractJobObject data)
        {
            //Logger.LogInfo("Distance calculation complete. Ready for raycast");
            Status = EJobStatus.UnScheduled;
        }

        public override void Dispose()
        {
            base.Dispose();
            OnItemAdded -= itemAdded;
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

        private void itemAdded(RaycastObject raycastData, int index)
        {
            raycastData.DistanceData = _vectorMagnitudes.Datas[index];
            raycastData.LayerMask = this.LayerMask;
        }

        public RaycastBatchData(LayerMask mask, ListCache<RaycastObject> cache) : base(EJobType.Raycast, cache)
        {
            UpdateMask(mask);
            _vectorMagnitudes.OnCompleted += onCompleteDistanceCalc;
            OnItemAdded += itemAdded;
        }
    }
}