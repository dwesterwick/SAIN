using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static SAIN.Components.RaycastManager;

namespace SAIN.Components
{
    public class RaycastManager
    {
        public RaycastManager()
        {
            _raycastJob = new SAINRaycastJob();
            JobManager.AddJob($"{nameof(RaycastManager)}{_instanceCount++}", _raycastJob);
        }

        private static int _instanceCount;

        private readonly SAINRaycastJob _raycastJob;
        private readonly List<RaycastJobData> _raycastDatas = new List<RaycastJobData>();

        private readonly List<RaycastCommand> _commands = new List<RaycastCommand>();
        private readonly List<RaycastHit> _hits = new List<RaycastHit>();

        private bool _completeNextFrame;

        public void Update()
        {
            if (_completeNextFrame) {
                _completeNextFrame = false;
                completeRaycasts();
            }
            if (_raycastJob.IsComplete) {
                scheduleRaycasts();
                _completeNextFrame = true;
            }
        }

        private void completeRaycasts()
        {
            int count = _raycastDatas.Count;
            if (count == 0) {
                return;
            }

            NativeArray<RaycastHit> jobHits = _raycastJob.Hits;
            _commands.Clear();
            _hits.Clear();
            for (int i = 0; i < count; i++) {
                RaycastJobData data = _raycastDatas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Status = EJobStatus.Complete;
                    data.Hit = jobHits[i];
                }
            }
            _raycastJob.Dispose();
        }

        private void scheduleRaycasts()
        {
            int count = _raycastDatas.Count;
            if (count == 0) {
                return;
            }

            _commands.Clear();
            _hits.Clear();
            for (int i = 0; i < count; i++) {
                RaycastJobData data = _raycastDatas[i];
                if (data.Status != EJobStatus.UnScheduled) {
                    continue;
                }

                DistanceData distanceData = data.DistanceData;
                data.Command = new RaycastCommand {
                    from = distanceData.Origin,
                    direction = distanceData.Direction,
                    distance = distanceData.Distance,
                    layerMask = data.LayerMask,
                };
                _commands.Add(data.Command);

                data.Hit = new RaycastHit();
                _hits.Add(data.Hit);
            }

            NativeArray<RaycastCommand> commandsArray = new NativeArray<RaycastCommand>(_commands.ToArray(), Allocator.TempJob);
            NativeArray<RaycastHit> hitsArray = new NativeArray<RaycastHit>(_hits.ToArray(), Allocator.TempJob);
            JobHandle handle = RaycastCommand.ScheduleBatch(commandsArray, hitsArray, 5);
            _raycastJob.Init(handle, commandsArray, hitsArray);
        }

        public int AddRaycastToJob(RaycastJobData data)
        {
            _raycastDatas.Add(data);
            return _raycastDatas.Count - 1;
        }

        public EJobStatus GetStatus(int index)
        {
            return _raycastDatas[index].Status;
        }

        public RaycastJobData RetreiveResults(int index)
        {
            return _raycastDatas[index];
        }

        public enum EJobStatus
        {
            None,
            AwaitingOtherJob,
            UnScheduled,
            Scheduled,
            Complete,
        }

        public abstract class JobData
        {
            public event Action<JobData> OnCompleted;

            public EJobStatus Status {
                get
                {
                    return _status;
                }
                set
                {
                    if (value != _status) {
                        value = _status;
                        if (value == EJobStatus.Complete) {
                            OnCompleted?.Invoke(this);
                        }
                    }
                }
            }

            private EJobStatus _status;

            protected bool CanBeScheduled()
            {
                switch (Status) {
                    case EJobStatus.None:
                    case EJobStatus.Complete:
                        return true;

                    default:
                        Logger.LogError($"Cannot update data that is in Queue for job! Status: {Status}");
                        return false;
                }
            }
        }

        public class RaycastJobData : JobData
        {
            public DistanceData DistanceData;
            public LayerMask LayerMask;
            public RaycastCommand Command;
            public RaycastHit Hit;

            public void Create()
            {
                if (!base.CanBeScheduled()) {
                    return;
                }
                Command = new RaycastCommand {
                    from = DistanceData.Origin,
                    direction = DistanceData.Direction,
                    distance = DistanceData.Distance,
                    layerMask = LayerMask,
                };
                Hit = new RaycastHit();
                Status = EJobStatus.UnScheduled;
            }
        }

