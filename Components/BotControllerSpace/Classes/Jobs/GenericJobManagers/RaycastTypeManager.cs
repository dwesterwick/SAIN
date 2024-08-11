using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class RaycastTypeManager : JobTypeManager<GlobalRaycastJob, RaycastData>
    {
        public RaycastTypeManager() : base(new GlobalRaycastJob())
        {
        }

        private readonly List<RaycastCommand> _commands = new List<RaycastCommand>();
        private readonly List<RaycastHit> _hits = new List<RaycastHit>();

        public override void Complete()
        {
            if (!ShallComplete()) {
                return;
            }

            int count = Datas.Count;
            base.Complete();
            NativeArray<RaycastHit> jobHits = SAINJob.Hits;
            for (int i = 0; i < count; i++) {
                RaycastData data = Datas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(jobHits[i]);
                }
            }
            SAINJob.DisposeArrays();
        }

        public override void Schedule()
        {
            if (!ShallSchedule()) {
                return;
            }

            int count = Datas.Count;
            _commands.Clear();
            _hits.Clear();
            int scheduledCount = 0;
            for (int i = 0; i < count; i++) {
                RaycastData data = Datas[i];
                if (data.Status != EJobStatus.UnScheduled) {
                    continue;
                }
                _commands.Add(data.Command);
                _hits.Add(data.Hit);
                data.Status = EJobStatus.Scheduled;
                scheduledCount++;
            }

            if (scheduledCount > 0) {
                NativeArray<RaycastCommand> commandsArray = new NativeArray<RaycastCommand>(_commands.ToArray(), Allocator.TempJob);
                NativeArray<RaycastHit> hitsArray = new NativeArray<RaycastHit>(_hits.ToArray(), Allocator.TempJob);
                JobHandle handle = RaycastCommand.ScheduleBatch(commandsArray, hitsArray, 5);
                SAINJob.Init(handle, commandsArray, hitsArray);
            }
        }
    }
}