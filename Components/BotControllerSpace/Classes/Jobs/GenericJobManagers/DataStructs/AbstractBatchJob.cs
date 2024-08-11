using System;
using System.Collections.Generic;

namespace SAIN.Components
{
    public abstract class AbstractBatchJob<T> : AbstractJobData where T : AbstractJobData
    {
        public int ActiveCount { get; set; }
        public readonly List<T> Datas = new List<T>();

        private int _completeCount;

        protected void Add(T data)
        {
            data.OnCompleted += checkComplete;
            Datas.Add(data);
        }

        protected void SetupJob(int count)
        {
            _completeCount = 0;
            ActiveCount = count;
            createCache(count);
            Status = EJobStatus.UnScheduled;
        }

        private void checkComplete(AbstractJobData data)
        {
            _completeCount++;
            if (_completeCount == ActiveCount) {
                Status = EJobStatus.Complete;
            }
        }

        private void createCache(int targetCount)
        {
            int cacheCount = Datas.Count;
            if (cacheCount >= targetCount) {
                return;
            }
            // This is not optimal, but the loops are making my fucking brain hurt
            while (cacheCount < targetCount) {
                Add((T)Activator.CreateInstance(typeof(T)));
                cacheCount++;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Logger.LogDebug("Disposed BatchJob");
            foreach (var data in Datas) {
                data.OnCompleted -= checkComplete;
                data.Dispose();
            }
        }
    }
}