using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class JobManager : MonoBehaviour
    {
        public static JobManager Instance { get; private set; }

        public RaycastTypeManager Raycasts = new RaycastTypeManager();
        private JobHandle _jobHandle;
        private bool _hasJobToComplete = false;

        private void Awake()
        {
            Instance = this;
            Logger.LogWarning("Awake");
        }

        private void Update()
        {
            completeAllJobs();
        }

        private void LateUpdate()
        {
            scheduleAllJobs();
        }

        private void completeAllJobs()
        {
            if (!_hasJobToComplete) {
                return;
            }
            _jobHandle.Complete();
            Raycasts.Complete();
            _hasJobToComplete = false;
            //Logger.LogInfo($"Complete");
        }

        private void scheduleAllJobs()
        {
            JobHandle handle = new JobHandle();
            Raycasts.Schedule(handle);
            _jobHandle = handle;
            _hasJobToComplete = true;
            //Logger.LogInfo($"Scheduled");
        }

        private void OnDestroy()
        {
            completeAllJobs();
            Raycasts.Dispose();
        }

        public void Add(AbstractJobObject jobData, EJobType type)
        {
            //Logger.LogDebug($"Added {type} to jobs");
            switch (type) {
                case EJobType.Raycast:
                    Raycasts.Add(jobData as RaycastObject);
                    break;

                default:
                    break;
            }
        }

        public void Remove(AbstractJobObject jobData, EJobType type)
        {
            //Logger.LogDebug($"Removed {type} to jobs");
            switch (type) {
                case EJobType.Raycast:
                    Raycasts.Remove(jobData as RaycastObject);
                    break;

                default:
                    break;
            }
        }
    }
}