using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using System.Collections.Generic;
using Unity.Jobs;

namespace SAIN.Components
{
    public class BiDirectionalTypeManager : JobTypeManager<SAINCustomJob<CalcBiDirectionalJob>, BiDirectionObject>
    {
        public BiDirectionalTypeManager() : base(new SAINCustomJob<CalcBiDirectionalJob>(0))
        {
        }

        private readonly List<BiDirectionObject> _directions = new List<BiDirectionObject>();

        public override void Complete()
        {
            if (!HasJobsToCheckComplete()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            base.Complete();

            var results = JobContainer.Job.DirectionData;
            int completeCount = 0;
            for (int i = 0; i < count; i++) {
                BiDirectionObject data = Datas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(results[completeCount]);
                    completeCount++;
                }
            }
            //Logger.LogInfo($"{completeCount} : {count}");
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
                BiDirectionObject data = Datas[i];
                if (data.Status == EJobStatus.UnScheduled) {
                    data.Schedule();
                    _directions.Add(data);
                    scheduledCount++;
                }
            }

            //Logger.LogInfo($"{scheduledCount} : {count}");
            if (scheduledCount > 0) {
                var job = new CalcBiDirectionalJob();
                job.Create(_directions, scheduledCount);
                var handle = job.ScheduleParallel(scheduledCount, 5, new JobHandle());
                JobContainer.Init(handle, job);
            }
        }
    }
}