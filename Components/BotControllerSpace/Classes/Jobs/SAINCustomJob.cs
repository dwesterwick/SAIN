using SAIN.Components.BotControllerSpace.Classes.Raycasts;
using Unity.Jobs;

namespace SAIN.Components
{
    public class SAINCustomJob : SAINJobBase
    {
        public ISAINJob Job { get; private set; }

        public void Init(JobHandle handle, ISAINJob job)
        {
            Job = job;
            base.Init(handle);
        }

        public override void Dispose()
        {
            Job.Dispose();
            base.Dispose();
        }
    }
}