namespace SAIN.Components.PlayerComponentSpace
{
    public class OtherPlayerData
    {
        public readonly string ProfileId;
        public PlayerComponent PlayerComponent;
        public bool IsAI => PlayerComponent.IsAI;
        public OtherPlayerDistanceData DistanceData { get; } = new OtherPlayerDistanceData();

        public OtherPlayerData(PlayerComponent playerComponent)
        {
            ProfileId = playerComponent.ProfileId;
            PlayerComponent = playerComponent;
        }
    }
}