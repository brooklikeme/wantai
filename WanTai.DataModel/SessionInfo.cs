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
        public static Dictionary<Guid, FormulaParameters> RotationFormulaParameters { get; set; }
        public static int NextButIndex { get; set; }
        public static int RotationIndex { get; set; }
        public static int BatchTimes { get; set; }
        public static Boolean AllowBatchMore {get; set;}
        public static string BatchType { get; set; }  //"1,2,3,4,5,6"
        public static int BatchScanTimes { get; set; }
        public static List<Guid> TestingItemIDs { get; set; }
        public static List<TubeGroup> BatchTubeGroups { get; set; }
        public static List<DataTable> BatchTubeList { get; set; }
        public static int BatchTotalHoles { get; set; }
        public static Dictionary<Guid, int> BatchTestingItem { get; set; }

        public static string WorkDeskType { get; set; }

        public static Dictionary<string, string> SystemConfigurations { get; set; }

        public static string GetSystemConfiguration(string ItemCode) {
            if (SystemConfigurations.ContainsKey(ItemCode))
            {
                return SystemConfigurations[ItemCode];
            }
            return null;
        }

        public static int WorkDeskMaxSize { get; set; }
        public static int LiquidCfgCount { get; set; }

        public static int FirstStepMixing { get; set; }

        public static bool ResumeExecution { get; set; }

        public static bool WaitForSuspend { get; set; }

        public static RotationInfo CurrentRotation { get; set; }

        public static bool NexRotation { get; set; }


        /**************在提取运行 TEST_3_TIQUANDMIX 脚本时，判断是否要跳转到扫描，-1时开始扫描文件 TECAN\EVOware\output\NextTurnStep.csv，
         *如果扫描到 0时跳转，
         *-1: 默认, 0:收到脚本消息，扫描进行中, 1:扫描分组完成，发送消息给脚本, 2:二次上样扫描进行中, 3:二次上样扫描完成
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
