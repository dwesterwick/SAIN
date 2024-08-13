using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public static class JobManager
    {
        // private static readonly Dictionary<EJobType, SAINJobBase> _jobs = new Dictionary<EJobType, SAINJobBase>();

        public static DistanceTypeManager Distances = new DistanceTypeManager();
        public static RaycastTypeManager Raycasts = new RaycastTypeManager();
        public static DirectionTypeManager Directions = new DirectionTypeManager();
        public static BiDirectionalTypeManager BiDirections = new BiDirectionalTypeManager();

        public static void Init()
        {
        }

        public static void Update()
        {
        }

        public static void LateUpdate()
        {
            completeAllJobs();
            scheduleAllJobs();
        }

        private static void completeAllJobs()
        {
            Distances.Complete();
            Directions.Complete();
            BiDirections.Complete();
            Raycasts.Complete();
        }

        private static void scheduleAllJobs()
        {
            Distances.Schedule();
            Directions.Schedule();
            BiDirections.Schedule();
            Raycasts.Schedule();
        }

        public static void Add(AbstractJobObject jobData, EJobType type)
        {
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

        public static void Remove(AbstractJobObject jobData, EJobType type)
        {
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