using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components
{
    public class JobManager : MonoBehaviour
    {
        public static JobManager Instance { get; private set; }

        public DistanceTypeManager Distances = new DistanceTypeManager();
        public RaycastTypeManager Raycasts = new RaycastTypeManager();
        public DirectionTypeManager Directions = new DirectionTypeManager();
        public BiDirectionalTypeManager BiDirections = new BiDirectionalTypeManager();

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
            Distances.Complete();
            Directions.Complete();
            BiDirections.Complete();
            Raycasts.Complete();
            _hasJobToComplete = false;
            //Logger.LogInfo($"Complete");
        }

        private void scheduleAllJobs()
        {
            JobHandle handle = new JobHandle();
            Distances.Schedule(handle);
            Directions.Schedule(handle);
            BiDirections.Schedule(handle);
            Raycasts.Schedule(handle);
            _jobHandle = handle;
            _hasJobToComplete = true;
            //Logger.LogInfo($"Scheduled");
        }

        private void OnDestroy()
        {
            completeAllJobs();
            Distances.Dispose();
            Directions.Dispose();
            BiDirections.Dispose();
            Raycasts.Dispose();
        }

        public void Add(AbstractJobObject jobData, EJobType type)
        {
            Logger.LogDebug($"Added {type} to jobs");
            switch (type) {
                case EJobType.Distance:
                    Distances.Add(jobData as DistanceObject);
                    break;

                case EJobType.Raycast:
                    Raycasts.Add(jobData as RaycastObject);
                    break;

                case EJobType.Directional:
                    Directions.Add(jobData as DirectionObject);
                    break;

                case EJobType.BiDirectional:
                    BiDirections.Add(jobData as BiDirectionObject);
                    break;

                default:
                    break;
            }
        }

        public void Remove(AbstractJobObject jobData, EJobType type)
        {
            Logger.LogDebug($"Removed {type} to jobs");
            switch (type) {
                case EJobType.Distance:
                    Distances.Remove(jobData as DistanceObject);
                    break;

                case EJobType.Raycast:
                    Raycasts.Remove(jobData as RaycastObject);
                    break;

                case EJobType.Directional:
                    Directions.Remove(jobData as DirectionObject);
                    break;

                case EJobType.BiDirectional:
                    BiDirections.Remove(jobData as BiDirectionObject);
                    break;

                default:
                    break;
            }
        }
    }
}