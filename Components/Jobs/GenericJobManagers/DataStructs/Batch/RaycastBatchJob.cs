using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class RaycastBatchJob : AbstractBatchJob<RaycastObject>
    {
        public LayerMask LayerMask { get; private set; }

        public void ScheduleRaycastToPoints(Vector3[] vectors, Vector3 origin)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = vectors.Length;
            if (count > 0) {
                base.SetupJob(count);
                for (int i = 0; i < count; i++) {
                    createRaycast(i, vectors[i], origin);
                }
                Status = EJobStatus.UnScheduled;
            }
        }

        public void ScheduleRaycastToPoints(List<Vector3> vectors, Vector3 origin)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            int count = vectors.Count;
            if (count > 0) {
                base.SetupJob(count);
                for (int i = 0; i < count; i++) {
                    createRaycast(i, vectors[i], origin);
                }
                Status = EJobStatus.UnScheduled;
            }
        }

        private void createRaycast(int index, Vector3 point, Vector3 origin)
        {
            DistanceData distanceData = new DistanceData {
                Origin = origin,
                Direction = point - origin,
                Distance = Vector.Distance(origin, point),
            };
            Datas[index].Create(distanceData, LayerMask);
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public RaycastBatchJob(LayerMask mask, ListCache<RaycastObject> cache) : base(EJobType.Raycast, cache)
        {
            LayerMask = mask;
        }
    }
}