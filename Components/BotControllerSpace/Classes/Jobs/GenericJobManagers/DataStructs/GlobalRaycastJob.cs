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

        public void Init(JobHandle handle, NativeArray<RaycastCommand> commands, NativeArray<RaycastHit> hits)
        {
            base.Init(handle);
            Commands = commands;
            Hits = hits;
        }

        public override void Dispose()
        {
            base.Dispose();
            Commands.Dispose();
            Hits.Dispose();
        }
    }
}