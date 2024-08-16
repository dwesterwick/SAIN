using SAIN.Helpers;
using System.Collections.Generic;

namespace SAIN.Components
{
    public class BiDirectionalBatchJob : AbstractBatchJob<BiDirectionObject>
    {
        public int Schedule(List<BiDirectionObject> datas)
        {
            if (!base.CanBeScheduled()) {
                return 0;
            }
            int count = datas.Count;
            if (count < 0) {
                return 0;
            }
            base.SetupJob(count);
            for (int i = 0; i < count; i++) {
                Datas[i].UpdateData(datas[i].Data);
            }
            return count;
        }

        public override void Dispose()
        {
            base.Dispose();
            base.ReturnAllToCache();
        }

        public BiDirectionalBatchJob(ListCache<BiDirectionObject> cache) : base(EJobType.BiDirectional, cache)
        {
        }
    }
}