using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public static class JobManager
    {
        private static readonly Dictionary<string, SAINJobBase> _jobs = new Dictionary<string, SAINJobBase>();

        public static void Update()
        {
            int frameCount = Time.frameCount;
            foreach (var job in _jobs.Values) {
                if (job.IsComplete) continue;
                if (job.FrameCreated < frameCount ||
                    job.Handle.IsCompleted) {
                    job.Handle.Complete();
                    job.IsComplete = true;
                }
            }
            RaycastManager.Update();
        }

        public static void AddJob(string name, SAINJobBase job)
        {
            if (!_jobs.ContainsKey(name)) {
                _jobs.Add(name, job);
            }
            else {
                Logger.LogError($"{name} already in dictionary");
            }
        }

        public static void RemoveJob(string name)
        {
            _jobs.Remove(name);
        }
    }
}