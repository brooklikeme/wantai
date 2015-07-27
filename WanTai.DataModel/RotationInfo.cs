using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace WanTai.DataModel
{
    public partial class RotationInfo
    {
        public int RunIndex { get; set; }
        public int CurrentOperationIndex { get; set; }
        public RotationOperate CurrentRotationOperate { get; set; }
        public string CurrentOperationColtrlName { get; set; }
        public int RotationTotalRunTime { get; set; }
        public List<OperationConfiguration> Operations { get; set; }
    }
}
