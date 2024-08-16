using Unity.Collections;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct CalcDirectionalJob : ISAINJob
    {
        public NativeArray<DirData> DirectionData;

        public void Execute(int index)
        {
            var data = DirectionData[index];
            data.Calculate();
        }

        public NativeArray<DirData> Create(int count)
        {
            DirectionData = new NativeArray<DirData>(count, Allocator.TempJob);
            return DirectionData;
        }

        public void Dispose()
        {
            if (DirectionData.IsCreated) DirectionData.Dispose();
        }
    }
}