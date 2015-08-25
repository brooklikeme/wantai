using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.Xml;

using WanTai.DataModel;
using WanTai.Common;

namespace WanTai.Controller.PCR
{
    public class ImportPCRTestResultFileController
    {
        public List<RotationInfo> GetFinishedRotation(Guid experimentId)
        {
            List<RotationInfo> recordList = new List<RotationInfo>();
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "SELECT distinct RotationInfo.RotationID, RotationInfo.RotationName"
                    + " FROM Plates LEFT JOIN RotationInfo on Plates.RotationID = RotationInfo.RotationID"
                    + " WHERE Plates.PlateType=@PlateType and Plates.ExperimentID=@ExperimentID and RotationInfo.State=@RotationState";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open(); 

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@PlateType", PlateType.PCR_Plate);
                        cmd.Parameters.AddWithValue("@ExperimentID", experimentId);
                        cmd.Parameters.AddWithValue("@RotationState", RotationInfoStatus.Finish);

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
            catch(Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetFinishedRotation", experimentId);
                throw;
            }

            return recordList;
        }

        public List<Plate> GetPCRPlateBarcode(Guid rotationId, Guid experimentId)
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
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPCRPlateBarcode", experimentId);
                throw;
            }

            return null;
        }

        public int ChangeCharacterToPositionNumber(string characterNumber)
        {
            characterNumber = characterNumber.Trim();
            int oneLineCellCount = 8;
            int number = 0;
            string character = characterNumber.Substring(0, 1);
            int lineNumber = int.Parse(characterNumber.Substring(1));
            switch (character)
            {
                case "A":
                    number = 1;
                    break;
                case "B":
                    number = 2;
                    break;
                case "C":
                    number = 3;
                    break;
                case "D":
                    number = 4;
                    break;
                case "E":
                    number = 5;
                    break;
                case "F":
                    number = 6;
                    break;
                case "G":
                    number = 7;
                    break;
                case "H":
                    number = 8;
                    break;                     
                default:
                    break;
            }

            return (lineNumber - 1) * oneLineCellCount + number;
        }

        public bool Create(List<PCRTestResult> recordList)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    foreach (PCRTestResult record in recordList)
                    {
                        entities.AddToPCRTestResults(record);
                    }

                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->Create", SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool IsPlateHasImportedResult(Guid rotationId, Guid plateId, Guid experimentId)
        {
            bool result = false;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    int count = entities.PCRTestResults.Where(p => p.RotationID == rotationId && p.PlateID == plateId).Count();
                    result = count > 0 ? true : false;                    
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->IsPlateHasImportedResult", experimentId);
                throw;
            }

            return result;
        }

        public Dictionary<int, DataRow> GetPCRPositionsByPlateID(Guid PCRPlateID, Guid experimentId)
        {
            DataTable dataTable = new DataTable();
            Dictionary<int, DataRow> result = new Dictionary<int, DataRow>();
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "SELECT TubeNumber, TestName, TubeType, PCRPosition FROM View_Tubes_PCRPlatePosition"
                    + " WHERE PCRPlateID=@PCRPlateID order by PCRPosition";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@PCRPlateID", PCRPlateID);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            dataTable.Load(reader);  
                        }
                    }
                }

                foreach (DataRow row in dataTable.Rows)
                {
                    if (!result.ContainsKey((int)row["PCRPosition"]))
                    {
                        result.Add((int)row["PCRPosition"], row);
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPCRPositionsByPlateID", experimentId);
                throw;
            }

            return result;
        }

        public bool SetPCRPlateExtContent(Guid PCRPlateID, Guid experimentId, string PCRStartTime, string PCREndTime, string PCRDevice){
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var plate = entities.Plates.Where(c => c.PlateID == PCRPlateID).FirstOrDefault();
                    if (null != plate)
                    {
                        plate.PCRContent = "<PCRContent><PCRStartTime>" + PCRStartTime + "</PCRStartTime><PCREndTime>" 
                            + PCREndTime + "</PCREndTime><PCRDevice>" + PCRDevice + "</PCRDevice></PCRContent>";
                        entities.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->SetPCRPlateExtContent", experimentId);
                throw;
            }
            return true;
        }
    }
}
