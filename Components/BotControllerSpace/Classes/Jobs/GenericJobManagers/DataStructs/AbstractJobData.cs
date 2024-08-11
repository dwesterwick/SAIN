using System;

namespace SAIN.Components
{
    public abstract class AbstractJobData
    {
        public event Action<AbstractJobData> OnCompleted;

        public event Action<AbstractJobData> OnDispose;

        public EJobStatus Status {
            get
            {
                return _status;
            }
            set
            {
                if (value != _status) {
                    _status = value;
                    switch (value) {
                        case EJobStatus.Complete:
                            OnCompleted?.Invoke(this);
                            break;

                        case EJobStatus.Disposed:
                            OnDispose?.Invoke(this);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        private EJobStatus _status;

        protected bool CanBeScheduled()
        {
            switch (Status) {
                case EJobStatus.None:
                case EJobStatus.Complete:
                    return true;

                default:
                    Logger.LogError($"Cannot update data that is in Queue for job! Status: {Status}");
                    return false;
            }
        }

        public virtual void Dispose()
        {
            Status = EJobStatus.Disposed;
        }
    }
}