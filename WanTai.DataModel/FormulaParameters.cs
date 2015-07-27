using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WanTai.DataModel
{
    public class FormulaParameters
    {
        /// <summary>
        /// 该轮次做几个检测项
        /// </summary>
        public int TestItemCountInTotal;

        /// <summary>
        /// 96孔数
        /// </summary>
        public int PoolCountInTotal;

        /// <summary>
        /// 各检测项的混样数，键：检测项，值：检测项混样数
        /// HIV 30
        /// HCV 25
        /// </summary>
        public Dictionary<string,int> PoolCountOfTestItem;

        /// <summary>
        /// 混样WorkList行数
        /// </summary>
        public int PoolingWorkListRowCount;

        /// <summary>
        /// PCR加模板WorkList行数
        /// </summary>
        public int PCRWorkListRowCount;

        /// <summary>
        /// PCR板数
        /// </summary>
        public int PCRPlatesCount;
    }
}
