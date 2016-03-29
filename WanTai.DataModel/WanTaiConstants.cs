using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WanTai.DataModel
{
    /// <summary>
    ///  0血管、1补液、2阳性、3阴性
    /// </summary>
    public enum Tubetype
    {
        IsNull=-1,
        Tube = 0,
        Complement = 1,
        PositiveControl = 2,
        NegativeControl = 3,
        QC = 4
    }

    public enum LogInfoLevelEnum
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Operate = 10,
        EVORunTime = 15,
        KingFisher=20
    }    

    public enum OperationType
    {
        Single = 0,
        Grouping = 1
    }

    public enum RotationInfoStatus
    {
        Create = 0,
        Processing = 10,
        Suspend=15,
        Finish = 20,
        Stop = 25,
        Fail = 30
    }

    public enum RotationOperateStatus
    {
        Create = 0,
        Processing = 10,
        Suspend=15,
        Finish = 20,
        Stop = 25,
        Fail = 30
    }
    /// <summary>
    /// 0 启动 1 暂停 2 继续(执行完剩余脚本才结束方法) 10 出错后继续 11出错后重新启动
    /// </summary>
    public enum ExperimentRunStatus
    {
        Start = 0,
        Suspend = 1,
        Continue = 2,
        Recover = 10,
        ReStart = 11,
        Stop = 20,
        Finish = 100
    }
    public enum PlateType
    {
        Mix_Plate = 0,
        Extracting_Plate = 1,
        PCR_Plate = 2
    }

    public enum ExperimentStatus
    {
        Create = 0,
        Processing = 10,
        Suspend = 15,
        Finish = 20,
        Stop = 25,
        Fail = 30
    }

    public class PageSetting
    {
        public static int RowNumberInEachPage = 25;        
    }

    public class DiTiType 
    {
        public static short DiTi1000=104;
        public static short DiTi200 = 105;
    }

    public class PlateName
    {
        public static string DWPlate1 = "DW 96 Plate 1";
        public static string DWPlate2 = "DW 96 Plate 2";
        public static string DWPlate5 = "DW 96 Plate 5";
        public static string PCRPlate = "PCR Plate";
    }

    public class ReagentType
    {
        public static int GeneralReagent = 0;
    }

    public class AccessAuthority
    {
        public static string All = "All";
        public static string Self = "Self";
        public static string ExperimentHistory = "ExperimentHistory";
        public static string UserInfo = "UserInfo";
        public static string LogInfo = "LogInfo";
    }

    public class ConsumptionType
    {
        public static short Need = 0;
        public static short consume = 1;
        public static short Add = 2;
        public static short FirstAdd = 3;
    }

    public class PCRTest
    {
        public static string NegativeResult = "阴性";
        public static string PositiveResult = "阳性";
        public static string InvalidResult = "重新测定";
        public static string NoResult = "计算无结果";
    }

    public class WantagColor
    {
        public static string WantagYellow = "#F1F75A";
        public static string WantagRed = "#CF0012";
        public static string WantagGreen = "#24C388";
        public static string WantagWhite = "#FFFFFF";
    }
}
