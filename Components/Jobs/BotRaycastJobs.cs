namespace SAIN.Components
{
    public class BotRaycastJobs : SAINControllerBase
    {
        public VisionRaycastJob VisionJob { get; }

        public BotRaycastJobs(SAINBotController botController) : base(botController)
        {
            VisionJob = new VisionRaycastJob(botController);
        }

        public void Update()
        {
        }

        public void Dispose()
        {
            VisionJob.Dispose();
        }
    }
}