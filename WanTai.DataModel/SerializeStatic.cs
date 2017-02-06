using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Data;
using WanTai.DataModel;
using WanTai.DataModel.Configuration;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace WanTai.DataModel
{
    [Serializable]  
    public class SerializeStatic
    {
        public string LoginName;
        public Guid ExperimentID;// 平台ID
        public RotationInfo PraperRotation;  // 轮次
        public ExperimentsInfo CurrentExperimentsInfo;
        public int RotationIndex;
        public Dictionary<Guid, FormulaParameters> RotationFormulaParameters;
        public int NextButIndex;
        public int BatchTimes;
        public Boolean AllowBatchMore;
        public string BatchType;  //A轮、 B轮 
        public int BatchScanTimes;
        public List<Guid> TestingItemIDs;
        public List<TubeGroup> BatchTubeGroups;
        public List<DataTable> BatchTubeList;
        public int BatchTotalHoles;
        public Dictionary<Guid, int> BatchTestingItem;
        public string WorkDeskType ;
        public string InstrumentType;
        public int WorkDeskMaxSize;
        public int LiquidCfgCount;
        public int FirstStepMixing;
        public int _NextTurnStep;
        public static int testtest = 0;
        public RotationInfo CurrentRotation;
        public bool NexRotation;

        public bool Save(string filename)
        {
            try
            {
                ExperimentID = SessionInfo.ExperimentID;// 平台ID
                PraperRotation = SessionInfo.PraperRotation;  // 轮次
                PraperRotation = new RotationInfo();
                CurrentExperimentsInfo = SessionInfo.CurrentExperimentsInfo;
                RotationIndex = SessionInfo.RotationIndex;
                RotationFormulaParameters = SessionInfo.RotationFormulaParameters;
                NextButIndex = SessionInfo.NextButIndex;
                BatchTimes = SessionInfo.BatchTimes;
                AllowBatchMore = SessionInfo.AllowBatchMore;
                BatchType = SessionInfo.BatchType;  //A轮、 B轮 
                BatchScanTimes = SessionInfo.BatchScanTimes;
                TestingItemIDs = SessionInfo.TestingItemIDs;
                BatchTubeGroups = SessionInfo.BatchTubeGroups;
                BatchTubeList = SessionInfo.BatchTubeList;
                BatchTotalHoles = SessionInfo.BatchTotalHoles;
                BatchTestingItem = SessionInfo.BatchTestingItem;
                WorkDeskType = SessionInfo.WorkDeskType;
                InstrumentType = SessionInfo.InstrumentType;
                WorkDeskMaxSize = SessionInfo.WorkDeskMaxSize;
                LiquidCfgCount = SessionInfo.LiquidCfgCount;
                FirstStepMixing = SessionInfo.FirstStepMixing;
                _NextTurnStep = SessionInfo.NextTurnStep;
                CurrentRotation = SessionInfo.CurrentRotation;
                NexRotation = SessionInfo.NexRotation;
        
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, this);
                stream.Close();  

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Load(string filename)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                stream.Position = 0;
                SerializeStatic obj = (SerializeStatic)formatter.Deserialize(stream);

                SessionInfo.ExperimentID = obj.ExperimentID;// 平台ID
                SessionInfo.PraperRotation = obj.PraperRotation;  // 轮次
                SessionInfo.CurrentExperimentsInfo = obj.CurrentExperimentsInfo;
                SessionInfo.RotationIndex = obj.RotationIndex;
                SessionInfo.RotationFormulaParameters = obj.RotationFormulaParameters;
                SessionInfo.NextButIndex = obj.NextButIndex;
                SessionInfo.BatchTimes = obj.BatchTimes;
                SessionInfo.AllowBatchMore = obj.AllowBatchMore;
                SessionInfo.BatchType = obj.BatchType;  //A轮、 B轮 
                SessionInfo.BatchScanTimes = obj.BatchScanTimes;
                SessionInfo.TestingItemIDs = obj.TestingItemIDs;
                SessionInfo.BatchTubeGroups = obj.BatchTubeGroups;
                SessionInfo.BatchTubeList = obj.BatchTubeList;
                SessionInfo.BatchTotalHoles = obj.BatchTotalHoles;
                SessionInfo.BatchTestingItem = obj.BatchTestingItem;
                SessionInfo.WorkDeskType = obj.WorkDeskType;
                SessionInfo.InstrumentType = obj.InstrumentType;
                SessionInfo.WorkDeskMaxSize = obj.WorkDeskMaxSize;
                SessionInfo.LiquidCfgCount = obj.LiquidCfgCount;
                SessionInfo.FirstStepMixing = obj.FirstStepMixing;
                SessionInfo.NextTurnStep = obj._NextTurnStep;
                SessionInfo.CurrentRotation = obj.CurrentRotation;
                SessionInfo.NexRotation = obj.NexRotation;

                stream.Close();

                File.Delete(filename);


                return true;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
