using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using WanTai.DataModel.Configuration;

namespace WanTai.Common
{
    public class Configuration
    {
        protected static string LOGON_USERNAME = "Username";
        protected static string LOGON_PASSWORD = "Password";
        protected static string EvoWarePath = "EvoWarePath";
        protected static string LOGON_SIMULATION = "Simulation";
        protected static string EvoOutputPath = "EvoOutputPath";
        protected static string ScanResultFileName = "ScanResultFileName";
        protected static string EvoScriptFileLocation = "EvoScriptFileLocation";
        protected static string WanTaiConnectionString = "WanTaiConnectionString";
        protected static string KingFisherConnectionString = "KingFisherConnectionString";
        protected static string TubesScanScriptName = "TubesScanScriptName";

        protected static string SampleTrackingFileLocation= "SampleTracking";        
        protected static string LiquidDetection = "LiquidDetection";
        protected static string PCRDetection = "PCRDetection";
        protected static string MinVolume = "MinVolume";
        protected static string WarningSleepTime = "WarningSleepTime";
        protected static string KingFisherScriptName = "KingFisherScriptName";
        protected static string PlatesBarCodes = "PlatesBarCodes";
        protected static string ScanTubesColumnNumberFileName = "ScanTubesColumnNumberFileName";
        protected static string TecanRestorationScriptName = "TecanRestorationScriptName";
        protected static string NextTurnStepScriptName = "NextTurnStepScriptName";
        public static string GetNextTurnStepScriptName()
        {
            return ConfigurationManager.AppSettings[NextTurnStepScriptName];
        }
        public static string GetTecanRestorationScriptName()
        {
            return ConfigurationManager.AppSettings[TecanRestorationScriptName];
        }
        public static string GetScanTubesColumnNumberFileName()
        {
            return ConfigurationManager.AppSettings[ScanTubesColumnNumberFileName];
        }
        public static string GetPlatesBarCodesFile()
        {
            return ConfigurationManager.AppSettings[PlatesBarCodes];
        }
        public static string GetSampleTrackingOutPutPath()
        {
            return GetEvoOutputPath() + GetSampleTrackingFileLocation();
        }
        public static string GetKingFisherScriptName()
        {
            return ConfigurationManager.AppSettings[KingFisherScriptName];
        }
        public static string GetLogonUserName()
        {
            return ConfigurationManager.AppSettings[LOGON_USERNAME];
        }

        public static string GetLogonPassword()
        {
            return ConfigurationManager.AppSettings[LOGON_PASSWORD];
        }

        public static string GetEvoWarePath()
        {
            return ConfigurationManager.AppSettings[EvoWarePath];
        }

        public static string GetEvoOutputPath()
        {
            return ConfigurationManager.AppSettings[EvoOutputPath];
        }

        public static string GetScanResultFileName()
        {
            return ConfigurationManager.AppSettings[ScanResultFileName];
        }

        public static string GetTubesScanScriptName()
        {
            return ConfigurationManager.AppSettings[TubesScanScriptName];
        }

        public static string GetLabwaresScanScriptName()
        {
            return ConfigurationManager.AppSettings["LabwaresScanScriptName"];
        }

        public static string GetLogonSimulation()
        {
            return ConfigurationManager.AppSettings[LOGON_SIMULATION];
        }

        public static string GetEvoScriptFileLocation()
        {
            return ConfigurationManager.AppSettings[EvoScriptFileLocation];
        }

        public static string GetSampleTrackingFileLocation()
        {
            return ConfigurationManager.AppSettings[SampleTrackingFileLocation];
        }        

        public static string GetLiquidDetection()
        {
            return ConfigurationManager.AppSettings[LiquidDetection];
        }

        public static string GetPCRDetection()
        {
            return ConfigurationManager.AppSettings[PCRDetection];
        }

        public static double GetMinVolume() 
        {
            return Convert.ToDouble(ConfigurationManager.AppSettings[MinVolume]);
        }

