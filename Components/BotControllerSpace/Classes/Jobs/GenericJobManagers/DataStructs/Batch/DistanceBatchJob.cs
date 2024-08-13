using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class DistanceBatchJob : AbstractBatchJob<DistanceObject>
    {
        public int ScheduleCalcBetweenVectors(Vector3[] vectors)
        {
            if (!base.CanBeScheduled()) {
                return 0;
            }
            int count = vectors.Length - 1;
            if (count < 0) {
                return 0;
            }
            base.SetupJob(count);
            for (int i = 0; i < count; i++) {
                Datas[i].UpdateData(vectors[i], vectors[i + 1]);
            }
            return count;
        }

        public int ScheduleCalcToPoints(Vector3[] vectors, Vector3 origin)
        {
            if (!base.CanBeScheduled()) {
                return 0;
            }
            int count = vectors.Length;
            if (count < 0) {
                return 0;
            }
            base.SetupJob(count);
            for (int i = 0; i < count; i++) {
                Datas[i].UpdateData(origin, vectors[i]);
            }
            return count;
        }

        public int ScheduleCalcToPoints(List<Vector3> vectors, Vector3 origin)
        {
            if (!base.CanBeScheduled()) {
                return 0;
            }
            int count = vectors.Count;
            if (count < 0) {
                return 0;
            }
            base.SetupJob(count);
            for (int i = 0; i < count; i++) {
                Datas[i].UpdateData(origin, vectors[i]);
            }
            return count;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public DistanceBatchJob(ListCache<DistanceObject> cache) : base(EJobType.Distance, cache)
        {
        }
    }
}