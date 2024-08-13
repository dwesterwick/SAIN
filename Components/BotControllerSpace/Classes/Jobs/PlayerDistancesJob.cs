using SAIN.Components.PlayerComponentSpace;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts
{
    public struct BiDirData
    {
        public BiDirData(Vector3 primaryDir, Vector3 secondDir)
        {
            Primary = new DirData {
                Direction = primaryDir,
            };
            Secondary = new DirData {
                Direction = secondDir,
            };
            SignedAngle = 0;
            Axis = Vector3.up;
            DotProduct = 0;
        }

        public DirData Primary;
        public DirData Secondary;
        public float SignedAngle;
        public Vector3 Axis;
        public float DotProduct;

        public void Calculate()
        {
            Primary.Calculate();
            Secondary.Calculate();
            SignedAngle = Vector3.SignedAngle(Primary.Normal, Secondary.Normal, Axis);
            DotProduct = Vector3.Dot(Primary.Normal, Secondary.Normal);
        }
    }

    public struct DirData
    {
        public Vector3 Direction;
        public Vector3 Normal;
        public float Distance;

        public void Calculate()
        {
            Normal = Direction.normalized;
            Distance = Direction.magnitude;
        }
    }

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

    public struct CalcDistanceJob : ISAINJob
    {
        [ReadOnly] public NativeArray<Vector3> Directions;
        [WriteOnly] public NativeArray<float> Distances;

        public void Execute(int index)
        {
            Vector3 direction = Directions[index];
            Distances[index] = direction.magnitude;
        }

        public void Create(NativeArray<Vector3> directions)
        {
            int total = directions.Length;
            this.Directions = directions;
            Distances = new NativeArray<float>(total, Allocator.TempJob);
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

    public class PlayerDistancesJob : SAINControllerBase
    {
        private JobHandle _distanceJobHandle;
        private CalcDistanceAndNormalJob _distanceJob;
        private JobHandle _partDistanceJobHandle;
        private CalcDistanceJob _partDistanceJob;
        private readonly List<PlayerComponent> _players = new List<PlayerComponent>();
        private static readonly EBodyPart[] _bodyParts = { EBodyPart.Head, EBodyPart.Chest, EBodyPart.Stomach, EBodyPart.LeftArm, EBodyPart.RightArm, EBodyPart.LeftLeg, EBodyPart.RightLeg };

        public PlayerDistancesJob(SAINBotController botController) : base(botController)
        {
            botController.StartCoroutine(calcDistancesLoop());
        }

        private IEnumerator calcDistancesLoop()
        {
            yield return null;
            while (true) {
                var gameWorld = GameWorldComponent.Instance;
                if (gameWorld == null) {
                    yield return null;
                    continue;
                }
                var players = gameWorld.PlayerTracker?.AlivePlayers;
                if (players == null || players.Count <= 1) {
                    yield return null;
                    continue;
                }

                if (BotController?.BotGame?.Status == EFT.GameStatus.Stopping) {
                    yield return null;
                    continue;
                }

                _players.Clear();
                _players.AddRange(players.Values);
                int playerCount = _players.Count;

                var directions = getDirections(_players, playerCount);
                int total = directions.Length;
                _distanceJob = new CalcDistanceAndNormalJob();
                _distanceJob.Create(directions);

                // schedule job and wait for next frame to read data
                _distanceJobHandle = _distanceJob.Schedule(total, new JobHandle());
                yield return null;
                _distanceJobHandle.Complete();
                readData(_players, _distanceJob);
                _distanceJob.Dispose();

                // Reset players list incase any were removed
                _players.Clear();
                _players.AddRange(players.Values);
                playerCount = _players.Count;

                // check part directions for vision checks
                var partDirections = getPartDirections(_players, playerCount);
                int partDirTotal = partDirections.Length;
                _partDistanceJob = new CalcDistanceJob();
                _partDistanceJob.Create(partDirections);

                // schedule job and wait for next frame to read data
                _partDistanceJobHandle = _partDistanceJob.Schedule(partDirTotal, new JobHandle());
                yield return null;
                _partDistanceJobHandle.Complete();
                readData(_players, _partDistanceJob);
                _partDistanceJob.Dispose();

                _players.Clear();
            }
        }

        private static void readData(List<PlayerComponent> players, CalcDistanceAndNormalJob job)
        {
            var directions = job.directions;
            var normals = job.normals;
            var distances = job.distances;
            int playerCount = players.Count;

            int count = 0;
            for (int i = 0; i < playerCount; i++) {
                var player = players[i];
                var datas = player?.OtherPlayersData.Datas;

                for (int j = 0; j < playerCount; j++) {
                    var otherPlayer = players[j];
                    if (otherPlayer != null && datas?.TryGetValue(otherPlayer.ProfileId, out var data) == true) {
                        data.DistanceData.Update(otherPlayer.Position, directions[count], normals[count], distances[count]);
                    }
                    count++;
                }
            }
        }

        private static void readData(List<PlayerComponent> players, CalcDistanceJob job)
        {
            var directions = job.Directions;
            var distances = job.Distances;
            int playerCount = players.Count;
            int partCount = _bodyParts.Length;

            int count = 0;
            for (int i = 0; i < playerCount; i++) {
                var player = players[i];
                var datas = player?.OtherPlayersData.Datas;

                for (int j = 0; j < playerCount; j++) {
                    var otherPlayer = players[j];
                    OtherPlayerData otherData = null;
                    if (otherPlayer != null)
                        datas?.TryGetValue(otherPlayer.ProfileId, out otherData);

                    for (int b = 0; b < partCount; b++) {
                        otherData?.DistanceData.UpdateBodyPart(_bodyParts[b], distances[count]);
                        count++;
                    }
                }
            }
        }

        private static NativeArray<Vector3> getPartDirections(List<PlayerComponent> players, int playerCount)
        {
            int count = 0;
            int partCount = _bodyParts.Length;
            int totalChecks = playerCount * playerCount * partCount;
            var directions = new NativeArray<Vector3>(totalChecks, Allocator.TempJob);
            for (int i = 0; i < playerCount; i++) {
                var player = players[i];
                Vector3 eyePosition = player != null ? player.Transform.EyePosition : Vector3.zero;

                for (int j = 0; j < playerCount; j++) {
                    var otherPlayer = players[j];
                    var parts = otherPlayer?.BodyParts.Parts;
                    for (int b = 0; b < partCount; b++) {
                        EBodyPart part = _bodyParts[b];
                        Vector3 partDir = parts != null ? parts[part].Transform.position - eyePosition : Vector3.zero;
                        directions[count] = partDir;
                        count++;
                    }
                }
            }
            return directions;
        }

        private static NativeArray<Vector3> getDirections(List<PlayerComponent> players, int playerCount)
        {
            int count = 0;
            int totalChecks = playerCount * playerCount;
            var directions = new NativeArray<Vector3>(totalChecks, Allocator.TempJob);
            for (int i = 0; i < playerCount; i++) {
                var player = players[i];
                Vector3 playerPos = player != null ? player.Position : Vector3.zero;

                for (int j = 0; j < playerCount; j++) {
                    var otherPlayer = players[j];
                    Vector3 otherPlayerPos = otherPlayer != null ? otherPlayer.Position : Vector3.zero;
                    Vector3 direction = otherPlayerPos - playerPos;
                    directions[count] = direction;
                    count++;
                }
            }
            return directions;
        }

        public void Dispose()
        {
            if (!_partDistanceJobHandle.IsCompleted) _partDistanceJobHandle.Complete();
            if (!_distanceJobHandle.IsCompleted) _distanceJobHandle.Complete();
            _distanceJob.Dispose();
            _partDistanceJob.Dispose();
        }
    }
}