        public class VectorsRaycastsData : JobData
        {
            public LayerMask LayerMask { get; private set; }
            public readonly List<RaycastJobData> RaycastDatas = new List<RaycastJobData>();
            private readonly VectorsDistancesData _vectorDistances = new VectorsDistancesData();
            public int CountToCheck => _vectorDistances.CountToCheck;

            public void RaycastBetweenVectors(Vector3[] vectors)
            {
                if (!base.CanBeScheduled()) {
                    return;
                }
                _vectorDistances.ScheduleCalcDistanceBetweenVectors(vectors);
                createCache(CountToCheck);
                Status = EJobStatus.AwaitingOtherJob;
            }

            public void RaycastToPoints(Vector3[] vectors, Vector3 origin)
            {
                if (!base.CanBeScheduled()) {
                    return;
                }
                _vectorDistances.ScheduleCalcDistanceToPoints(vectors, origin);
                createCache(CountToCheck);
                Status = EJobStatus.AwaitingOtherJob;
            }

            private void onCompleteDistanceCalc(JobData data)
            {
                for (int i = 0; i < CountToCheck; i++) {
                    RaycastDatas[i].Create();
                }
                Status = EJobStatus.UnScheduled;
            }

            private void createCache(int targetCount)
            {
                int cacheCount = RaycastDatas.Count;
                if (cacheCount >= targetCount) {
                    return;
                }
                // This is not optimal, but the loops are making my fucking brain hurt
                while (cacheCount < targetCount) {
                    RaycastDatas.Add(new RaycastJobData {
                        LayerMask = this.LayerMask
                    });
                    cacheCount++;
                }
                var distances = _vectorDistances.DistanceDatas;
                for (int i = 0; i < cacheCount; i++) {
                    RaycastDatas[i].DistanceData = distances[i];
                }
            }

            public void Dispose()
            {
                _vectorDistances.OnCompleted -= onCompleteDistanceCalc;
            }

            public void UpdateMask(LayerMask mask)
            {
                LayerMask = mask;
                foreach (var data in RaycastDatas) {
                    data.LayerMask = mask;
                }
            }

            public VectorsRaycastsData(LayerMask mask, DistanceData distanceData)
            {
                LayerMask = mask;
                _vectorDistances.OnCompleted += onCompleteDistanceCalc;
            }
        }

        public class VectorsDistancesData : JobData
        {
            public int CountToCheck { get; private set; }
            public readonly List<DistanceData> DistanceDatas = new List<DistanceData>();

            public void ScheduleCalcDistanceBetweenVectors(Vector3[] vectors)
            {
                if (!base.CanBeScheduled()) {
                    return;
                }
                int count = vectors.Length - 1;
                createCache(count);
                CountToCheck = count;
                for (int i = 0; i < count; i++) {
                    DistanceDatas[i].UpdateData(vectors[i], vectors[i + 1]);
                }
                Status = EJobStatus.UnScheduled;
            }

            public void ScheduleCalcDistanceToPoints(Vector3[] vectors, Vector3 origin)
            {
                if (!base.CanBeScheduled()) {
                    return;
                }
                int count = vectors.Length;
                createCache(count);
                CountToCheck = count;
                for (int i = 0; i < count; i++) {
                    DistanceDatas[i].UpdateData(origin, vectors[i]);
                }
                Status = EJobStatus.UnScheduled;
            }

            private void createCache(int targetCount)
            {
                int cacheCount = DistanceDatas.Count;
                if (cacheCount >= targetCount) {
                    return;
                }
                // This is not optimal, but the loops are making my fucking brain hurt
                while (DistanceDatas.Count < targetCount) {
                    DistanceDatas.Add(new DistanceData());
                }
            }
        }

        public class DistanceData : JobData
        {
            public Vector3 Origin { get; private set; }
            public Vector3 Direction { get; private set; }

            public Vector3 Normal;
            public float Distance;

            public void UpdateData(Vector3 origin, Vector3 target)
            {
                if (!base.CanBeScheduled()) {
                    return;
                }
                Origin = origin;
                Direction = target - origin;
                Status = EJobStatus.UnScheduled;
            }
        }
    }
}