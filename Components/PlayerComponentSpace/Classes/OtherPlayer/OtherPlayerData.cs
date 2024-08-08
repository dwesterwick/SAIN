namespace SAIN.Components.PlayerComponentSpace
{
    public class OtherPlayerData
    {
        public readonly string ProfileId;
        public OtherPlayerDistanceData DistanceData { get; } = new OtherPlayerDistanceData();

        public OtherPlayerData(string id)
        {
            ProfileId = id;
        }
    }
}