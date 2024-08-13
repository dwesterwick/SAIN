namespace SAIN.Components
{
    public enum EJobStatus
    {
        Ready,
        AwaitingOtherJob,
        UnScheduled,
        Scheduled,
        Complete,
        Disposed,
        Cached,
    }
}