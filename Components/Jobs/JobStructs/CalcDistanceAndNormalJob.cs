using Unity.Collections;
using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct CalcDistanceAndNormalJob : ISAINJob
    {
        [ReadOnly] public NativeArray<Vector3> directions;

        [WriteOnly] public NativeArray<float> distances;
        [WriteOnly] public NativeArray<Vector3> normals;

        public void Execute(int index)
        {
            Vector3 direction = directions[index];
            distances[index] = direction.magnitude;
            normals[index] = direction.normalized;
        }

        public void Create(NativeArray<Vector3> directions)
        {
            int total = directions.Length;
            this.directions = directions;
            distances = new NativeArray<float>(total, Allocator.TempJob);
            normals = new NativeArray<Vector3>(total, Allocator.TempJob);
        }

        public void Dispose()
        {
            if (directions.IsCreated) directions.Dispose();
            if (distances.IsCreated) distances.Dispose();
            if (normals.IsCreated) normals.Dispose();
        }
    }
}