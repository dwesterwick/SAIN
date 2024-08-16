using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class DirectionTypeManager : JobTypeManager<SAINCustomJob<CalcDistanceAndNormalJob>, DirectionObject>
    {
        public DirectionTypeManager() : base(new SAINCustomJob<CalcDistanceAndNormalJob>(0))
        {
        }

        private readonly List<Vector3> _directions = new List<Vector3>();

        public override void Complete()
        {
            if (!HasJobsToCheckComplete()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            base.Complete();

            var normals = JobContainer.Job.normals;
            var distances = JobContainer.Job.distances;
            int completeCount = 0;
            for (int i = 0; i < count; i++) {
                DirectionObject data = Datas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(normals[completeCount], distances[completeCount]);
                    completeCount++;
                }
            }
            JobContainer.Job.Dispose();
        }

        public override void Schedule(JobHandle dependency)
        {
            if (!HasJobsToSchedule()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            int scheduledCount = 0;
            _directions.Clear();
            for (int i = 0; i < count; i++) {
                DirectionObject data = Datas[i];
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
                var handle = job.ScheduleParallel(scheduledCount, 32, dependency);
                JobContainer.Init(handle, job);
            }
        }
    }
}