        public static int GetWarningSleepTime()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings[WarningSleepTime]);
        }

        public static List<LiquidType> GetLiquidTypes()
        {
            List<LiquidType> typeList = null;
            LiquidTypeConfigSection section = (LiquidTypeConfigSection)ConfigurationManager.GetSection("LiquidTypeSettings");
            if (section != null)
            {
                typeList = new List<LiquidType>();
                foreach (LiquidTypeElement element in section.LiquidTypeItems)
                {
                    LiquidType liquidType = new LiquidType();
                    liquidType.TypeName = element.TypeName;
                    liquidType.Color = element.Color;
                    liquidType.HasVolume = string.IsNullOrEmpty(element.HasVolume) ? false : Boolean.Parse(element.HasVolume);
                    liquidType.DefaultVolume = string.IsNullOrEmpty(element.DefaultVolume) ? 0 : int.Parse(element.DefaultVolume);
                    liquidType.CanSelectedMultiCell = string.IsNullOrEmpty(element.CanSelectedMultiCell) ? false : Boolean.Parse(element.CanSelectedMultiCell);
                    liquidType.TypeId = string.IsNullOrEmpty(element.TypeId) ? (short)1 : short.Parse(element.TypeId);
                    typeList.Add(liquidType);
                }
            }
            return typeList;
        }

        public static List<ReagentSuppliesType> GetReagentSuppliesTypes()
        {
            List<ReagentSuppliesType> typeList = null;
            ReagentSuppliesTypeConfigSection section = (ReagentSuppliesTypeConfigSection)ConfigurationManager.GetSection("ReagentSuppliesTypeSettings");
            if (section != null)
            {
                typeList = new List<ReagentSuppliesType>();
                foreach (ReagentSuppliesTypeElement element in section.ReagentSuppliesTypeItems)
                {
                    ReagentSuppliesType type = new ReagentSuppliesType();
                    type.TypeName = element.TypeName; ;
                    type.TypeId = element.TypeId;
                    type.Unit = element.Unit;
                    typeList.Add(type);
                }
            }
            return typeList;
        }

        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[WanTaiConnectionString].ConnectionString;
        }
        public static string GetKingFisherConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[KingFisherConnectionString].ConnectionString;
        }
        public static string GetWorkListFilePath()
        {
            return ConfigurationManager.AppSettings["WorkListFilePath"];
        }

        public static string GetAddSamplesWorkListFileName()
        {
            return ConfigurationManager.AppSettings["AddSamplesWorkListFileName"];
        }

        public static string GetEvoVariableOutputPath()
        {
            return ConfigurationManager.AppSettings["EvoVariableOutputPath"];
        }

        public static string GetSampleNumberFileName()
        {
            return ConfigurationManager.AppSettings["SampleNumberFileName"];
        }

        public static string GetMixSampleNumberFileName()
        {
            return ConfigurationManager.AppSettings["MixSampleNumberFileName"];
        }

        public static string GetTubeScanResultFileName()
        {
            return ConfigurationManager.AppSettings["TubeScanResultFileName"];
        }

        public static string GetEvoWareLogPath()
        {
            return ConfigurationManager.AppSettings["EvoWareLogPath"];
        }

        public static string GetCheckDoorLockStatusCommand()
        {
            return ConfigurationManager.AppSettings["CheckDoorLockStatusCommand"];
        }

        public static string GetMaintainEvoScriptName()
        {
            return ConfigurationManager.AppSettings["MaintainEvoScriptName"];
        }

        public static string GetThermoUsername()
        {
            return ConfigurationManager.AppSettings["ThermoUsername"];
        }

        public static string GetThermoPassword()
        {
            return ConfigurationManager.AppSettings["ThermoPassword"];
        }

        public static string GetThermoInstrumentName()
        {
            return ConfigurationManager.AppSettings["ThermoInstrumentName"];
        }

        public static string GetThermoPluginPath()
        {
            return ConfigurationManager.AppSettings["ThermoPluginPath"];
        }

        public static bool GetIsMock()
        {
            return Boolean.Parse(ConfigurationManager.AppSettings["IsMock"]);
        }

        public static string GoToNextTurnScripts()
        {
            return ConfigurationManager.AppSettings["GoToNextTurnScripts"];
        }
    }
}
