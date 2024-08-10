using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public static class RaycastManager
    {
        static RaycastManager()
        {
            _raycastJob = new SAINRaycastJob();
            JobManager.AddJob(nameof(RaycastManager), _raycastJob);
        }

        private static readonly SAINRaycastJob _raycastJob;
        private static readonly List<RaycastJobData> _raycastDatas = new List<RaycastJobData>();

        private static readonly List<RaycastCommand> _commands = new List<RaycastCommand>();
        private static readonly List<RaycastHit> _hits = new List<RaycastHit>();

        private static bool _completeNextFrame;

        public static void Update()
        {
            if (_raycastJob.IsComplete) {
                scheduleRaycasts();
                _completeNextFrame = true;
            }
            else if (_completeNextFrame) {
                _completeNextFrame = false;
                completeRaycasts();
            }
        }

        private static void completeRaycasts()
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
                if (!data.Scheduled) continue;
                if (data.Complete) continue;
                data.Complete = true;
                data.Hit = jobHits[i];
                _raycastDatas[i] = data;
            }
            _raycastJob.Dispose();
        }

        private static void scheduleRaycasts()
        {
            int count = _raycastDatas.Count;
            if (count == 0) {
                return;
            }

            _commands.Clear();
            _hits.Clear();
            for (int i = 0; i < count; i++) {
                RaycastJobData data = _raycastDatas[i];
                if (data.Scheduled) continue;
                data.Scheduled = true;

                _commands.Add(new RaycastCommand {
                    from = data.Origin,
                    direction = data.Direction,
                    distance = data.Distance,
                    layerMask = data.LayerMask,
                });
                data.Hit = new RaycastHit();
                _hits.Add(data.Hit);
                _raycastDatas[i] = data;
            }

            NativeArray<RaycastCommand> commandsArray = new NativeArray<RaycastCommand>(_commands.ToArray(), Allocator.TempJob);
            NativeArray<RaycastHit> hitsArray = new NativeArray<RaycastHit>(_hits.ToArray(), Allocator.TempJob);
            JobHandle handle = RaycastCommand.ScheduleBatch(commandsArray, hitsArray, 5);
            _raycastJob.Init(handle, commandsArray, hitsArray);
        }

        public static int AddRaycastToJob(RaycastJobData data)
        {
            _raycastDatas.Add(data);
            return _raycastDatas.Count - 1;
        }

        public static ERaycastStatus GetStatus(int index)
        {
            RaycastJobData data = _raycastDatas[index];
            if (data.Complete) {
                return ERaycastStatus.Complete;
            }
            if (data.Scheduled) {
                return ERaycastStatus.Scheduled;
            }
            return ERaycastStatus.UnScheduled;
        }

        public static RaycastJobData RetreiveResults(int index)
        {
            RaycastJobData data = _raycastDatas[index];
            _raycastDatas.Remove(data);
            return data;
        }

        public enum ERaycastStatus
        {
            UnScheduled,
            Complete,
            Scheduled,
        }

        public struct RaycastJobData
        {
            public Vector3 Origin;
            public Vector3 Direction;
            public float Distance;
            public LayerMask LayerMask;
            public RaycastHit Hit;
            public bool Scheduled;
            public bool Complete;
        }
    }
}