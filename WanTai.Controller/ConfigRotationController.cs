using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;

using WanTai.DataModel;
using System.Data.Objects;

namespace WanTai.Controller
{
    public class ConfigRotationController
    {
        public List<OperationConfiguration> GetDisplayedOperationConfigurations()
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.OperationConfigurations.Where(c => c.DisplayFlag == true && c.ActiveStatus == true).OrderByDescending(c => c.OperationType).ThenBy(c => c.OperationSequence);
                    List<OperationConfiguration> results = records.ToList<OperationConfiguration>();
                    
                    return results;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "GetDisplayedOperationConfigurations()", SessionInfo.ExperimentID);
                throw;
            }
        }

        public TubesBatch GetLastTubesBatch()
        {
            try
            {
                Guid experimentId = SessionInfo.ExperimentID;
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    TubesBatch record = entities.TubesBatches.Where(c => c.ExperimentID == experimentId).OrderByDescending(c => c.CreateTime).FirstOrDefault();
                    return record;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "GetLastTubesBatch()", SessionInfo.ExperimentID);
                throw;
            }
        }

        public TubesBatch GetTubesBatchByID(Guid batchID)
        {
            try
            {                
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    TubesBatch record = entities.TubesBatches.Where(c => c.TubesBatchID == batchID).FirstOrDefault();
                    return record;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "GetTubesBatchByID()", SessionInfo.ExperimentID);
                throw;
            }
        }        

        public bool Create(List<RotationInfo> rotationInfoList)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var oldRotations = entities.RotationInfoes.Where(c => c.ExperimentID == SessionInfo.ExperimentID);
                    
                    if (oldRotations.Count() > 0)
                    {                        
                        foreach (RotationInfo old in oldRotations)
                        { 
                            entities.DeleteObject(old);
                        }
                    }

                    foreach (RotationInfo rotationInfo in rotationInfoList)
                    {
                        entities.AddToRotationInfoes(rotationInfo);
                        if (rotationInfo.TubesBatchID != null)
                        {
                            var plates = entities.Plates.Where(P => P.TubesBatchID == rotationInfo.TubesBatchID);
                            foreach (Plate plate in plates)
                            {
                                plate.RotationID = rotationInfo.RotationID;
                            }
                        }
                    }

                    entities.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "Create()", SessionInfo.ExperimentID);
                return false;
            }
        }

        public List<RotationInfo> GetCurrentRotationInfos(Guid experimentID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var rotations = entities.RotationInfoes.Where(c => c.ExperimentID == experimentID).OrderBy(c=> c.RotationSequence);

                    return rotations.ToList<RotationInfo>();
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "GetCurrentRotationInfos()", SessionInfo.ExperimentID);
                throw;
            }
        }

        public RotationInfo GetRotationInfo(Guid rotationID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    RotationInfo rotation = entities.RotationInfoes.FirstOrDefault(c => c.RotationID == rotationID);

                    return rotation;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "GetRotationInfo(Guid rotationID)", SessionInfo.ExperimentID);
                throw;
            }
        }


        public void GetRotationOperates(Guid rotationID, DataTable dataTable, System.Windows.Media.Color alternativeColor, System.Windows.Media.Color defaultColor)
        {            
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "SELECT RotationOperate.*, OperationConfiguration.OperationName FROM RotationOperate left join OperationConfiguration"
                    + " on RotationOperate.OperationID = OperationConfiguration.OperationID where RotationID=@RotationID";                

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@RotationID", rotationID);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            int index = 1;
                            while (reader.Read())
                            {
                                System.Data.DataRow dRow = dataTable.NewRow();
                                dRow["Number"] = index;
                                if (index % 2 == 0)
                                {
                                    dRow["Color"] = alternativeColor;
                                }
                                else
                                {
                                    dRow["Color"] = defaultColor;
                                }
                                dRow["OperationID"] = reader["OperationID"];
                                dRow["OperationName"] = reader["OperationName"];
                                dRow["StartTime"] = reader["StartTime"].ToString();
                                if (reader["EndTime"] != DBNull.Value)
                                {
                                    dRow["EndTime"] = reader["EndTime"].ToString();
                                }

                                dRow["State"] = ConvertEnumStatusToText((RotationOperateStatus)((short)reader["State"]));
                                dRow["ErrorLog"] = reader["ErrorLog"];

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
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetRotationOperates", SessionInfo.ExperimentID);
                throw;
            }
        }

        public string ConvertEnumStatusToText(RotationInfoStatus status)
        {
            if (status == RotationInfoStatus.Create)
                return "新建";
            else if (status == RotationInfoStatus.Fail)
                return "失败";
            else if (status == RotationInfoStatus.Finish)
                return "完成";
            else if (status == RotationInfoStatus.Processing)
                return "运行";
            else if (status == RotationInfoStatus.Stop)
                return "停止";
            else if (status == RotationInfoStatus.Suspend)
                return "暂停";
            else return null;
        }

        public string ConvertEnumStatusToText(RotationOperateStatus status)
        {
            if (status == RotationOperateStatus.Create)
                return "新建";
            else if (status == RotationOperateStatus.Fail)
                return "失败";
            else if (status == RotationOperateStatus.Finish)
                return "完成";
            else if (status == RotationOperateStatus.Processing)
                return "运行";
            else if (status == RotationOperateStatus.Stop)
                return "停止";
            else if (status == RotationOperateStatus.Suspend)
                return "暂停";
            else return null;
        }
    }
}
