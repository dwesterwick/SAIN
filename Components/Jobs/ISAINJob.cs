using Unity.Jobs;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public interface ISAINJob : IJobFor
    {
        void Dispose();
    }
}