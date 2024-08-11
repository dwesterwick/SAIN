using System;
using System.Collections.Generic;

namespace SAIN.Components
{
    public class JobTypeManager<T, K> where T : SAINJobBase where K : AbstractJobData
    {
        public JobTypeManager(T sainJob)
        {
            SAINJob = sainJob;
        }

        public readonly T SAINJob;
        public readonly List<K> Datas = new List<K>();

        public virtual void Complete()
        {
            SAINJob.Complete();
        }

        public virtual void Schedule()
        {
        }

        protected bool ShallComplete()
        {
            if (SAINJob.Status == EJobStatus.Complete) {
                return false;
            }
            if (Datas.Count == 0) {
                return false;
            }
            return true;
        }

        protected bool ShallSchedule()
        {
            if (SAINJob.Status == EJobStatus.Scheduled) {
                return false;
            }

            if (!SAINJob.ShallCalculate()) {
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
                Logger.LogError($"Data already added to list!");
                return;
            }
            Datas.Add(data);
            Logger.LogDebug($"Added data to Raycasts");
        }

        public void Remove(K data)
        {
            if (!Datas.Contains(data)) {
                Logger.LogError($"Data not in List!");
                return;
            }
            Logger.LogDebug($"Removed data from Raycasts");
            Datas.Remove(data);
        }
    }
}