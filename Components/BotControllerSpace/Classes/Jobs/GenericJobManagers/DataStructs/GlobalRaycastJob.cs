using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class GlobalRaycastJob : SAINJobBase
    {
        public NativeArray<RaycastCommand> Commands { get; private set; }
        public NativeArray<RaycastHit> Hits { get; private set; }

        public GlobalRaycastJob() : base(0)
        {
        }

        public override void Complete()
        {
            base.Complete();
        }

        public void Init(JobHandle handle, NativeArray<RaycastCommand> commands, NativeArray<RaycastHit> hits)
        {
            base.Schedule(handle);
            Commands = commands;
            Hits = hits;
        }

        public void DisposeArrays()
        {
            if (Commands.IsCreated) Commands.Dispose();
            if (Hits.IsCreated) Hits.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            DisposeArrays();
        }
    }
}