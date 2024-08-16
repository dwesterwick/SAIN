using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using Unity.Jobs;

namespace SAIN.Components
{
    public class SAINCustomJob<T> : SAINJobBase where T : ISAINJob
    {
        public T Job { get; private set; }

        public SAINCustomJob(int frameDelay) : base(frameDelay)
        {
        }

        public SAINCustomJob(float timeDelay) : base(timeDelay)
        {
        }

        public void Init(JobHandle handle, T job)
        {
            base.Schedule(handle);
            Job = job;
        }

        public override void Dispose()
        {
            base.Dispose();
            Job.Dispose();
        }
    }
}