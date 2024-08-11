using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public abstract class SAINJobBase
    {
        public JobHandle Handle { get; private set; }
        public int FrameCreated { get; private set; }
        public float TimeCreated { get; private set; }

        public bool IsComplete;
        private readonly int _frameDelay = -1;
        private readonly float _timeDelay = -1f;

        public SAINJobBase(float timeDelay)
        {
        }

        public SAINJobBase(int frameDelay)
        {
        }

        public bool ShallCalculate()
        {
            if (!IsComplete) {
                return false;
            }
            if (_frameDelay > 0) {
                return Time.frameCount <= FrameCreated + _frameDelay;
            }
            if (_timeDelay > 0) {
                return Time.time <= TimeCreated + _timeDelay;
            }
            return true;
        }

        protected void Init(JobHandle handle)
        {
            Handle = handle;
            FrameCreated = Time.frameCount;
            TimeCreated = Time.time;
            IsComplete = false;
        }

        public virtual void Dispose()
        {
            if (!Handle.IsCompleted) {
                Handle.Complete();
            }
        }
    }
}