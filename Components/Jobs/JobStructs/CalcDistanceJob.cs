using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct CalcDistanceJob : ISAINJob
    {
        [ReadOnly] public NativeArray<Vector3> Directions;
        [WriteOnly] public NativeArray<float> Distances;

        public void Execute(int index)
        {
            Distances[index] = Directions[index].magnitude;
        }

        public void Create(NativeArray<Vector3> directions)
        {
            Directions = directions;
            Distances = new NativeArray<float>(directions.Length, Allocator.TempJob);
        }

        public void Create(List<Vector3> directions, int count)
        {
            Distances = new NativeArray<float>(count, Allocator.TempJob);
            Directions = new NativeArray<Vector3>(count, Allocator.TempJob);
            for (int i = 0; i < count; i++) {
                Directions[i] = directions[i];
            }
        }

        public void Dispose()
        {
            if (Directions.IsCreated) Directions.Dispose();
            if (Distances.IsCreated) Distances.Dispose();
        }
    }
}