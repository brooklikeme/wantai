using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using System.Globalization;

using WanTai.DataModel;
using WanTai.DataModel.Configuration;
using WanTai.Common;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;

using NPOI.HSSF.Model; // InternalWorkbook
using NPOI.HSSF.UserModel; // HSSFWorkbook, HSSFSheet


namespace WanTai.Controller.PCR
{
    public class PCRTestResultViewListController
    {
        public List<RotationInfo> GetPCRResultRotation()
        {
            List<RotationInfo> recordList = new List<RotationInfo>();
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                //string commandText = "  select RotationID, RotationName from  RotationInfo"+
                //    " where RotationID in ( select distinct RotationID FROM PCRTestResult WHERE ExperimentID = @ExperimentID)";
                string commandText = "  select RotationID, RotationName from  RotationInfo" +
                    " where ExperimentID = @ExperimentID";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@ExperimentID", SessionInfo.ExperimentID);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read())
                            {
                                RotationInfo info = new RotationInfo();
                                info.RotationID = (Guid)reader.GetValue(0);
                                info.RotationName = reader.GetValue(1).ToString();
                                recordList.Add(info);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPCRResultRotation", SessionInfo.ExperimentID);
                throw;
            }

            return recordList;
        }

        public List<Plate> GetPCRPlateList(Guid rotationId, Guid experimentId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.Plates.Where(c => c.RotationID == rotationId && c.ExperimentID == experimentId && c.PlateType == (short)PlateType.PCR_Plate);
                    if (records.Count() > 0)
                    {
                        return records.ToList<Plate>();
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPCRPlateList", experimentId);
                throw;
            }

            return null;
        }

        public int GetSampleNumber(Guid experimentID, Guid rotationID)
        {
            int sampleNumber = 0;
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "select count(distinct Tubes.TubeID) from Tubes, TubeGroup, TubesBatch, RotationInfo "
                    + "where Tubes.TubeGroupID = TubeGroup.TubeGroupID "
                    + "and TubeGroup.TubesBatchID = TubesBatch.TubesBatchID "
                    + "and TubesBatch.TubesBatchID = RotationInfo.TubesBatchID "
                    + "and RotationInfo.RotationID = @RotationID "
                    + "and RotationInfo.ExperimentID = @ExperimentID";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@RotationID", rotationID);
                        cmd.Parameters.AddWithValue("@ExperimentID", experimentID);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            if (reader.Read())
                            {
                                sampleNumber = reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPCRResultRotation", SessionInfo.ExperimentID);
                throw;
            }

            return sampleNumber;
        }

