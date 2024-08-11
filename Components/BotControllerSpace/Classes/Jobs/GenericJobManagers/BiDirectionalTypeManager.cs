using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using System.Collections.Generic;
using Unity.Jobs;

namespace SAIN.Components
{
    public class BiDirectionalTypeManager : JobTypeManager<SAINCustomJob<CalcBiDirectionalJob>, BiDirectionData>
    {
        public BiDirectionalTypeManager() : base(new SAINCustomJob<CalcBiDirectionalJob>(0))
        {
        }

        private readonly List<BiDirectionData> _directions = new List<BiDirectionData>();

        public override void Complete()
        {
            if (!ShallComplete()) {
                return;
            }

            int count = Datas.Count;
            //Logger.LogInfo(count);
            base.Complete();

            var results = JobContainer.Job.DirectionData;
            int completeCount = 0;
            for (int i = 0; i < count; i++) {
                BiDirectionData data = Datas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(results[completeCount]);
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
                BiDirectionData data = Datas[i];
                if (data.Status == EJobStatus.UnScheduled) {
                    _directions.Add(data);
                    data.Schedule();
                    scheduledCount++;
                }
            }

            //Logger.LogInfo(scheduledCount);

            if (scheduledCount > 0) {
                var job = new CalcBiDirectionalJob();
                job.Create(_directions, scheduledCount);
                var handle = job.Schedule(scheduledCount, new JobHandle());
                JobContainer.Init(handle, job);
            }
        }
    }
}