using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public static class JobManager
    {
        // private static readonly Dictionary<EJobType, SAINJobBase> _jobs = new Dictionary<EJobType, SAINJobBase>();

        public static RaycastTypeManager Raycasts = new RaycastTypeManager();

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
            Raycasts.Complete();
        }

        private static void scheduleAllJobs()
        {
            Raycasts.Schedule();
        }

        public static void Add(AbstractJobData jobData, EJobType type)
        {
            switch (type) {
                case EJobType.Raycast:
                    Raycasts.Add(jobData as RaycastData);
                    break;

                case EJobType.Directional:
                    break;

                default:
                    break;
            }
        }

        public static void Remove(AbstractJobData jobData, EJobType type)
        {
            switch (type) {
                case EJobType.Raycast:
                    Raycasts.Remove(jobData as RaycastData);
                    break;

                case EJobType.Directional:
                    break;

                default:
                    break;
            }
        }
    }
}