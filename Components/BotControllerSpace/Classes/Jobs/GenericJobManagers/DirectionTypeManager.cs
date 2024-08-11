using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class DistanceTypeManager : JobTypeManager<SAINCustomJob<CalcDistanceJob>, DistanceData>
    {
        public DistanceTypeManager() : base(new SAINCustomJob<CalcDistanceJob>(0))
        {
        }

        private readonly List<Vector3> _directions = new List<Vector3>();

        public override void Complete()
        {
            if (!ShallComplete()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            base.Complete();

            var distances = JobContainer.Job.Distances;
            int completeCount = 0;
            for (int i = 0; i < count; i++) {
                DistanceData data = Datas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(distances[completeCount]);
                    completeCount++;
                }
            }
            JobContainer.Job.Dispose();
        }

        public override void Schedule()
        {
            if (!ShallSchedule()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            int scheduledCount = 0;
            _directions.Clear();
            for (int i = 0; i < count; i++) {
                DistanceData data = Datas[i];
                if (data.Status == EJobStatus.UnScheduled) {
                    _directions.Add(data.Direction);
                    data.Schedule();
                    scheduledCount++;
                }
            }

            //Logger.LogInfo(scheduledCount);

            if (scheduledCount > 0) {
                var job = new CalcDistanceJob();
                job.Create(_directions, scheduledCount);
                var handle = job.Schedule(scheduledCount, new JobHandle());
                JobContainer.Init(handle, job);
            }
        }
    }

    public class DirectionTypeManager : JobTypeManager<SAINCustomJob<CalcDistanceAndNormalJob>, DirectionData>
    {
        public DirectionTypeManager() : base(new SAINCustomJob<CalcDistanceAndNormalJob>(0))
        {
        }

        private readonly List<Vector3> _directions = new List<Vector3>();

        public override void Complete()
        {
            if (!ShallComplete()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            base.Complete();

            var normals = JobContainer.Job.normals;
            var distances = JobContainer.Job.distances;
            int completeCount = 0;
            for (int i = 0; i < count; i++) {
                DirectionData data = Datas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(normals[completeCount], distances[completeCount]);
                    completeCount++;
                }
            }
            JobContainer.Job.Dispose();
        }

        public override void Schedule()
        {
            if (!ShallSchedule()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            int scheduledCount = 0;
            _directions.Clear();
            for (int i = 0; i < count; i++) {
                DirectionData data = Datas[i];
                if (data.Status == EJobStatus.UnScheduled) {
                    _directions.Add(data.Direction);
                    data.Schedule();
                    scheduledCount++;
                }
            }

            //Logger.LogInfo(scheduledCount);

            if (scheduledCount > 0) {
                var job = new CalcDistanceAndNormalJob();
                job.Create(new NativeArray<Vector3>(_directions.ToArray(), Allocator.TempJob));
                var handle = job.Schedule(scheduledCount, new JobHandle());
                JobContainer.Init(handle, job);
            }
        }
    }
}