namespace SAIN.Components
{
    public class BotRaycastJobs : SAINControllerBase
    {
        public VisionRaycastJob VisionJob { get; }

        public BotRaycastJobs(SAINBotController botController) : base(botController)
        {
            VisionJob = new VisionRaycastJob(botController);
        }

        public void Init()
        {
            VisionJob.Init();
        }

        public void Update()
        {
            VisionJob.Update();
        }

        public void Dispose()
        {
            VisionJob.Dispose();
        }
    }
}