        public void QueryTubesPCRTestResult(Guid experimentId, Guid rotationId, DataTable poolTable, DataTable sampleTable, System.Collections.Generic.Dictionary<int, string> liquidTypeDictionary, System.Windows.Media.Color redColor, System.Windows.Media.Color greenColor, out string resultMessage, out string reagent_batch, out string qc_batch, out bool has_bci, out System.Collections.Generic.Dictionary<string, int> resultDict)
        {
            resultDict = new Dictionary<string, int>();
            int positivePoolNumber = 0;
            int negativePoolNumber = 0;
            int invalidPoolNumber = 0;
            int negativeSampleNumber = 0;
            int invalidSampleNumber = 0;

            reagent_batch = "";
            qc_batch = "";
            has_bci = false;
            //try
            {
                ExperimentsInfo expInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId);

                bool ignoreSampleTracking = WanTai.Common.Configuration.GetIgnoreSampleTracking();
                DataTable middTable = poolTable.Clone();

                Dictionary<Guid, string> tubeSampleCheckResult = new Dictionary<Guid, string>();
                List<ReagentSuppliesType> reagentSuppliesTypeList = WanTai.Common.Configuration.GetReagentSuppliesTypes();
                Dictionary<string, string> reagentSuppliesTypeDic = new Dictionary<string, string>();
                foreach (ReagentSuppliesType rType in reagentSuppliesTypeList)
                {
                    reagentSuppliesTypeDic.Add(rType.TypeId, rType.TypeName);
                }

                Dictionary<string, string> testingItemCheckResultDic = new Dictionary<string, string>();
                List<Guid> SampleTrackingIDList = new List<Guid>();
                List<string> SampleTrackingSampleIDList = new List<string>();

                resultMessage = string.Empty;
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                int OperationSequence = 1;
                int PCR_Liquid_Plate_itemtype = 205;
                string PCRLiquidPlateBarCode = null;
                //check 1.mix-DW96Plate sample tracking, if the volume < required volume, record it in memory
                string commandText_Mix = "select TubeID, SampleTracking.ItemID from View_Tubes_PCRPlatePosition left join SampleTracking on"
                    + " View_Tubes_PCRPlatePosition.BarCode = SampleTracking.SampleID and"
                    + " View_Tubes_PCRPlatePosition.RotationID = SampleTracking.RotationID and"
                    + " View_Tubes_PCRPlatePosition.ExperimentID = SampleTracking.ExperimentsID and"
                    + " View_Tubes_PCRPlatePosition.DWPlateBarCode = SampleTracking.RackID and"
                    + " SampleTracking.OperationSequence=@OperationSequence"
                    + " WHERE View_Tubes_PCRPlatePosition.RotationID=@RotationID"
                    + " and View_Tubes_PCRPlatePosition.ExperimentID = @ExperimentID"
                    + " and (SampleTracking.ItemID is null or View_Tubes_PCRPlatePosition.volume>ceiling(SampleTracking.CONCENTRATION*SampleTracking.VOLUME))";

                string commandText_AddLiquidToMix = "select TubeID, EnglishName, SampleID from View_Tubes_PCRPlatePosition"
                    + " inner join ( Select * from ( select SampleTracking.SampleID, SUM(ceiling(SampleTracking.CONCENTRATION*SampleTracking.VOLUME)) as volume,"
                    + " ReagentAndSuppliesConfiguration.EnglishName, SampleTracking.Position, ReagentAndSuppliesConfiguration.SimpleTrackingVolumn as SimpleTrackingVolumn"
                    + " from SampleTracking left join ReagentAndSuppliesConfiguration"
                    + " on charindex(ReagentAndSuppliesConfiguration.BarcodePrefix, SampleTracking.SampleID)=1"
                    + " and ReagentAndSuppliesConfiguration.ItemType=0 and ReagentAndSuppliesConfiguration.WorkDeskType=@WorkDeskType WHERE SampleTracking.RotationID=@RotationID"
                    + " and SampleTracking.OperationSequence=@OperationSequence group by SampleTracking.SampleID, ReagentAndSuppliesConfiguration.EnglishName,"
                    + " SampleTracking.Position, ReagentAndSuppliesConfiguration.SimpleTrackingVolumn) as a"
                    + " where volume<SimpleTrackingVolumn ) as s on View_Tubes_PCRPlatePosition.DWPosition = dbo.ChangeCharacterToPositionNumber(s.Position)"
                    + " where  View_Tubes_PCRPlatePosition.RotationID=@RotationID"
                    + " and View_Tubes_PCRPlatePosition.ExperimentID = @ExperimentID";

                string commandText_PCRLiquid = "select SampleTracking.ItemID, r.ItemType from SampleTracking inner join"
                    + " (select ReagentsAndSuppliesConsumption.Volume, ReagentsAndSuppliesConsumption.RotationID, ReagentAndSupplies.BarCode,"
                    + " ReagentAndSupplies.ItemType from ReagentsAndSuppliesConsumption inner join ReagentAndSupplies"
                    + " on ReagentsAndSuppliesConsumption.ReagentAndSupplieID = ReagentAndSupplies.ItemID and ReagentsAndSuppliesConsumption.VolumeType=0) as r"
                    + " on SampleTracking.RotationID = r.RotationID and SampleTracking.SampleID = @PCRLiquidPlateBarCode +'_'+r.BarCode"
                    + " where SampleTracking.RotationID=@RotationID"
                    + " and SampleTracking.ExperimentsID = @ExperimentID"
                    + " and SampleTracking.OperationSequence=@OperationSequence"
                    + " and ceiling(ABS((SampleTracking.CONCENTRATION*SampleTracking.VOLUME))) < r.Volume";

                string commandText_FenZhuang = "select View_Tubes_PCRPlatePosition.TubeID, SampleTracking.ItemID from View_Tubes_PCRPlatePosition left join SampleTracking on"
                    + " View_Tubes_PCRPlatePosition.PCRPlateBarcode = SampleTracking.RackID and View_Tubes_PCRPlatePosition.PCRPosition = dbo.ChangeCharacterToPositionNumber(SampleTracking.Position)"
                    + " and View_Tubes_PCRPlatePosition.RotationID = SampleTracking.RotationID"
                    + " and SampleTracking.CavityID = SampleTracking.SampleID"
                    + " and SampleTracking.OperationSequence = @OperationSequence"
                    + " where View_Tubes_PCRPlatePosition.RotationID =@RotationID"
                    + " and View_Tubes_PCRPlatePosition.ExperimentID = @ExperimentID"
                    + " and (SampleTracking.ItemID is null or (SampleTracking.CONCENTRATION = 0 and ABS(SampleTracking.VOLUME) < 50 ))";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    //1 混样
                    using (SqlCommand cmd = new SqlCommand(commandText_Mix, conn))
                    {
                        cmd.Parameters.AddWithValue("@RotationID", rotationId);
                        cmd.Parameters.AddWithValue("@ExperimentID", experimentId);
                        cmd.Parameters.AddWithValue("@OperationSequence", OperationSequence);

                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read() && !ignoreSampleTracking)
                            {
                                if (reader["TubeID"] != DBNull.Value && !tubeSampleCheckResult.ContainsKey((Guid)reader["TubeID"]))
                                {
                                    tubeSampleCheckResult.Add((Guid)reader["TubeID"], "加样不足");
                                }

                                if (reader["ItemID"] != DBNull.Value)
                                {
                                    SampleTrackingIDList.Add((Guid)reader["ItemID"]);
                                }
                            }
                        }
                    }
                    //2 加试剂
                    OperationSequence = 2;
                    using (SqlCommand cmd = new SqlCommand(commandText_AddLiquidToMix, conn))
                    {
                        cmd.Parameters.AddWithValue("@RotationID", rotationId);
                        cmd.Parameters.AddWithValue("@ExperimentID", experimentId);
                        cmd.Parameters.AddWithValue("@OperationSequence", OperationSequence);
                        cmd.Parameters.AddWithValue("@WorkDeskType", SessionInfo.WorkDeskType);

                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read() && !ignoreSampleTracking)
                            {
                                if (!tubeSampleCheckResult.ContainsKey((Guid)reader["TubeID"]))
                                {
                                    tubeSampleCheckResult.Add((Guid)reader["TubeID"], "试剂" + reader["EnglishName"].ToString() + "吸取量不足");
                                }
                                else
                                {
                                    string message = tubeSampleCheckResult[(Guid)reader["TubeID"]];
                                    if (message.IndexOf("试剂" + reader["EnglishName"].ToString() + "吸取量不足") < 0)
                                        tubeSampleCheckResult[(Guid)reader["TubeID"]] = message + Environment.NewLine + "试剂" + reader["EnglishName"].ToString() + "吸取量不足";
                                }

                                if (reader["SampleID"] != DBNull.Value && !SampleTrackingSampleIDList.Contains((string)reader["SampleID"]))
                                {
                                    SampleTrackingSampleIDList.Add((string)reader["SampleID"]);
                                }
                            }
                        }
                    }

                    //2 PCR配液-get PCRLiquidPlateBarCode
                    OperationSequence = 3;
                    string selectCommand = "SELECT BarcodePrefix FROM ReagentAndSuppliesConfiguration WHERE ItemType=@ItemType and WorkDeskType=@WorkDeskType";
                    using (SqlCommand cmd = new SqlCommand(selectCommand, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemType", PCR_Liquid_Plate_itemtype);
                        cmd.Parameters.AddWithValue("@WorkDeskType", SessionInfo.WorkDeskType);

                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            if (reader.Read())
                            {
                                PCRLiquidPlateBarCode = reader[0].ToString();
                            }
                        }
                    }

                    //check if can find the PCRLiquidPlate sampletracking file, if not find, then all pcr liquid is invalid,//todo
                    //2 PCR配液- get PCR liquid and its testing item is not enough
                    using (SqlCommand cmd = new SqlCommand(commandText_PCRLiquid, conn))
                    {
                        cmd.Parameters.AddWithValue("@PCRLiquidPlateBarCode", PCRLiquidPlateBarCode);
                        cmd.Parameters.AddWithValue("@RotationID", rotationId);
                        cmd.Parameters.AddWithValue("@ExperimentID", experimentId);
                        cmd.Parameters.AddWithValue("@OperationSequence", OperationSequence);

                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read() && !ignoreSampleTracking)
                            {
                                string typeName = reagentSuppliesTypeDic[reader["ItemType"].ToString()];
                                if (!testingItemCheckResultDic.ContainsKey(typeName))
                                {
                                    testingItemCheckResultDic.Add(typeName, typeName + "配液添加量不够");
                                }

                                SampleTrackingIDList.Add((Guid)reader["ItemID"]);
                            }
                        }
                    }

                    //4试剂模板分装
                    OperationSequence = 5;
                    using (SqlCommand cmd = new SqlCommand(commandText_FenZhuang, conn))
                    {
                        cmd.Parameters.AddWithValue("@RotationID", rotationId);
                        cmd.Parameters.AddWithValue("@ExperimentID", experimentId);
                        cmd.Parameters.AddWithValue("@OperationSequence", OperationSequence);

                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read() && !ignoreSampleTracking)
                            {
                                if (!tubeSampleCheckResult.ContainsKey((Guid)reader["TubeID"]))
                                {
                                    tubeSampleCheckResult.Add((Guid)reader["TubeID"], "分装不足");
                                }
                                else
                                {
                                    string message = tubeSampleCheckResult[(Guid)reader["TubeID"]];
                                    if (message.IndexOf("分装不足") < 0)
                                        tubeSampleCheckResult[(Guid)reader["TubeID"]] = message + Environment.NewLine + "分装不足";
                                }

                                if (reader["ItemID"] != DBNull.Value)
                                {
                                    SampleTrackingIDList.Add((Guid)reader["ItemID"]);
                                }
                            }
                        }
                    }

                    string updateCommand = "UPDATE SampleTracking SET IsKeeping=1 where ItemID=@ItemID";
                    using (SqlCommand cmd = new SqlCommand(updateCommand, conn))
                    {
                        foreach (Guid itemId in SampleTrackingIDList)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@ItemID", itemId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    updateCommand = "UPDATE SampleTracking SET IsKeeping=1 where SampleID=@SampleID";
                    using (SqlCommand cmd = new SqlCommand(updateCommand, conn))
                    {
                        foreach (string SampleID in SampleTrackingSampleIDList)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@SampleID", SampleID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Dictionary<string, string> pcrContentDict = new Dictionary<string, string>();
                using (WanTai.DataModel.WanTaiEntities _WanTaiEntities = new WanTaiEntities())
                {
                    List<PCRTestResult> testResultList = _WanTaiEntities.PCRTestResults.Where(s => s.RotationID == rotationId).ToList();
                    foreach (PCRTestResult testResult in testResultList)
                    {
                        if (!pcrContentDict.ContainsKey(testResult.ItemID.ToString()))
                        {
                            pcrContentDict.Add(testResult.ItemID.ToString(), testResult.PCRContent.ToString());
                        }
                    }
                }
                string commandText = "SELECT distinct TubeID, View_Tubes_PCRPlatePosition.BarCode, View_Tubes_PCRPlatePosition.Position,"
                + " Grid, BatchType, TubeType, PoolingRulesName, TestName, PCRPlateBarcode, PCRPlateID, PCRPosition, PCRTestResult.Result,"
                + " PCRTestResult.ItemID FROM View_Tubes_PCRPlatePosition left join PCRTestResult"
                + " on View_Tubes_PCRPlatePosition.RotationID = PCRTestResult.RotationID"
                + " and View_Tubes_PCRPlatePosition.PCRPosition = PCRTestResult.Position"
                + " and View_Tubes_PCRPlatePosition.PCRPlateID = PCRTestResult.PlateID"
                + " WHERE View_Tubes_PCRPlatePosition.RotationID=@RotationID order by PCRPlateID, PCRPosition";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@RotationID", rotationId);

                        cmd.CommandTimeout = 120;

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            WanTaiEntities _WanTaiEntities = new WanTaiEntities();

                            List<SystemFluidConfiguration> SystemFluid = _WanTaiEntities.SystemFluidConfigurations.ToList().Where(systemFluidConfiguration => systemFluidConfiguration.ItemType == 4).ToList();

                            int index = 1;
                            int refIndex = 1;
                            while (reader.Read())
                            {
                                System.Data.DataRow dRow = sampleTable.NewRow();
                                dRow["Number"] = index;

                                dRow["TubeID"] = reader["TubeID"];
                                if (reader["BarCode"] != DBNull.Value)
                                {
                                    dRow["TubeBarCode"] = reader["BarCode"].ToString();
                                    // fill reagent and qc batch\
                                    string tubeBarCode = dRow["TubeBarCode"].ToString();
                                    if (reagent_batch == "" && tubeBarCode.Length > 2 && (tubeBarCode.Substring(tubeBarCode.Length - 2).ToUpper() == "NC" || tubeBarCode.Substring(tubeBarCode.Length - 2).ToUpper() == "PC"))
                                    {
                                        reagent_batch = tubeBarCode.Substring(0, tubeBarCode.Length - 2);
                                    }
                                    string batchType = reader["BatchType"].ToString();
                                    if (batchType == null || batchType == "null" || batchType == "")
                                    {
                                        foreach (SystemFluidConfiguration sf in SystemFluid) {
                                            if (sf.BatchType == "1" && sf.Grid == (int)reader["Grid"] && sf.Position == (int)reader["Position"])
                                            {
                                                if (qc_batch == "")
                                                {
                                                    qc_batch = tubeBarCode;
                                                }
                                                else
                                                {
                                                    if (!qc_batch.Contains(tubeBarCode))
                                                        qc_batch = qc_batch + "\n" + tubeBarCode;
                                                }
                                            }
                                        }    
                                    }
                                    else
                                    {
                                        foreach (SystemFluidConfiguration sf in SystemFluid)
                                        {
                                            if (sf.BatchType == batchType && sf.Grid == (int)reader["Grid"] && sf.Position == (int)reader["Position"])
                                            {
                                                if (qc_batch == "")
                                                {
                                                    qc_batch = tubeBarCode;
                                                }
                                                else
                                                {
                                                    if (!qc_batch.Contains(tubeBarCode))
                                                        qc_batch = qc_batch + "\n" + tubeBarCode;
                                                }
                                            }
                                        }
                                    }
                                }

                                dRow["TubePosition"] = reader["BatchType"] == DBNull.Value ? "Tube" + reader["Grid"].ToString() + "_" + reader["Position"].ToString() : reader["BatchType"] + ":Tube" + reader["Grid"].ToString() + "_" + reader["Position"].ToString();
                                if (sampleTable.Columns.Contains("TubePositionNumber"))
                                    dRow["TubePositionNumber"] = reader["BatchType"] == DBNull.Value ? (int)reader["Grid"] * 100 + (int)reader["Position"] : System.Convert.ToInt32(reader["BatchType"].ToString()) * 10000 + (int)reader["Grid"] * 100 + (int)reader["Position"];
                                dRow["TubeType"] = reader["TubeType"];
                                dRow["TestingItemName"] = reader["TestName"];

                                string tubeTypeStr = string.Empty;
                                if ((int)dRow["TubeType"] == (int)Tubetype.PositiveControl)
                                {
                                    if (SessionInfo.WorkDeskType == "100")
                                    {
                                        tubeTypeStr = dRow["TestingItemName"].ToString() + " PC";
                                    }
                                    else
                                    {
                                        tubeTypeStr = "PC";
                                    }
                                }
                                else if ((int)dRow["TubeType"] == (int)Tubetype.NegativeControl)
                                {
                                    if (SessionInfo.WorkDeskType == "100")
                                    {
                                        tubeTypeStr = dRow["TestingItemName"].ToString() + " NC";
                                    }
                                    else
                                    {
                                        tubeTypeStr = "NC";
                                    }
                                }
                                else if ((int)dRow["TubeType"] == (int)Tubetype.Complement)
                                {
                                    if (SessionInfo.WorkDeskType == "100")
                                    {
                                        if (refIndex == 5)
                                            refIndex = 1;
                                        tubeTypeStr = dRow["TestingItemName"].ToString() + " 定量参考品" + refIndex;
                                        refIndex++;
                                    }
                                    else
                                    {
                                        tubeTypeStr = "补液";
                                    }
                                }
                                else
                                {
                                    tubeTypeStr = "样本";
                                }
                                dRow["TubeTypeName"] = tubeTypeStr;


                                if ((int)dRow["TubeType"] == (int)Tubetype.PositiveControl)
                                {
                                    dRow["TubeTypeColor"] = liquidTypeDictionary[(int)dRow["TubeType"]];
                                    dRow["TubeTypeColorVisible"] = "Visible";

                                    if (reader["TestName"] != DBNull.Value && reader["TestName"].ToString() == "M")
                                    {
                                        if (reader["Result"] != DBNull.Value &&
                                            !string.IsNullOrEmpty(reader["Result"].ToString()) && reader["Result"].ToString().Split(new string[] { PCRTest.PositiveResult }, StringSplitOptions.None).Length - 1 != 3)
                                        {
                                            resultMessage = "QC: 质控品结果不符合标准，实验无效";
                                        }
                                    }
                                    else
                                    {
                                        if (reader["Result"] != DBNull.Value &&
                                            !string.IsNullOrEmpty(reader["Result"].ToString()) && reader["Result"].ToString().Contains("重新测定"))
                                        {
                                            resultMessage = "QC: 质控品结果不符合标准，实验无效";
                                        }
                                    }
                                }
                                else if ((int)dRow["TubeType"] == (int)Tubetype.NegativeControl)
                                {
                                    dRow["TubeTypeColor"] = liquidTypeDictionary[(int)dRow["TubeType"]];
                                    dRow["TubeTypeColorVisible"] = "Visible";

                                    if (reader["TestName"] != DBNull.Value && reader["TestName"].ToString() == "M")
                                    {
                                        if (reader["Result"] != DBNull.Value &&
                                            !string.IsNullOrEmpty(reader["Result"].ToString()) && reader["Result"].ToString().Split(new string[] { PCRTest.NegativeResult }, StringSplitOptions.None).Length - 1 != 3)
                                        {
                                            resultMessage = "QC: 质控品结果不符合标准，实验无效";
                                        }
                                    }
                                    else
                                    {
                                        if (reader["Result"] != DBNull.Value &&
                                            !string.IsNullOrEmpty(reader["Result"].ToString()) && reader["Result"].ToString().Contains("重新测定"))
                                        {
                                            resultMessage = "QC: 质控品结果不符合标准，实验无效";
                                        }
                                    }
                                }
                                else
                                {
                                    dRow["TubeTypeColorVisible"] = "Hidden";
                                }

                                dRow["PoolingRuleName"] = reader["PoolingRulesName"];
                                if (reader["PCRPlateBarcode"] != DBNull.Value)
                                {
                                    dRow["PCRPlateBarCode"] = reader["PCRPlateBarcode"];
                                }

                                if (reader["PCRPosition"] != DBNull.Value)
                                {
                                    dRow["PCRPosition"] = dRow["PCRPlateBarCode"] + ":" + ChangePositionNumberToCharacter(Int32.Parse(reader["PCRPosition"].ToString()));

                                    // previousPCRPosition = (int)dRow["PCRPosition"];
                                }

                                if (reader["ItemID"] != DBNull.Value)
                                {
                                    dRow["PCRTestItemID"] = reader["ItemID"];
                                }

                                dRow["Color"] = System.Windows.Media.Colors.White;

                                string itemID = reader["ItemID"] != DBNull.Value ? reader["ItemID"].ToString() : "";
                                string pcrContent = pcrContentDict.ContainsKey(itemID.ToString()) ? pcrContentDict[itemID.ToString()] : null;

                                if (null != pcrContent)
                                {
                                    XmlDocument xdoc = new XmlDocument();
                                    xdoc.LoadXml(pcrContent);
                                    XmlNodeList nodelist = xdoc.SelectSingleNode("PCRContent").SelectNodes("Row");
                                    foreach (XmlNode node in nodelist)
                                    {
                                        dRow["PCRName"] = node.SelectSingleNode("SampleName").InnerText;
                                        string Detector = node.SelectSingleNode("Detector").InnerText;
                                        float ctNumber = 0;
                                        string ctString = node.SelectSingleNode("Ct").InnerText;
                                        if (float.TryParse(node.SelectSingleNode("Ct").InnerText, out ctNumber))
                                        {
                                            ctString = ctNumber.ToString("F2");//.Format("{0:E2}", myNumber); 
                                        }
                                        if (dRow["TestingItemName"].ToString() == "HBV")
                                        {
                                            if (SessionInfo.WorkDeskType == "100")
                                            {
                                                if ("VIC" == Detector || "HEX" == Detector)
                                                {
                                                    dRow["HBV"] = ctString;
                                                }
                                                else if ("ROX" == Detector)
                                                {
                                                    dRow["HBVIC"] = ctString;
                                                }
                                            }
                                            else
                                            {
                                                if ("VIC" == Detector || "HEX" == Detector)
                                                {
                                                    dRow["HBV"] = ctString;
                                                }
                                                else if ("ROX" == Detector)
                                                {
                                                    dRow["HBVIC"] = ctString;
                                                }
                                            }
                                        }
                                        else if (dRow["TestingItemName"].ToString() == "HCV")
                                        {
                                            if (SessionInfo.WorkDeskType == "100")
                                            {
                                                if ("ROX" == Detector)
                                                {
                                                    dRow["HCV"] = ctString;
                                                }
                                                else if ("FAM" == Detector)
                                                {
                                                    dRow["HCVIC"] = ctString;
                                                }
                                            }
                                            else
                                            {
                                                if ("FAM" == Detector)
                                                {
                                                    dRow["HCV"] = ctString;
                                                }
                                                else if ("ROX" == Detector)
                                                {
                                                    dRow["HCVIC"] = ctString;
                                                }
                                            }
                                        }
                                        else if (dRow["TestingItemName"].ToString() == "HIV")
                                        {
                                            if (SessionInfo.WorkDeskType == "100")
                                            {
                                                if ("FAM" == Detector)
                                                {
                                                    dRow["HIV"] = ctString;
                                                }
                                                else if ("VIC" == Detector || "HEX" == Detector)
                                                {
                                                    dRow["HIVIC"] = ctString;
                                                }
                                            }
                                            else
                                            {
                                                if ("FAM" == Detector)
                                                {
                                                    dRow["HIV"] = ctString;
                                                }
                                                else if ("ROX" == Detector)
                                                {
                                                    dRow["HIVIC"] = ctString;
                                                }
                                            }
                                        }
                                        else if (dRow["TestingItemName"].ToString() == "BCI")
                                        {
                                            has_bci = true;
                                            if ("ROX" == Detector)
                                            {
                                                dRow["HBV"] = ctString;
                                            }
                                            else if ("FAM" == Detector)
                                            {
                                                dRow["HCV"] = ctString;
                                            }
                                            else if ("CY5" == Detector.ToUpper())
                                            {
                                                dRow["HIV"] = ctString;
                                            }
                                            else if ("VIC" == Detector || "HEX" == Detector)
                                            {
                                                dRow["HBVIC"] = ctString;
                                                dRow["HCVIC"] = ctString;
                                                dRow["HIVIC"] = ctString;
                                            }
                                        }
                                    }
                                }

                                // set QC tube type name
                                if (dRow["PCRName"].ToString().ToUpper() == "QC")
                                {
                                    dRow["TubeTypeName"] = "QC";
                                }

                                if (reader["Result"] != DBNull.Value)
                                {
                                    dRow["PCRTestResult"] = reader["Result"];

                                    if (reader["Result"].ToString().Contains(PCRTest.PositiveResult) || reader["Result"].ToString().Contains(PCRTest.LowResult) || reader["Result"].ToString().Contains(PCRTest.BCILowResult))
                                    {
                                        dRow["Color"] = WantagColor.WantagRed;
                                    }
                                    else if (reader["Result"].ToString().Contains(PCRTest.InvalidResult) || reader["Result"].ToString().Contains(PCRTest.NoResult))
                                    {
                                        dRow["Color"] = WantagColor.WantagYellow;
                                    }

                                    if (SessionInfo.WorkDeskType == "100"
                                        && dRow["PCRTestResult"].ToString() != PCRTest.InvalidResult
                                        && dRow["PCRTestResult"].ToString() != PCRTest.NegativeResult
                                        && dRow["PCRTestResult"].ToString() != PCRTest.PositiveResult
                                        && dRow["PCRTestResult"].ToString() != PCRTest.NoResult)
                                    {
                                        float myNumber = 0;
                                        if (float.TryParse(dRow["PCRTestResult"].ToString(), out myNumber))
                                        {
                                            dRow["PCRTestResult"] = myNumber.ToString("E2");//.Format("{0:E2}", myNumber); 
                                        }
                                    }
                                    dRow["PCRTestResult"] += ((SessionInfo.WorkDeskType == "100"
                                        && (dRow["PCRTestResult"].ToString() != PCRTest.InvalidResult
                                        && dRow["PCRTestResult"].ToString() != PCRTest.NegativeResult
                                        && dRow["PCRTestResult"].ToString() != PCRTest.PositiveResult
                                        && dRow["PCRTestResult"].ToString() != PCRTest.NoResult
                                        && "No Ct" != dRow["PCRTestResult"].ToString()
                                        && "N/A" != dRow["PCRTestResult"].ToString()
                                        && "NaN" != dRow["PCRTestResult"].ToString()
                                        && "" != dRow["PCRTestResult"].ToString().Trim()
                                        && "Undetermined" != dRow["PCRTestResult"].ToString()))
                                        ? ((dRow["TestingItemName"].ToString() == "HBV" || dRow["TestingItemName"].ToString() == "HCV") ? " IU/ml" : " cps/ml") : "");
                                }

                                if (tubeSampleCheckResult.ContainsKey((Guid)reader["TubeID"]))
                                {
                                    if (expInfo.State != (short)ExperimentStatus.Finish)
                                    {
                                        if (tubeSampleCheckResult[(Guid)reader["TubeID"]].Contains("加样不足")) {
                                            dRow["SimpleTrackingResult"] = "[" + dRow["TubeBarCode"] + "]加样不足 人工操作";
                                        }
                                        else
                                        {
                                            dRow["SimpleTrackingResult"] = "人工操作";
                                        }
                                    }
                                    else
                                    {
                                        dRow["SimpleTrackingResult"] = "[" + dRow["TubeBarCode"] + "]" + tubeSampleCheckResult[(Guid)reader["TubeID"]];
                                    }

                                    if (!string.IsNullOrEmpty(dRow["SimpleTrackingResult"].ToString()) && dRow["SimpleTrackingResult"] != "人工操作")
                                    {
                                        // dRow["Color"] = n new Color(232, 231, 154);
                                        // dRow["Color"] = System.Windows.Media.Colors.Yellow; 
                                        dRow["Color"] = WantagColor.WantagYellow; //System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(232, 231, 154));
                                        // dRow["Color"] = System.Windows.Media.Colors.Yellow; 
                                    }
                                }

                                if (testingItemCheckResultDic.ContainsKey((string)reader["TestName"]))
                                {
                                    if (dRow["SimpleTrackingResult"] != null)
                                        dRow["SimpleTrackingResult"] = dRow["SimpleTrackingResult"] + Environment.NewLine + "[" + dRow["TubeBarCode"] + "]" + testingItemCheckResultDic[(string)reader["TestName"]];
                                    else
                                        dRow["SimpleTrackingResult"] = "[" + dRow["TubeBarCode"] + "]" + testingItemCheckResultDic[(string)reader["TestName"]];
                                }

                                index++;
                                sampleTable.Rows.Add(dRow);
                            }
                        }
                    }
                }

                /*
                int dataIndex = 1;
                dataTable.Columns[11].DataType = typeof(string);
                foreach (DataRow baseRow in sampleTable.Rows)
                {
                    System.Data.DataRow dataRow = dataTable.NewRow();
                    dataRow.ItemArray = baseRow.ItemArray;
                    dataTable.Rows.Add(dataRow);
                }*/

                // get pcr position dict
                Dictionary<string, List<DataRow>> pcrPosRows = new Dictionary<string, List<DataRow>>();
                foreach (DataRow baseRow in sampleTable.Rows)
                {
                    string pcrPos = baseRow["PCRPosition"].ToString();
                    if (!pcrPosRows.ContainsKey(pcrPos))
                    {
                        pcrPosRows.Add(pcrPos, new List<DataRow>());
                    }
                    pcrPosRows[pcrPos].Add(baseRow);
                }
                // get midd table
                foreach (KeyValuePair<string, List<DataRow>> pcrPosRow in pcrPosRows)
                {
                    int listIndex = 0;
                    System.Data.DataRow middRow = middTable.NewRow();
                    foreach (DataRow baseRow in pcrPosRow.Value)
                    {
                        if (listIndex == 0)
                        {
                            middRow.ItemArray = baseRow.ItemArray;
                            middTable.Rows.Add(middRow);
                        }
                        else
                        {
                            middRow["TubeBarCode"] += "\n" + baseRow["TubeBarCode"];
                            middRow["TubePosition"] += "\n" + baseRow["TubePosition"];
                        }
                        listIndex++;
                    }
                }
                // get midd dictionary
                Dictionary<string, List<DataRow>> pcrTubeRows = new Dictionary<string, List<DataRow>>();
                foreach (DataRow middRow in middTable.Rows)
                {
                    string tubePos = (string)middRow["TubePosition"] + "-" + (string)middRow["PoolingRuleName"];
                    if (!pcrTubeRows.ContainsKey(tubePos))
                    {
                        pcrTubeRows.Add(tubePos, new List<DataRow>());
                    }
                    pcrTubeRows[tubePos].Add(middRow);
                }
                // get data table
                int dataIndex = 1;
                poolTable.Columns[11].DataType = typeof(string);
                foreach (KeyValuePair<string, List<DataRow>> pcrTubeRow in pcrTubeRows)
                {
                    int listIndex = 0;
                    System.Data.DataRow dataRow = poolTable.NewRow();
                    foreach (DataRow middRow in pcrTubeRow.Value)
                    {
                        if (listIndex == 0)
                        {
                            dataRow.ItemArray = middRow.ItemArray;
                            dataRow["Number"] = dataIndex;
                            if (middRow["PCRTestResult"] != null && middRow["PCRTestResult"].ToString() != "") {
                                if (middRow["TestingItemName"].ToString() == "BCI")
                                {
                                    string [] pcrResults = dataRow["PCRTestResult"].ToString().Split('|');
                                    if (pcrResults.Length > 3)
                                    {
                                        dataRow["PCRTestResult"] = "HBV " + pcrResults[1];
                                        dataRow["PCRTestResult"] += "\nHCV " + pcrResults[2];
                                        dataRow["PCRTestResult"] += "\nHIV " + pcrResults[3];
                                    }
                                } else {
                                    dataRow["PCRTestResult"] = middRow["TestingItemName"] + " " + dataRow["PCRTestResult"];
                                }
                            }                                
                            else
                                dataRow["PCRTestResult"] = " ";
                            poolTable.Rows.Add(dataRow);
                        }
                        else
                        {
                            dataRow["TestingItemName"] += "\n" + middRow["TestingItemName"];
                            dataRow["PCRPosition"] += "\n" + middRow["PCRPosition"].ToString();
                            if (middRow["PCRTestResult"] != null && middRow["PCRTestResult"].ToString() != "")
                                dataRow["PCRTestResult"] += "\n" + middRow["TestingItemName"] + " " + middRow["PCRTestResult"];
                            else
                                dataRow["PCRTestResult"] += "\n ";
                            dataRow["HBV"] += "\n" + middRow["HBV"];
                            dataRow["HBVIC"] += "\n" + middRow["HBVIC"];
                            dataRow["HCV"] += "\n" + middRow["HCV"];
                            dataRow["HCVIC"] += "\n" + middRow["HCVIC"];
                            dataRow["HIV"] += "\n" + middRow["HIV"];
                            dataRow["HIVIC"] += "\n" + middRow["HIVIC"];
                            // check color
                            if (!string.IsNullOrEmpty(middRow["SimpleTrackingResult"].ToString()))
                            {
                                dataRow["Color"] = WantagColor.WantagYellow;
                            }
                            else if (middRow["PCRTestResult"].ToString().Contains(PCRTest.PositiveResult) || middRow["PCRTestResult"].ToString().Contains(PCRTest.LowResult) || middRow["PCRTestResult"].ToString().Contains(PCRTest.BCILowResult))
                            {
                                dataRow["Color"] = WantagColor.WantagRed;
                            }
                            else if (middRow["PCRTestResult"].ToString() == PCRTest.InvalidResult || middRow["PCRTestResult"].ToString() == PCRTest.NoResult)
                            {
                                dataRow["Color"] = WantagColor.WantagYellow;
                            }
                        }
                        listIndex++;
                        if ((int)middRow["TubeType"] != (int)Tubetype.PositiveControl && (string)middRow["TubeTypeName"] != "QC" && middRow["PCRTestResult"].ToString().Contains(PCRTest.PositiveResult))
                        {
                            positivePoolNumber++;
                        }
                        else if ((int)middRow["TubeType"] != (int)Tubetype.NegativeControl && (string)middRow["TubeTypeName"] != "QC")
                        {
                            negativePoolNumber++;
                            negativeSampleNumber += middRow["TubePosition"].ToString().Split('\n').Length;
                        }
                        if (middRow["PCRTestResult"].ToString().Contains(PCRTest.LowResult)
                            || middRow["PCRTestResult"].ToString() == PCRTest.InvalidResult || middRow["PCRTestResult"].ToString() == PCRTest.NoResult)
                        {
                            invalidPoolNumber++;
                            invalidSampleNumber += middRow["TubePosition"].ToString().Split('\n').Length;
                        }
                    }
                    // dataRow["TubeBarCode"] = formatTwoColumns(dataRow["tubeBarCode"].ToString());
                    // dataRow["TubePosition"] = formatTwoColumns(dataRow["TubePosition"].ToString());
                    dataIndex++;
                }

                //
                resultDict.Add("positivePoolNumber", positivePoolNumber);
                resultDict.Add("negativePoolNumber", negativePoolNumber);
                resultDict.Add("invalidPoolNumber", invalidPoolNumber);
                resultDict.Add("negativeSampleNumber", negativeSampleNumber);
                resultDict.Add("invalidSampleNumber", invalidSampleNumber);

                
                /*
                _pcrTable.Columns.Add("Number", typeof(int));
                _pcrTable.Columns.Add("Color", typeof(string));
                _pcrTable.Columns.Add("TubeID", typeof(Guid));
                _pcrTable.Columns.Add("TubeBarCode", typeof(string));
                _pcrTable.Columns.Add("TubePosition", typeof(string));
                _pcrTable.Columns.Add("TubeType", typeof(int));
                _pcrTable.Columns.Add("TubeTypeColor", typeof(string));
                _pcrTable.Columns.Add("TubeTypeColorVisible", typeof(string));
                _pcrTable.Columns.Add("PoolingRuleName", typeof(string));
                _pcrTable.Columns.Add("TestingItemName", typeof(string));
                _pcrTable.Columns.Add("PCRPlateBarCode", typeof(string));
                _pcrTable.Columns.Add("PCRPosition", typeof(int));
                _pcrTable.Columns.Add("PCRTestItemID", typeof(Guid));
                _pcrTable.Columns.Add("PCRTestResult", typeof(string));
                _pcrTable.Columns.Add("PCRTestContent", typeof(string));
                _pcrTable.Columns.Add("SimpleTrackingResult", typeof(string));
                 */
            }
            //catch (Exception e)
            // {
            //    string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
            //    LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->QueryTubesPCRTestResult", SessionInfo.ExperimentID);
            //    throw;
            // }
        }

        public string formatTwoColumns(string originString)
        {
            string newString = "";
            int occurTimes = 0;
            for (int i = 0; i < originString.Length; i++)
            {
                if (originString[i] == '\n')
                {
                    occurTimes += 1;
                    if (occurTimes % 2 == 1)
                    {
                        newString += "  ";
                    }
                    else
                    {
                        newString += '\n';
                    }
                }
                else
                {
                    newString += originString[i];
                }
            }
            return newString;
        }

        public string ChangePositionNumberToCharacter(int positionNumber)
        {
            int oneLineCellCount = 8;
            int number = (positionNumber - 1) % oneLineCellCount + 1;
            int lineNumber = (positionNumber - 1) / oneLineCellCount + 1;
            string character = "";
            switch (number)
            {
                case 1:
                    character = "A" + lineNumber;
                    break;
                case 2:
                    character = "B" + lineNumber;
                    break;
                case 3:
                    character = "C" + lineNumber;
                    break;
                case 4:
                    character = "D" + lineNumber;
                    break;
                case 5:
                    character = "E" + lineNumber;
                    break;
                case 6:
                    character = "F" + lineNumber;
                    break;
                case 7:
                    character = "G" + lineNumber;
                    break;
                case 8:
                    character = "H" + lineNumber;
                    break;
                default:
                    break;
            }

            return character;
        }

        public bool DeleteRotationPCRTestResult(Guid rotationId)
        {
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "DELETE FROM  PCRTestResult WHERE RotationID=@RotationID";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@RotationID", rotationId);

                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }

            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->DeleteRotationPCRTestResult", SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool CheckRotationHasPCRTestResult(Guid rotationId, Guid experimentId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.PCRTestResults.Where(c => c.RotationID == rotationId && c.ExperimentID == experimentId);
                    return records.Count() > 0 ? true : false;
                }

            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->CheckRotationHasPCRTestResult", SessionInfo.ExperimentID);
                throw;
            }
        }

        public void QueryTubesPCRTestResultByBarcode(string barcode, DataTable dataTable, System.Collections.Generic.Dictionary<int, string> liquidTypeDictionary)
        {
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();

                string commandText = "SELECT distinct TubeID, A.BarCode, A.Position, TubeType, PoolingRulesName, TestName, PCRTestResult.Result,"
                    + " Grid, PCRTestResult.ItemID, A.ExperimentID, A.ExperimentName, A.StartTime, A.EndTime, A.LoginName  FROM ( "
                    + " SELECT View_Tubes_PCRPlatePosition.*, ExperimentsInfo.ExperimentName, ExperimentsInfo.StartTime, ExperimentsInfo.EndTime,"
                    + " ExperimentsInfo.LoginName FROM View_Tubes_PCRPlatePosition inner join ExperimentsInfo on View_Tubes_PCRPlatePosition.ExperimentID=ExperimentsInfo.ExperimentID) AS A"
                    + " left join PCRTestResult on A.RotationID = PCRTestResult.RotationID and A.PCRPosition = PCRTestResult.Position and A.PCRPlateID = PCRTestResult.PlateID"
                    + " WHERE A.BarCode=@Barcode order by A.StartTime desc";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@Barcode", barcode);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            int index = 1;
                            while (reader.Read())
                            {
                                System.Data.DataRow dRow = dataTable.NewRow();
                                dRow["Number"] = index;

                                dRow["TubeID"] = reader["TubeID"];
                                if (reader["BarCode"] != DBNull.Value)
                                {
                                    dRow["TubeBarCode"] = reader["BarCode"].ToString();
                                }
                                dRow["TubePosition"] = "Tube" + reader["Grid"].ToString() + "_" + reader["Position"].ToString();
                                dRow["TubeType"] = reader["TubeType"];
                                if ((int)dRow["TubeType"] == (int)Tubetype.PositiveControl)
                                {
                                    dRow["TubeTypeColor"] = liquidTypeDictionary[(int)dRow["TubeType"]];
                                    dRow["TubeTypeColorVisible"] = "Visible";
                                }
                                else if ((int)dRow["TubeType"] == (int)Tubetype.NegativeControl)
                                {
                                    dRow["TubeTypeColor"] = liquidTypeDictionary[(int)dRow["TubeType"]];
                                    dRow["TubeTypeColorVisible"] = "Visible";
                                }
                                else
                                {
                                    dRow["TubeTypeColorVisible"] = "Hidden";
                                }

                                dRow["PoolingRuleName"] = reader["PoolingRulesName"];
                                dRow["TestingItemName"] = reader["TestName"];

                                if (reader["ItemID"] != DBNull.Value)
                                {
                                    dRow["PCRTestItemID"] = reader["ItemID"];
                                }

                                if (reader["Result"] != DBNull.Value)
                                {
                                    dRow["PCRTestResult"] = reader["Result"];
                                }

                                if (reader["ExperimentID"] != DBNull.Value)
                                {
                                    dRow["ExperimentID"] = reader["ExperimentID"];
                                }

                                if (reader["ExperimentName"] != DBNull.Value)
                                {
                                    dRow["ExperimentName"] = reader["ExperimentName"];
                                }

                                if (reader["StartTime"] != DBNull.Value)
                                {
                                    dRow["StartTime"] = reader["StartTime"];
                                }

                                if (reader["EndTime"] != DBNull.Value)
                                {
                                    dRow["EndTime"] = reader["EndTime"];
                                }

                                if (reader["LoginName"] != DBNull.Value)
                                {
                                    dRow["LoginName"] = reader["LoginName"];
                                }
                                index++;
                                dataTable.Rows.Add(dRow);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->QueryTubesPCRTestResultByBarcode", SessionInfo.ExperimentID);
                throw;
            }
        }

        private bool postProcessExcelFile(ExperimentsInfo expInfo, string fileName, Dictionary<Guid, bool> PCRTestResultDict)
        {
            HSSFWorkbook old_workbook, new_workbook;
            HSSFSheet old_sheet = null, new_sheet;
            string PCRTimeString = "";
            string PCRDeviceString = "";
            string PCRBarCodeString = "";

            // 复制新文件
            ConfigRotationController rotationController = new ConfigRotationController();

            // remove readonly attribute
            // File.SetAttributes(fileName, FileAttributes.Normal);

            // get sheets list from xls
            using (var old_fs = new FileStream(fileName + ".pre.xls", FileMode.Open, FileAccess.Read))
            {
                var new_fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);

                old_workbook = new HSSFWorkbook(old_fs);
                new_workbook = new HSSFWorkbook();

                List<RotationInfo> rotationList = rotationController.GetCurrentRotationInfos(expInfo.ExperimentID);
                foreach (RotationInfo rotationInfo in rotationList)
                {
                    for (int i = 0; i < old_workbook.Count; i++)
                    {
                        if (old_workbook.GetSheetAt(i).SheetName.Contains(rotationInfo.RotationName))
                        {
                            old_sheet = (HSSFSheet)old_workbook.GetSheetAt(i);
                            break;
                        }
                    }

                    if (null == old_sheet)
                        return false;

                    new_sheet = (HSSFSheet)new_workbook.CreateSheet(rotationInfo.RotationName);
                    HSSFRow row = (HSSFRow)new_sheet.CreateRow(0);
                    row.CreateCell(0).SetCellValue("实验名称");
                    row.CreateCell(1).SetCellValue(expInfo.ExperimentName);
                    row.CreateCell(2).SetCellValue("操作员");
                    row.CreateCell(3).SetCellValue(expInfo.LoginName);
                    row = (HSSFRow)new_sheet.CreateRow(1);
                    row.CreateCell(0).SetCellValue("样本数量");
                    row.CreateCell(1).SetCellValue(GetSampleNumber(expInfo.ExperimentID, rotationInfo.RotationID));
                    row.CreateCell(2).SetCellValue("实验时间");
                    row.CreateCell(3).SetCellValue(expInfo.StartTime.ToString("yyyy/MM/dd HH:mm:ss") + "--" + Convert.ToDateTime(expInfo.EndTime).ToString("yyyy/MM/dd HH:mm:ss"));
                    List<Plate> plateList = GetPCRPlateList(rotationInfo.RotationID, expInfo.ExperimentID);
                    if (plateList != null && plateList.Count > 0)
                    {
                        foreach (Plate plate in plateList)
                        {
                            PCRBarCodeString += PCRBarCodeString == "" ? plate.BarCode : ", " + plate.BarCode;
                            XmlDocument xdoc = new XmlDocument();
                            if (null != plate.PCRContent)
                            {
                                xdoc.LoadXml(plate.PCRContent);
                                XmlNode node = xdoc.SelectSingleNode("PCRContent");
                                PCRDeviceString += PCRDeviceString == "" ? node.SelectSingleNode("PCRDevice").InnerText : ", " + node.SelectSingleNode("PCRDevice").InnerText;
                                string timeString = node.SelectSingleNode("PCRStartTime").InnerText + "--" + node.SelectSingleNode("PCREndTime").InnerText;
                                PCRTimeString += PCRTimeString == "" ? timeString : ", " + timeString;
                            }
                        }
                    }
                    row = (HSSFRow)new_sheet.CreateRow(2);
                    row.CreateCell(0).SetCellValue(rotationInfo.RotationName);
                    row.CreateCell(1).SetCellValue("");
                    row.CreateCell(2).SetCellValue("扩增时间");
                    row.CreateCell(3).SetCellValue(PCRTimeString);
                    row = (HSSFRow)new_sheet.CreateRow(3);
                    row.CreateCell(0).SetCellValue("PCR仪");
                    row.CreateCell(1).SetCellValue(PCRDeviceString);
                    row.CreateCell(2).SetCellValue("PCR条码");
                    row.CreateCell(3).SetCellValue(PCRBarCodeString);
                    row = (HSSFRow)new_sheet.CreateRow(4);
                    if (PCRTestResultDict.ContainsKey(rotationInfo.RotationID) && PCRTestResultDict[rotationInfo.RotationID] == false)
                    {
                        row.CreateCell(0).SetCellValue("QC: 质控品结果不符合标准，实验无效");
                    }
                    else
                    {
                        row.CreateCell(0).SetCellValue("");
                    }
                    // create data
                    for (int i = 0; i <= old_sheet.LastRowNum; i++)
                    {
                        row = (HSSFRow)new_sheet.CreateRow(i + 5);
                        for (int j = old_sheet.GetRow(i).FirstCellNum; j < old_sheet.GetRow(i).LastCellNum; j++)
                        {

                            string cellValue = old_sheet.GetRow(i).GetCell(j) != null ? old_sheet.GetRow(i).GetCell(j).ToString() : "";
                            cellValue = cellValue.Replace("HBV_Ct", "HBV(Ct)");
                            cellValue = cellValue.Replace("HBVIC_Ct", "HBVIC(Ct)");
                            cellValue = cellValue.Replace("HCV_Ct", "HCV(Ct)");
                            cellValue = cellValue.Replace("HCVIC_Ct", "HCVIC(Ct)");
                            cellValue = cellValue.Replace("HIV_Ct", "HIV(Ct)");
                            cellValue = cellValue.Replace("HIVIC_Ct", "HIVIC(Ct)");
                            row.CreateCell(j).SetCellValue(cellValue);
                        }
                    }
                }

                old_fs.Close();
                File.Delete(fileName + ".pre.xls");


                new_workbook.Write(new_fs);
                new_fs.Close();

            }
            return true;
        }

        public bool SaveExcelFile(string fileName, Guid experimentId, Guid rotationID, string rotationName, int exportOrder, string startRow, string endRow)
        {
            try
            {

                DataTable _pcrTable = new DataTable();
                _pcrTable.Columns.Add("Number", typeof(int));
                _pcrTable.Columns.Add("Color", typeof(string));
                _pcrTable.Columns.Add("TubeID", typeof(Guid));
                _pcrTable.Columns.Add("TubeBarCode", typeof(string));
                _pcrTable.Columns.Add("TubePosition", typeof(string));
                _pcrTable.Columns.Add("TubePositionNumber", typeof(int));
                _pcrTable.Columns.Add("TubeType", typeof(int));
                _pcrTable.Columns.Add("TubeTypeName", typeof(string));
                _pcrTable.Columns.Add("PCRName", typeof(string));
                _pcrTable.Columns.Add("TubeTypeColor", typeof(string));
                _pcrTable.Columns.Add("TubeTypeColorVisible", typeof(string));
                _pcrTable.Columns.Add("PoolingRuleName", typeof(string));
                _pcrTable.Columns.Add("TestingItemName", typeof(string));
                _pcrTable.Columns.Add("PCRPlateBarCode", typeof(string));
                _pcrTable.Columns.Add("PCRPosition", typeof(string));
                _pcrTable.Columns.Add("HBV", typeof(string));
                _pcrTable.Columns.Add("HBVIC", typeof(string));
                _pcrTable.Columns.Add("HCV", typeof(string));
                _pcrTable.Columns.Add("HCVIC", typeof(string));
                _pcrTable.Columns.Add("HIV", typeof(string));
                _pcrTable.Columns.Add("HIVIC", typeof(string));
                _pcrTable.Columns.Add("PCRTestItemID", typeof(Guid));
                _pcrTable.Columns.Add("PCRTestResult", typeof(string));
                _pcrTable.Columns.Add("PCRTestContent", typeof(string));
                _pcrTable.Columns.Add("SimpleTrackingResult", typeof(string));

                DataTable _sampleTable = _pcrTable.Clone();

                System.Collections.Generic.Dictionary<int, string> liquidTypeDictionary = new System.Collections.Generic.Dictionary<int, string>();
                List<LiquidType> LiquidTypeList = SessionInfo.LiquidTypeList = WanTai.Common.Configuration.GetLiquidTypes();
                foreach (LiquidType liquidType in LiquidTypeList)
                {
                    liquidTypeDictionary.Add(liquidType.TypeId, liquidType.Color);
                }

                string errorMessage = string.Empty;

                ExperimentsInfo expInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId);
                string experimentName = expInfo.ExperimentName;
                //string fileName = folderPath + "\\" + experimentName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".xls";
                if (System.IO.File.Exists(fileName))
                    System.IO.File.Delete(fileName);
                if (System.IO.File.Exists(fileName + ".pre.xls"))
                    System.IO.File.Delete(fileName + ".pre.xls");

                WanTai.Controller.PCR.PCRTestResultViewListController pcrController = new WanTai.Controller.PCR.PCRTestResultViewListController();

                string extension = System.IO.Path.GetExtension(fileName);

                if (extension.Equals(".pdf"))
                {
                    DataTable dt = new DataTable(rotationID + "," + rotationName);

                    _pcrTable.Clear();
                    _sampleTable.Clear();
                    string reagent_batch;
                    string qc_batch;
                    bool has_bci;
                    Dictionary<string, int> resultDict;
                    pcrController.QueryTubesPCRTestResult(experimentId, rotationID, _pcrTable, _sampleTable, liquidTypeDictionary, System.Windows.Media.Colors.Red, System.Windows.Media.Colors.Green, out errorMessage, out reagent_batch, out qc_batch, out has_bci, out resultDict);

                    dt.Columns.Add("序号", typeof(Int32));
                    dt.Columns.Add("类型");
                    dt.Columns.Add("样本名称");
                    dt.Columns.Add("样本条码");
                    dt.Columns.Add("样本位置");
                    dt.Columns.Add("检测方式");
                    dt.Columns.Add("检测项目");
                    // dt.Columns.Add("PCR板条码");
                    dt.Columns.Add("PCR孔位");
                    dt.Columns.Add("HBV(Ct)");
                    if (!has_bci) dt.Columns.Add("HBVIC(Ct)");
                    dt.Columns.Add("HCV(Ct)");
                    if (!has_bci) dt.Columns.Add("HCVIC(Ct)");
                    dt.Columns.Add("HIV(Ct)");
                    if (!has_bci) 
                        dt.Columns.Add("HIVIC(Ct)"); 
                    else 
                        dt.Columns.Add("IC(Ct)");
                    dt.Columns.Add("检测结果");
                    dt.Columns.Add("实验记录");
                    dt.Columns.Add("Color");

                    DataTable sampleTable = dt.Clone();

                    foreach (DataRow row in _pcrTable.Rows)
                    {
                        int tubeType = (int)row["TubeType"];

                        DataRow dr = dt.NewRow();
                        dr["序号"] = (int)row["Number"];
                        dr["类型"] = row["TubeTypeName"].ToString();
                        dr["样本名称"] = row["PCRName"].ToString();
                        dr["样本条码"] = row["TubeBarCode"].ToString();
                        dr["样本位置"] = row["TubePosition"].ToString();
                        dr["检测方式"] = row["PoolingRuleName"].ToString();
                        dr["检测项目"] = row["TestingItemName"].ToString();
                        // dr["PCR板条码"] = row["PCRPlateBarCode"].ToString();
                        dr["PCR孔位"] = row["PCRPosition"].ToString();
                        dr["HBV(Ct)"] = row["HBV"].ToString().Replace("Undetermined", "No Ct");
                        if (!has_bci)
                            dr["HBVIC(Ct)"] = row["HBVIC"].ToString().Replace("Undetermined", "No Ct");
                        dr["HCV(Ct)"] = row["HCV"].ToString().Replace("Undetermined", "No Ct");
                        if (!has_bci)
                            dr["HCVIC(Ct)"] = row["HCVIC"].ToString().Replace("Undetermined", "No Ct");
                        dr["HIV(Ct)"] = row["HIV"].ToString().Replace("Undetermined", "No Ct");
                        if (!has_bci) 
                            dr["HIVIC(Ct)"] = row["HIVIC"].ToString().Replace("Undetermined", "No Ct");
                        else
                            dr["IC(Ct)"] = row["HIVIC"].ToString().Replace("Undetermined", "No Ct");
                        dr["检测结果"] = row["PCRTestResult"].ToString();
                        dr["实验记录"] = row["SimpleTrackingResult"].ToString();
                        dr["Color"] = row["Color"];
                        dt.Rows.Add(dr);
                    }

                    // _sampleTable 排序
                    _sampleTable = _sampleTable.Select("TubeTypeName <> 'NC' AND TubeTypeName <> 'PC' AND TubeTypeName <> 'QC'", "TubePositionNumber asc").CopyToDataTable();;
                    int sampleIndex = 1;
                    foreach (DataRow row in _sampleTable.Rows)
                    {
                        int tubeType = (int)row["TubeType"];

                        DataRow dr = sampleTable.NewRow();
                        dr["序号"] = sampleIndex;
                        dr["类型"] = row["TubeTypeName"].ToString();
                        dr["样本名称"] = row["PCRName"].ToString();
                        dr["样本条码"] = row["TubeBarCode"].ToString();
                        dr["样本位置"] = row["TubePosition"].ToString();
                        dr["检测方式"] = row["PoolingRuleName"].ToString();
                        dr["检测项目"] = row["TestingItemName"].ToString();
                        // dr["PCR板条码"] = row["PCRPlateBarCode"].ToString();
                        dr["PCR孔位"] = row["PCRPosition"].ToString();
                        dr["HBV(Ct)"] = row["HBV"].ToString().Replace("Undetermined", "No Ct");
                        if (!has_bci)
                            dr["HBVIC(Ct)"] = row["HBVIC"].ToString().Replace("Undetermined", "No Ct");
                        dr["HCV(Ct)"] = row["HCV"].ToString().Replace("Undetermined", "No Ct");
                        if (!has_bci)
                            dr["HCVIC(Ct)"] = row["HCVIC"].ToString().Replace("Undetermined", "No Ct");
                        dr["HIV(Ct)"] = row["HIV"].ToString().Replace("Undetermined", "No Ct");
                        if (!has_bci) 
                            dr["HIVIC(Ct)"] = row["HIVIC"].ToString().Replace("Undetermined", "No Ct");
                        else
                            dr["IC(Ct)"] = row["HIVIC"].ToString().Replace("Undetermined", "No Ct");
                        string[] pcrResults = row["PCRTestResult"].ToString().Split('|');
                        if (pcrResults.Length > 3)
                        {
                            dr["检测结果"] = "HBV" + pcrResults[1];
                            dr["检测结果"] += " HCV" + pcrResults[2];
                            dr["检测结果"] += " HIV" + pcrResults[3];
                        }
                        dr["实验记录"] = row["SimpleTrackingResult"].ToString();
                        dr["Color"] = row["Color"];
                        sampleTable.Rows.Add(dr);
                        sampleIndex++;
                    }

                     return ExportToPdf(dt, sampleTable, fileName, expInfo, reagent_batch, qc_batch, has_bci, resultDict, exportOrder, startRow, endRow);
                }
                else if (extension.Equals(".xls"))
                {
                    Dictionary<Guid, bool> PCRTestResultDict = new Dictionary<Guid, bool>();
                    using (OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ".pre.xls;Extended Properties=\"Excel 8.0;HDR=YES;\""))
                    {
                        connection.Open();
                        OleDbCommand command = new OleDbCommand();
                        command.Connection = connection;

                        _pcrTable.Clear();
                        _sampleTable.Clear();
                        string reagent_batch;
                        string qc_batch;
                        bool has_bci;
                        Dictionary<string, int> resultDict;
                        pcrController.QueryTubesPCRTestResult(experimentId, rotationID, _pcrTable, _sampleTable, liquidTypeDictionary, System.Windows.Media.Colors.Red, System.Windows.Media.Colors.Green, out errorMessage, out reagent_batch, out qc_batch, out has_bci, out resultDict);
                        
                        string createTableSql = "";
                        if (!has_bci)
                            createTableSql = "create table [" + rotationName + "] ([序号] Integer,[类型] nvarchar, [样本名称] nvarchar,[样本条码] text,[样本位置] text,"
                                + "[检测方式] nvarchar,[检测项目] nvarchar, [PCR孔位] nvarchar,[HBV_Ct] nvarchar,[HBVIC_Ct] nvarchar,[HCV_Ct] nvarchar,[HCVIC_Ct] nvarchar,[HIV_Ct] nvarchar,[HIVIC_Ct] nvarchar,[检测结果] nvarchar,[实验记录] nvarchar)";
                        else
                            createTableSql = "create table [" + rotationName + "] ([序号] Integer,[类型] nvarchar, [样本名称] nvarchar,[样本条码] text,[样本位置] text,"
                                + "[检测方式] nvarchar,[检测项目] nvarchar, [PCR孔位] nvarchar,[HBV_Ct] nvarchar,[HCV_Ct] nvarchar,[HIV_Ct] nvarchar,[IC_Ct] nvarchar,[检测结果] nvarchar,[实验记录] nvarchar)";
                        command.CommandText = createTableSql;
                        command.ExecuteNonQuery();

                        string insertSql = string.Empty;
                        bool PCRTestOK = true;
                        foreach (DataRow row in _pcrTable.Rows)
                        {
                            if ((row["Number"].ToString() == "1" && row["PCRTestResult"].ToString().Contains("重新测定"))
                                || (row["Number"].ToString() == "2" && row["PCRTestResult"].ToString().Contains("重新测定")))
                            {
                                PCRTestOK = false;
                            }
                            if (!has_bci) 
                                insertSql = string.Format("Insert into [" + rotationName + "] (序号,类型,样本名称,样本条码,样本位置,检测方式,检测项目,PCR孔位,HBV_Ct,HBVIC_Ct,HCV_Ct,HCVIC_Ct,HIV_Ct,HIVIC_Ct,检测结果,实验记录) "
                                    + "values({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}', '{11}','{12}','{13}','{14}','{15}')",
                                        row["Number"].ToString(),
                                        row["TubeTypeName"].ToString(),
                                        row["PCRName"].ToString(),
                                        row["TubeBarCode"].ToString(),
                                        row["TubePosition"].ToString(),
                                        row["PoolingRuleName"].ToString(),
                                        row["TestingItemName"].ToString(),
                                        row["PCRPosition"].ToString(),
                                        row["HBV"].ToString(),
                                        row["HBVIC"].ToString(),
                                        row["HCV"].ToString(),
                                        row["HCVIC"].ToString(),
                                        row["HIV"].ToString(),
                                        row["HIVIC"].ToString(),
                                        row["PCRTestResult"].ToString(),
                                        row["SimpleTrackingResult"].ToString());
                            else
                                insertSql = string.Format("Insert into [" + rotationName + "] (序号,类型,样本名称,样本条码,样本位置,检测方式,检测项目,PCR孔位,HBV_Ct,HCV_Ct,HIV_Ct,IC_Ct,检测结果,实验记录) "
                                    + "values({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}', '{11}','{12}','{13}')",
                                        row["Number"].ToString(),
                                        row["TubeTypeName"].ToString(),
                                        row["PCRName"].ToString(),
                                        row["TubeBarCode"].ToString(),
                                        row["TubePosition"].ToString(),
                                        row["PoolingRuleName"].ToString(),
                                        row["TestingItemName"].ToString(),
                                        row["PCRPosition"].ToString(),
                                        row["HBV"].ToString(),
                                        row["HCV"].ToString(),
                                        row["HIV"].ToString(),
                                        row["HIVIC"].ToString(),
                                        row["PCRTestResult"].ToString(),
                                        row["SimpleTrackingResult"].ToString());
                            command.CommandText = insertSql;
                            command.ExecuteNonQuery();
                        }
                        PCRTestResultDict.Add(rotationID, PCRTestOK);

                        connection.Close();
                    }
                    return postProcessExcelFile(expInfo, fileName, PCRTestResultDict);
                }

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        #region ExportToPdf()

        /// <summary>
        /// Exporttopdf
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool ExportToPdf(DataTable dt, DataTable sampleTable, string fileName, ExperimentsInfo expInfo, string reagent_batch, string qc_batch, bool has_bci, Dictionary<string, int> resultDict, int exportOrder, string startRow, string endRow)
        {
            ///设置导出字体
            string FontPath = "C://WINDOWS//Fonts//msyh.ttf"; //"C://WINDOWS//Fonts//simsun.ttc,1";
            int FontSize = 8;

            Boolean result = true;
            //竖排模式,大小为A4，四周边距均为25
            Document document = new Document(PageSize.A4, 25, 25, 20, 20);

            //调用PDF的写入方法流
            //注意FileMode-Create表示如果目标文件不存在，则创建，如果已存在，则覆盖。
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));

            //创建PDF文档中的字体
            BaseFont baseFont = BaseFont.CreateFont(
              FontPath,
              BaseFont.IDENTITY_H,
              BaseFont.NOT_EMBEDDED);

            //根据字体路径和字体大小属性创建字体
            iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, FontSize);
            iTextSharp.text.Font fontWhite = new iTextSharp.text.Font(baseFont, FontSize);
            fontWhite.Color = iTextSharp.text.Color.WHITE;
            iTextSharp.text.Font fontRed = new iTextSharp.text.Font(baseFont, FontSize);
            fontRed.Color = iTextSharp.text.Color.RED;

            iTextSharp.text.Font fontLabel = new iTextSharp.text.Font(baseFont, 9, Font.BOLD);

            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(baseFont, 8);

            iTextSharp.text.Font fontFoot = new iTextSharp.text.Font(baseFont, 7, Font.BOLD);
            fontFoot.Color = iTextSharp.text.Color.GRAY;

            iTextSharp.text.Font fontSmall = new iTextSharp.text.Font(baseFont, 4);

            iTextSharp.text.Font fontWanTag = new iTextSharp.text.Font(baseFont, 12, Font.BOLD);
            iTextSharp.text.Font fontWanTagRed = new iTextSharp.text.Font(baseFont, 12, Font.BOLD);
            fontWanTagRed.Color = iTextSharp.text.Color.RED;

            iTextSharp.text.Font fontTableHead = new iTextSharp.text.Font(baseFont, 6, Font.BOLD);
            iTextSharp.text.Font fontTableContent = new iTextSharp.text.Font(baseFont, 6);
            iTextSharp.text.Font fontTableRed = new iTextSharp.text.Font(baseFont, 6);
            fontTableRed.Color = iTextSharp.text.Color.RED;
            iTextSharp.text.Font fontTableWhite = new iTextSharp.text.Font(baseFont, 6);
            fontTableWhite.Color = iTextSharp.text.Color.WHITE;

            iTextSharp.text.Font fontTableContentBigger = new iTextSharp.text.Font(baseFont, 7.5F);
            iTextSharp.text.Font fontTableRedBigger = new iTextSharp.text.Font(baseFont, 7.5F);
            fontTableRedBigger.Color = iTextSharp.text.Color.RED;
            iTextSharp.text.Font fontTableWhiteBigger = new iTextSharp.text.Font(baseFont, 7.5F);
            fontTableWhiteBigger.Color = iTextSharp.text.Color.WHITE;

            DateTime dTime = DateTime.Now;

            // iTextSharp.text.HeaderFooter footer = new iTextSharp.text.HeaderFooter(new Phrase("导出时间：" + dTime.ToString("yyyy/MM/dd HH:mm:ss") + "    页数: "), true);
            iTextSharp.text.HeaderFooter footer = new iTextSharp.text.HeaderFooter(new Phrase("实验   " + expInfo.ExperimentName + " " + dTime.ToString("yyyy/MM/dd HH:mm:ss") + "   ", fontFoot), true);
            footer.Border = Rectangle.NO_BORDER;
            footer.Alignment = Element.ALIGN_RIGHT;
            document.Footer = footer;

            string PCRTimeString = "";
            string PCRDeviceString = "";
            string PCRBarCodeString = "";
            string[] rotationArr = dt.TableName.Split(new char[] { ',' });
            string rotationID = rotationArr[0];
            string rotationName = rotationArr[1];

            List<Plate> plateList = GetPCRPlateList(new Guid(rotationID), expInfo.ExperimentID);
            if (plateList != null && plateList.Count > 0)
            {
                foreach (Plate plate in plateList)
                {
                    PCRBarCodeString += PCRBarCodeString == "" ? plate.BarCode : " " + plate.BarCode;
                    XmlDocument xdoc = new XmlDocument();
                    if (null != plate.PCRContent)
                    {
                        xdoc.LoadXml(plate.PCRContent);
                        XmlNode node = xdoc.SelectSingleNode("PCRContent");
                        PCRDeviceString += PCRDeviceString == "" ? node.SelectSingleNode("PCRDevice").InnerText : " " + node.SelectSingleNode("PCRDevice").InnerText;
 
                        DateTime pcrStartTime, pcrEndTime;
                        string pcrStartTimeString = node.SelectSingleNode("PCRStartTime").InnerText;
                        string pcrEndTimeString = node.SelectSingleNode("PCREndTime").InnerText;
                        int spaceIndex = pcrStartTimeString.IndexOf(' ', pcrStartTimeString.IndexOf(' ') + 1);
                        if (spaceIndex > 0)
                        {
                            pcrStartTimeString = pcrStartTimeString.Substring(0, spaceIndex);
                        }
                        spaceIndex = pcrEndTimeString.IndexOf(' ', pcrEndTimeString.IndexOf(' ') + 1);
                        if (spaceIndex > 0)
                        {
                            pcrEndTimeString = pcrEndTimeString.Substring(0, spaceIndex);
                        }
                        if (DateTime.TryParse(pcrStartTimeString, out pcrStartTime))
                        {
                            pcrStartTimeString = pcrStartTime.ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        if (DateTime.TryParse(pcrEndTimeString, out pcrEndTime))
                        {
                            pcrEndTimeString = pcrEndTime.ToString("yyyy/MM/dd HH:mm:ss");
                        }
                        string timeString = pcrStartTimeString + "--" + pcrEndTimeString;
                        PCRTimeString += PCRTimeString == "" ? timeString : "\n " + timeString;
                    }
                }
            }

            //打开目标文档对象
            document.Open();

            // document.Add(new Paragraph("\n"));
            Chunk redChunk = new Chunk("WanTag ", fontWanTagRed);
            Chunk blackTrunk = new Chunk("检测报告", fontWanTag);
            Paragraph p_WanTag = new Paragraph();
            p_WanTag.Alignment = Element.ALIGN_RIGHT;
            p_WanTag.Add(redChunk);
            p_WanTag.Add(blackTrunk);
            document.Add(p_WanTag);

            document.Add(new Paragraph("\n"));

            PdfPTable header_table = new PdfPTable(6);
            header_table.WidthPercentage = 100f;
            header_table.SetTotalWidth(new float[] { 9, 18, 9, 36, 9, 18 });

            // 实验名称
            PdfPCell cell = new PdfPCell(new Phrase("实验名称：", fontLabel));
            // cell.UseAscender = true;
            cell.BorderWidth = 0; 
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(expInfo.ExperimentName, font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 提取时间
            cell = new PdfPCell(new Phrase("提取时间：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(expInfo.StartTime.ToString("yyyy/MM/dd HH:mm:ss") + "--" + Convert.ToDateTime(expInfo.EndTime).ToString("yyyy/MM/dd HH:mm:ss"), font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 仪器型号
            cell = new PdfPCell(new Phrase("仪器型号：", fontLabel));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(SessionInfo.GetSystemConfiguration("InstrumentType"), font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0; 
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

           // 操作员
            cell = new PdfPCell(new Phrase("操作员：", fontLabel));
            // cell.UseAscender = true;
            cell.BorderWidth = 0; 
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(expInfo.LoginName, font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0; 
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 扩增时间：
            cell = new PdfPCell(new Phrase("扩增时间：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(PCRTimeString, font));
            cell.BorderWidth = 0;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 仪器编码
            cell = new PdfPCell(new Phrase("仪器编码：", fontLabel));
            // cell.UseAscender = true;
            cell.BorderWidth = 0; 
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell((new Phrase(SessionInfo.GetSystemConfiguration("InstrumentType"), font)));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 轮次
            cell = new PdfPCell(new Phrase(rotationName, fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase("", font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // "PCR仪：
            cell = new PdfPCell(new Phrase("PCR仪：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(PCRDeviceString, font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);


            // 试剂批号
            cell = new PdfPCell(new Phrase("试剂批号：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(reagent_batch, font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 样本数量：
            cell = new PdfPCell(new Phrase("样本数量：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(GetSampleNumber(expInfo.ExperimentID, new Guid(rotationID)).ToString(), font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // PCR板条码：
            cell = new PdfPCell(new Phrase("PCR板条码：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(PCRBarCodeString, font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 质控品批号
            cell = new PdfPCell(new Phrase("质控品批号：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(qc_batch, font));
            cell.BorderWidth = 0;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 检验人
            cell = new PdfPCell(new Phrase("", font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.Colspan = 5;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase("检验人：_________________", font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            // 复核人
            cell = new PdfPCell(new Phrase("", font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.Colspan = 5;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            cell = new PdfPCell(new Phrase("复核人：_________________", font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0; 
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);

            /*
            // empty
            cell = new PdfPCell(new Phrase("", font));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 5;
            cell.Colspan = 6;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            header_table.AddCell(cell);
             */

            document.Add(header_table);

            //Paragraph pLine = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.Color.BLACK, Element.ALIGN_LEFT, 1)));
            // call the below to add the line when required.
            //document.Add(pLine);


 
            if (dt.Rows[0]["检测结果"].ToString().Contains("重新测定") || dt.Rows[1]["检测结果"].ToString().Contains("重新测定"))
            {
                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.Color.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);
                Paragraph p_QC = new Paragraph("QC: 质控品结果不符合标准，实验无效", fontLabel);
                document.Add(p_QC);
            }
            else
            {
                Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.Color.BLACK, Element.ALIGN_LEFT, 1)));
                document.Add(p);
                Paragraph p_QC = new Paragraph("QC: 质控品结果符合标准，实验有效", fontLabel);
                document.Add(p_QC);
            }

            document.Add(new Paragraph("\n", fontSmall));

            PdfPTable second_table = new PdfPTable(6);
            second_table.WidthPercentage = 100f;
            second_table.SetTotalWidth(new float[] { 16, 16, 16, 16, 16, 16 });

            // 样本份数
            cell = new PdfPCell(new Phrase("共检测样本 " + GetSampleNumber(expInfo.ExperimentID, new Guid(rotationID)).ToString() + " 份", fontLabel));
            // cell.UseAscender = true;
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.Colspan = 6;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            if (!string.IsNullOrEmpty(startRow) && !string.IsNullOrEmpty(endRow))
            {
                // 样本份数
                cell = new PdfPCell(new Phrase("打印结果样本 从 " + startRow.ToString() + " 至 " + endRow.ToString(), fontLabel));
                // cell.UseAscender = true;
                cell.BorderWidth = 0;
                cell.FixedHeight = 15;
                cell.Colspan = 6;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                second_table.AddCell(cell);
            }

            //阳性Pool数：
            cell = new PdfPCell(new Phrase("阳性Pool数：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(resultDict["positivePoolNumber"].ToString(), font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.Colspan = 5;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            //阴性Pool数：
            cell = new PdfPCell(new Phrase("阴性Pool数：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(resultDict["negativePoolNumber"].ToString(), font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            //阴性样本数：
            cell = new PdfPCell(new Phrase("阴性样本数：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(resultDict["negativeSampleNumber"].ToString(), font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.Colspan = 3;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            //无效Pool数：
            cell = new PdfPCell(new Phrase("无效Pool数：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(resultDict["invalidPoolNumber"].ToString(), font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            //无效样本数：
            cell = new PdfPCell(new Phrase("无效样本数：", fontLabel));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            cell = new PdfPCell(new Phrase(resultDict["invalidSampleNumber"].ToString(), font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.Colspan = 3;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            // empty
            cell = new PdfPCell(new Phrase("", font));
            cell.BorderWidth = 0;
            cell.FixedHeight = 15;
            cell.Colspan = 6;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            second_table.AddCell(cell);

            document.Add(second_table);

            // 阳性数据
            DataTable dtPositive = dt.Select("类型 <> 'NC' AND 类型 <> 'PC' AND 类型 <> 'QC' AND 检测结果 like '%阳性%'", "").CopyToDataTable();

            // 阳性样本数据
            Paragraph pLabel = new Paragraph("阳性样本信息：", fontLabel);
            document.Add(pLabel);

            document.Add(new Paragraph("\n", fontSmall));


            //根据数据表内容创建一个PDF格式的表
            PdfPTable table = new PdfPTable(dtPositive.Columns.Count - 1);
            table.WidthPercentage = 100f;
            string PCRTestResultWidths = WanTai.Common.Configuration.GetPCRTestResultWidthList();
            if (!string.IsNullOrEmpty(PCRTestResultWidths))
            {
                if (!has_bci)
                    table.SetTotalWidth(Array.ConvertAll(PCRTestResultWidths.Split(','), new Converter<string, float>(float.Parse)));    
                else
                    table.SetTotalWidth(new float[] { 5, 8, 0, 20, 15, 8, 8, 16, 9, 9, 9, 9, 14, 10 });
            }
            else
            {
                if (!has_bci)
                    table.SetTotalWidth(new float[] { 4, 8, 8, 20, 15, 8, 8, 16, 9, 9, 9, 9, 9, 9, 14, 10 });
                else
                    table.SetTotalWidth(new float[] { 4, 8, 8, 20, 15, 8, 8, 16, 9, 9, 9, 9, 14, 10 });
            }
            // 添加表头，每一页都有表头
            for (int j = 0; j < dtPositive.Columns.Count - 1; j++)
            {
                string cellName = dtPositive.Columns[j].ColumnName;
                cellName = cellName.Replace("BarCode", "条码").Replace("Position", "孔位");
                cell = new PdfPCell(new Phrase(cellName, fontTableHead));

                // cell.UseAscender = true;
                cell.FixedHeight = 20;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = new Color(220, 220, 220);

                table.AddCell(cell);
            }

            // 告诉程序这行是表头，这样页数大于1时程序会自动为你加上表头。
            table.HeaderRows = 1;

            //遍历原datatable的数据行
            for (int i = 0; i < dtPositive.Rows.Count; i++)
            {
                for (int j = 0; j < dtPositive.Columns.Count - 1; j++)
                {
                    try
                    {
                        cell = new PdfPCell(new Phrase(dtPositive.Rows[i][j].ToString(), fontTableContentBigger));
                        System.Drawing.Color Color = System.Drawing.ColorTranslator.FromHtml(dtPositive.Rows[i]["Color"].ToString());
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = new iTextSharp.text.Color(Color);
                        
                        if (dtPositive.Rows[i]["Color"].ToString() == WantagColor.WantagRed)
                        {
                            cell.Phrase = new Phrase(dtPositive.Rows[i][j].ToString(), fontTableRedBigger);
                            cell.BackgroundColor = new iTextSharp.text.Color(System.Drawing.ColorTranslator.FromHtml(WantagColor.WantagWhite));
                        }                     

                        table.AddCell(cell);
                    }
                    catch (Exception e)
                    {
                        result = false;
                    }
                }
            }

            //在目标文档中添加转化后的表数据
            document.Add(table);

            // Make the page break
            document.NewPage();

            // NC、PC、QC数据
            DataTable dtNCPCQC = dt.Select("类型 = 'NC' OR 类型 = 'PC' OR 类型 = 'QC'", "").CopyToDataTable();

            // 阳性样本数据
            pLabel = new Paragraph("附件：实验数据", fontLabel);
            document.Add(pLabel);

            document.Add(new Paragraph("\n", fontSmall));


            //根据数据表内容创建一个PDF格式的表
            table = new PdfPTable(dtNCPCQC.Columns.Count - 1);
            table.WidthPercentage = 100f;
            PCRTestResultWidths = WanTai.Common.Configuration.GetPCRTestResultWidthList();
            if (!string.IsNullOrEmpty(PCRTestResultWidths))
            {
                if (!has_bci)
                    table.SetTotalWidth(Array.ConvertAll(PCRTestResultWidths.Split(','), new Converter<string, float>(float.Parse)));
                else
                    table.SetTotalWidth(new float[] { 5, 8, 0, 20, 15, 8, 8, 16, 9, 9, 9, 9, 14, 10 });
            }
            else
            {
                if (!has_bci)
                    table.SetTotalWidth(new float[] { 4, 8, 8, 20, 15, 8, 8, 16, 9, 9, 9, 9, 9, 9, 14, 10 });
                else
                    table.SetTotalWidth(new float[] { 4, 8, 8, 20, 15, 8, 8, 16, 9, 9, 9, 9, 14, 10 });
            }
            // 添加表头，每一页都有表头
            for (int j = 0; j < dtNCPCQC.Columns.Count - 1; j++)
            {
                string cellName = dtNCPCQC.Columns[j].ColumnName;
                cellName = cellName.Replace("BarCode", "条码").Replace("Position", "孔位");
                cell = new PdfPCell(new Phrase(cellName, fontTableHead));

                // cell.UseAscender = true;
                cell.FixedHeight = 20;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = new Color(220, 220, 220);

                table.AddCell(cell);
            }

            // 告诉程序这行是表头，这样页数大于1时程序会自动为你加上表头。
            table.HeaderRows = 1;

            //遍历原datatable的数据行
            for (int i = 0; i < dtNCPCQC.Rows.Count; i++)
            {
                for (int j = 0; j < dtNCPCQC.Columns.Count - 1; j++)
                {
                    try
                    {
                        cell = new PdfPCell(new Phrase(dtNCPCQC.Rows[i][j].ToString(), fontTableContentBigger));
                        System.Drawing.Color Color = System.Drawing.ColorTranslator.FromHtml(dtNCPCQC.Rows[i]["Color"].ToString());
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = new iTextSharp.text.Color(Color);
                        if (dtNCPCQC.Rows[i]["Color"].ToString() == WantagColor.WantagRed && !dtNCPCQC.Rows[i]["类型"].ToString().Contains("PC"))
                        {
                            cell.Phrase = new Phrase(dtNCPCQC.Rows[i][j].ToString(), fontTableRedBigger);
                            cell.BackgroundColor = new iTextSharp.text.Color(System.Drawing.ColorTranslator.FromHtml(WantagColor.WantagWhite));
                        }
                        if (dtNCPCQC.Rows[i]["类型"].ToString().Contains("PC") && dtNCPCQC.Rows[i]["Color"].ToString() == WantagColor.WantagRed)
                        {
                            cell.Phrase = new Phrase(dtNCPCQC.Rows[i][j].ToString(), fontTableWhiteBigger);
                        }

                        table.AddCell(cell);
                    }
                    catch (Exception e)
                    {
                        result = false;
                    }
                }
            }

            //在目标文档中添加转化后的表数据
            document.Add(table);

            // 样本数据信息
            // Make the page break
            document.NewPage();
            string sampleSelectStr = "类型 <> 'NC' AND 类型 <> 'PC' AND 类型 <> 'QC'";
            if (!string.IsNullOrEmpty(startRow) && !string.IsNullOrEmpty(endRow))
            {
                sampleSelectStr += " AND 序号 >= " + startRow + " AND 序号 <= " + endRow ;
            }
            DataTable dtSample;
            float paddingTop = 1;
            float paddingBottom = 1;
            if (exportOrder == 0)
            {
                dtSample = dt.Select(sampleSelectStr, "").CopyToDataTable();
                paddingTop = 2f;
                paddingBottom = 2f;
                fontTableContent = fontTableContentBigger;
                fontTableRed = fontTableRedBigger;
                fontTableWhite = fontTableWhiteBigger;
            }
            else
            {
                dtSample = sampleTable.Select(sampleSelectStr, "").CopyToDataTable();
                paddingTop = 0.8f;
                paddingBottom = 0.7f;
            }


            //根据数据表内容创建一个PDF格式的表
            table = new PdfPTable(dtSample.Columns.Count - 1);
            table.WidthPercentage = 100f;
            PCRTestResultWidths = WanTai.Common.Configuration.GetPCRTestResultWidthList();
            if (!string.IsNullOrEmpty(PCRTestResultWidths))
            {
                if (!has_bci)
                    table.SetTotalWidth(Array.ConvertAll(PCRTestResultWidths.Split(','), new Converter<string, float>(float.Parse)));
                else
                    table.SetTotalWidth(new float[] { 5, 8, 0, 20, 15, 8, 8, 16, 9, 9, 9, 9, 22, 10 });
            }
            else
            {
                if (!has_bci)
                    table.SetTotalWidth(new float[] { 4, 8, 8, 20, 15, 8, 8, 16, 9, 9, 9, 9, 9, 9, 14, 10 });
                else
                    table.SetTotalWidth(new float[] { 4, 8, 8, 20, 15, 8, 8, 16, 9, 9, 9, 9, 14, 10 });
            }
            // 添加表头，每一页都有表头
            for (int j = 0; j < dtSample.Columns.Count - 1; j++)
            {
                string cellName = dtSample.Columns[j].ColumnName;
                cellName = cellName.Replace("BarCode", "条码").Replace("Position", "孔位");
                cell = new PdfPCell(new Phrase(cellName, fontTableHead));

                // cell.UseAscender = true;
                cell.FixedHeight = 20;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = new Color(220, 220, 220);

                table.AddCell(cell);
            }

            // 告诉程序这行是表头，这样页数大于1时程序会自动为你加上表头。
            table.HeaderRows = 1;

            //遍历原datatable的数据行
            for (int i = 0; i < dtSample.Rows.Count; i++)
            {
                for (int j = 0; j < dtSample.Columns.Count - 1; j++)
                {
                    try
                    {
                        cell = new PdfPCell(new Phrase(dtSample.Rows[i][j].ToString(), fontTableContent));
                        cell.MinimumHeight = 5f;
                        cell.PaddingTop = paddingTop;
                        cell.PaddingBottom = paddingBottom;
                        System.Drawing.Color Color = System.Drawing.ColorTranslator.FromHtml(dtSample.Rows[i]["Color"].ToString());
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = new iTextSharp.text.Color(Color);
                        if (dtSample.Rows[i]["Color"].ToString() == WantagColor.WantagRed)
                        {
                            cell.Phrase = new Phrase(dtSample.Rows[i][j].ToString(), fontTableRed);
                            cell.BackgroundColor = new iTextSharp.text.Color(System.Drawing.ColorTranslator.FromHtml(WantagColor.WantagWhite));
                        }
                        if (dtSample.Columns[j].ColumnName == "类型")
                        {
                            if (dtSample.Rows[i][j].ToString().Contains("PC"))
                            {
                                cell.BackgroundColor = new iTextSharp.text.Color(System.Drawing.ColorTranslator.FromHtml(WantagColor.WantagRed));
                                cell.Phrase = new Phrase(dtSample.Rows[i][j].ToString(), fontTableWhite);
                            }
                        }

                        table.AddCell(cell);
                    }
                    catch (Exception e)
                    {
                        result = false;
                    }
                }
            }

            //在目标文档中添加转化后的表数据
            document.Add(table);

            //关闭目标文件
            document.Close();

            //关闭写入流
            writer.Close();
            return result;
        }


        #endregion
    }
}
