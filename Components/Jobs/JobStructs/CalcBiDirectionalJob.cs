using System.Collections.Generic;
using Unity.Collections;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct CalcBiDirectionalJob : ISAINJob
    {
        public NativeArray<BiDirData> DirectionData;

        public void Execute(int index)
        {
            BiDirData data = DirectionData[index];
            data.Calculate();
            DirectionData[index] = data;
        }

        public void Create(List<BiDirectionObject> dataList, int count)
        {
            DirectionData = new NativeArray<BiDirData>(count, Allocator.TempJob);
            for (int i = 0; i < count; i++) {
                DirectionData[i] = dataList[i].Data;
            }
        }

        public void Dispose()
        {
            if (DirectionData.IsCreated) DirectionData.Dispose();
        }
    }
}