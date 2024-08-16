using SAIN.Helpers;
using System;
using System.Collections.Generic;

namespace SAIN.Components
{
    public abstract class AbstractBatchJob<T> : AbstractJobObject where T : AbstractJobObject
    {
        public event Action<T, int> OnItemAdded;

        public int ActiveCount { get; set; }
        public readonly List<T> Datas = new List<T>();
        public readonly EJobType JobType;
        public readonly ListCache<T> Cache;

        private int _completeCount;

        public AbstractBatchJob(EJobType type, ListCache<T> cache)
        {
            Cache = cache;
            JobType = type;
        }

        protected void Add(T data)
        {
            data.OnCompleted += checkComplete;
            data.SetAsReady();
            JobManager.Instance.Add(data, JobType);
            OnItemAdded?.Invoke(data, Datas.Count - 1);
        }

        protected void Remove(T data)
        {
            data.OnCompleted -= checkComplete;
            data.SetAsCached();
            JobManager.Instance.Remove(data, JobType);
        }

        protected void SetupJob(int count)
        {
            _completeCount = 0;
            ActiveCount = count;

            ReturnAllToCache();
            Cache.HandleCache(Datas, count);
            foreach (var data in Datas) {
                Add(data);
            }
            //foreach (var data in cache.Removed) {
            //    Remove(data);
            //}
            Status = EJobStatus.UnScheduled;
        }

        public void ReturnAllToCache()
        {
            foreach (var data in Datas) {
                Remove(data);
            }
            Cache.ReturnAllToCache(Datas);
            if (Datas.Count > 0) {
                Logger.LogWarning("Datas.Count > 0");
            }
        }

        private void checkComplete(AbstractJobObject data)
        {
            _completeCount++;
            if (_completeCount == ActiveCount) {
                Status = EJobStatus.Complete;
            }
        }

        public override void Dispose()
        {
            ReturnAllToCache();
            base.Dispose();
        }
    }
}