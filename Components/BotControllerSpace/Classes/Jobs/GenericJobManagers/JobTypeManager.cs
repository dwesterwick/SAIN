using System;
using System.Collections.Generic;

namespace SAIN.Components
{
    public class JobTypeManager<T, K> where T : SAINJobBase where K : AbstractJobData
    {
        public JobTypeManager(T job)
        {
            Job = job;
        }

        public readonly T Job;
        public readonly List<K> Datas = new List<K>();

        public virtual void Complete()
        {
            Job.Complete();
        }

        public virtual void Schedule()
        {
        }

        protected bool ShallComplete()
        {
            if (Job.Status == EJobStatus.Complete) {
                return false;
            }
            if (Datas.Count == 0) {
                return false;
            }
            return true;
        }

        protected bool ShallSchedule()
        {
            if (Job.Status == EJobStatus.Scheduled) {
                return false;
            }

            if (!Job.ShallCalculate()) {
                return false;
            }

            int count = Datas.Count;
            if (count == 0) {
                return false;
            }
            return true;
        }

        public void Add(K data)
        {
            if (Datas.Contains(data)) {
                Logger.LogError($"Data already added to {typeof(K)} batch list!");
                return;
            }
            Datas.Add(data);
            Logger.LogDebug($"Added data to {typeof(K)} batch");
        }

        public void Remove(K data)
        {
            if (!Datas.Contains(data)) {
                Logger.LogError($"Data not in {typeof(K)} batch List!");
                return;
            }
            Logger.LogDebug($"Removed data from {typeof(K)} batch");
            Datas.Remove(data);
        }
    }
}