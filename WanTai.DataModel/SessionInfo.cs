using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using WanTai.DataModel;
using WanTai.DataModel.Configuration;
namespace WanTai.DataModel
{
    public static class SessionInfo
    {
        public static string LoginName { get; set; }
        public static Guid ExperimentID { get; set; }
        public static RotationInfo PraperRotation { get; set; }
        public static List<LiquidType> LiquidTypeList { get; set; }
        public static ExperimentsInfo CurrentExperimentsInfo { get; set; }
        public static int BatchIndex { get; set; }
        public static Dictionary<Guid, FormulaParameters> RotationFormulaParameters { get; set; }
        public static int NextButIndex { get; set; }
        public static string BatchType { get; set; }

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
