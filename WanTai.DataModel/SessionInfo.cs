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
    public static class SessionInfo
    {
        public static string LoginName { get; set; }
        public static Guid ExperimentID { get; set; } // 平台ID
        public static RotationInfo PraperRotation { get; set; }  // 轮次
        public static List<LiquidType> LiquidTypeList { get; set; }
        public static ExperimentsInfo CurrentExperimentsInfo { get; set; }
        public static int BatchIndex { get; set; }
        public static Dictionary<Guid, FormulaParameters> RotationFormulaParameters { get; set; }
        public static int NextButIndex { get; set; }
        public static string BatchType { get; set; }  //A轮、 B轮 
        public static List<Guid> TestingItemIDs { get; set; }
        public static List<TubeGroup> BatchATubeGroups { get; set; }
        public static DataTable BatchATubes { get; set; }

        /**************在提取运行 TEST_3_TIQUANDMIX 脚本时，判断是否要跳转到扫描，-1时开始扫描文件 TECAN\EVOware\output\NextTurnStep.csv，
         *如果扫描到 0时跳转，
         **********************************/
        private static int _NextTurnStep = -1;
        public static int NextTurnStep {
            get 
            {
                return _NextTurnStep;   
             }
            set {
                _NextTurnStep = value;
            }
        }
    }
}
