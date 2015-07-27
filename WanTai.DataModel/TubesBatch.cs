using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WanTai.DataModel
{
    public partial class TubesBatch
    {
        //public int HBVNumber { get; set; }
        //public int HCVNumber { get; set; }
        //public int HIVNumber { get; set; }

        public Dictionary<Guid, int> TestingItem{get;set;}
    }
}
