using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WanTai.DataModel;
using WanTai.DataModel.Configuration;
using System.Data;
namespace WanTai.DataModel
{
    public class NATInfo
    {
        public Guid ExperimentID { get; set; } // 平台ID
        public int RotationNumber { get; set; }  // 轮次数
        public DateTime StartTime { get; set; }  
        public int NC { get; set; }  
        public int PC { get; set; }
        public int Complement { get; set; }
        public int Sample { get; set; }
        public int QC { get; set; }  
        public int Mix { get; set; }  
        public int Split { get; set; }  
        public int Single { get; set; }  
        public int ReagentTheory { get; set; }  
        public int ReagentCost { get; set; }  
        public int ReagentTotal { get; set; }  
        public int Diti1000 { get; set; }  
        public int Diti200 { get; set; }  
        public int DW96 { get; set; }  
        public int Microtiter { get; set; }  
        public double ReagentSlot { get; set; }  
        public int PCR { get; set; }  
    }
}
