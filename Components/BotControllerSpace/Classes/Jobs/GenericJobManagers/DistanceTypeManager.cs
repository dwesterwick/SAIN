using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class DistanceTypeManager : JobTypeManager<SAINCustomJob<CalcDistanceJob>, DistanceObject>
    {
        public DistanceTypeManager() : base(new SAINCustomJob<CalcDistanceJob>(0))
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

            var distances = JobContainer.Job.Distances;
            int completeCount = 0;
            for (int i = 0; i < count; i++) {
                DistanceObject data = Datas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(distances[completeCount]);
                    completeCount++;
                }
            }
            JobContainer.Job.Dispose();
        }

        public override void Schedule()
        {
            if (!HasJobsToSchedule()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            int scheduledCount = 0;
            _directions.Clear();
            for (int i = 0; i < count; i++) {
                DistanceObject data = Datas[i];
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
                var handle = job.ScheduleParallel(scheduledCount, 10, new JobHandle());
                JobContainer.Init(handle, job);
            }
        }
    }
}