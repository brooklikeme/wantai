using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WanTai.DataModel
{
    public partial class TubeGroup
    {
        public string TubesGroupName { get; set; }
        public string PoolingRulesName { get; set; }
        public string TubesPosition { get; set; }
        public string TestintItemName { get; set; }
        public int RowIndex { get; set; }
        public bool isComplement { get; set; }
        public int TubesNumber { get; set; }
        public int PoolingRulesTubesNumber { get; set; }
       // public List<TestingItemConfiguration> TestingItemConfigurations { get; set; }
    }
}
