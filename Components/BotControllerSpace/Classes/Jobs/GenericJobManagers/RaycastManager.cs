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
            _raycastJob = new GlobalRaycastJob();
            JobManager.AddJob(nameof(RaycastManager), _raycastJob);
        }

        private static readonly GlobalRaycastJob _raycastJob;
        private static readonly List<RaycastData> _raycastDatas = new List<RaycastData>();
        private static readonly List<RaycastCommand> _commands = new List<RaycastCommand>();
        private static readonly List<RaycastHit> _hits = new List<RaycastHit>();
        private static bool _jobScheduled;

        public static void Update()
        {
        }

        public static void LateUpdate()
        {
            if (_jobScheduled) {
                completeRaycasts();
            }
            if (_raycastJob.IsComplete) {
                scheduleRaycasts();
            }
        }

        private static void completeRaycasts()
        {
            if (!_jobScheduled) {
                return;
            }
            int count = _raycastDatas.Count;
            if (count == 0) {
                return;
            }

            NativeArray<RaycastHit> jobHits = _raycastJob.Hits;
            for (int i = 0; i < count; i++) {
                RaycastData data = _raycastDatas[i];
                if (data.Status == EJobStatus.Scheduled) {
                    data.Complete(jobHits[i]);
                }
            }
            _raycastJob.Dispose();
            _jobScheduled = false;
        }

        private static void scheduleRaycasts()
        {
            if (!_raycastJob.IsComplete) {
                return;
            }

            int count = _raycastDatas.Count;
            if (count == 0) {
                return;
            }

            _commands.Clear();
            _hits.Clear();
            for (int i = 0; i < count; i++) {
                RaycastData data = _raycastDatas[i];
                if (data.Status != EJobStatus.UnScheduled) {
                    continue;
                }
                _commands.Add(data.Command);
                _hits.Add(data.Hit);
                data.Status = EJobStatus.Scheduled;
            }

            NativeArray<RaycastCommand> commandsArray = new NativeArray<RaycastCommand>(_commands.ToArray(), Allocator.TempJob);
            NativeArray<RaycastHit> hitsArray = new NativeArray<RaycastHit>(_hits.ToArray(), Allocator.TempJob);
            JobHandle handle = RaycastCommand.ScheduleBatch(commandsArray, hitsArray, 5);
            _raycastJob.Init(handle, commandsArray, hitsArray);
            _jobScheduled = true;
        }

        public static void AddRaycastToJob(RaycastData data)
        {
            if (_raycastDatas.Contains(data)) {
                Logger.LogError($"Data already added to list!");
                return;
            }
            _raycastDatas.Add(data);
            Logger.LogDebug($"Added data to Raycasts");
        }

        public static void Remove(RaycastData data)
        {
            if (!_raycastDatas.Contains(data)) {
                Logger.LogError($"Data not in List!");
                return;
            }
            Logger.LogDebug($"Removed data from Raycasts");
            _raycastDatas.Remove(data);
        }
    }
}