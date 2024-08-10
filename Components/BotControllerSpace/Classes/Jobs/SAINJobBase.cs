using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINJobBase
    {
        public JobHandle Handle { get; private set; }
        public int FrameCreated { get; private set; }

        public bool IsComplete;

        protected void Init(JobHandle handle)
        {
            Handle = handle;
            FrameCreated = Time.frameCount;
            IsComplete = false;
        }

        public virtual void Dispose()
        {
        }
    }
}