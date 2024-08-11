using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public abstract class SAINJobBase
    {
        public EJobStatus Status;
        public JobHandle Handle { get; private set; }
        public int FrameCreated { get; private set; }
        public float TimeCreated { get; private set; }

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
            switch (Status) {
                case EJobStatus.Complete:
                case EJobStatus.None:
                    break;

                default:
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

        public virtual void Complete()
        {
            if (!Handle.IsCompleted) {
                Handle.Complete();
            }
            Status = EJobStatus.Complete;
        }

        public virtual void Schedule(JobHandle handle)
        {
            Handle = handle;
            FrameCreated = Time.frameCount;
            TimeCreated = Time.time;
            Status = EJobStatus.Scheduled;
        }

        public virtual void Dispose()
        {
            Complete();
        }
    }
}