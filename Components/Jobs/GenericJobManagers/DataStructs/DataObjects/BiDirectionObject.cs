using SAIN.Components.BotControllerSpace.Classes.Raycasts;

namespace SAIN.Components
{
    public class BiDirectionObject : AbstractJobObject
    {
        public BiDirData Data { get; private set; }

        public void Complete(BiDirData data)
        {
            Data = data;
            Status = EJobStatus.Complete;
            Logger.LogInfo(data.SignedAngle);
        }

        public void Schedule()
        {
            Status = EJobStatus.Scheduled;
        }

        public void UpdateData(BiDirData data)
        {
            if (!base.CanBeScheduled()) {
                return;
            }
            Data = data;
            Status = EJobStatus.UnScheduled;
        }
    }
}