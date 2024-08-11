using System.Collections.Generic;

namespace SAIN.Components
{
    public class BiDirectionalBatchJob : AbstractBatchJob<BiDirectionData>
    {
        public int Schedule(List<BiDirectionData> datas)
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

        public BiDirectionalBatchJob() : base(EJobType.BiDirectional)
        {
        }
    }
}