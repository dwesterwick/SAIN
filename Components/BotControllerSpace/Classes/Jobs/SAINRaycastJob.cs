using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINRaycastJob : SAINJobBase
    {
        public NativeArray<RaycastCommand> Commands { get; private set; }
        public NativeArray<RaycastHit> Hits { get; private set; }

        public void Init(JobHandle handle, NativeArray<RaycastCommand> commands, NativeArray<RaycastHit> hits)
        {
            Commands = commands;
            Hits = hits;
            base.Init(handle);
        }

        public override void Dispose()
        {
            Commands.Dispose();
            Hits.Dispose();
            base.Dispose();
        }
    }
}