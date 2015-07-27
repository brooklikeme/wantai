using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using WanTai.DataModel;

namespace WanTai.Controller.Configuration
{
    public class OperationConfigurationController
    {
        public bool Create(OperationConfiguration configuration)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToOperationConfigurations(configuration);
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

        public List<OperationConfiguration> GetAllSingleOperation()
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.OperationConfigurations.Where(c => c.OperationType == (short)OperationType.Single).OrderBy(c => c.OperationSequence);
                    return records.ToList<OperationConfiguration>();
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetAllSingleOperation", SessionInfo.ExperimentID);
                throw;
            }
        }

        public List<OperationConfiguration> GetAllOperations()
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.OperationConfigurations.OrderBy(c => c.OperationSequence);
                    return records.ToList<OperationConfiguration>();
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetAllOperations", SessionInfo.ExperimentID);
                throw;
            }
        }

        public string GetOperationName(Guid operationID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    OperationConfiguration record = entities.OperationConfigurations.Where(c => c.OperationID == operationID).FirstOrDefault();
                    if (record != null)
                    {
                        return record.OperationName;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetOperationName", SessionInfo.ExperimentID);
                throw;
            }
        }

        public bool IsOperationCanBeDeleted(string operationID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.OperationConfigurations.Where(c => c.SubOperationIDs.Contains(operationID));
                    if (records.Count() ==0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->IsOperationCanBeDeleted", SessionInfo.ExperimentID);
                throw;
            }
        }

        public bool Delete(Guid operationID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var record = entities.OperationConfigurations.Where(c => c.OperationID == operationID).FirstOrDefault();
                    entities.DeleteObject(record);

                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->Delete", SessionInfo.ExperimentID);
                return false;
            }
        }

        public OperationConfiguration GetOperation(Guid operationID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    OperationConfiguration record = entities.OperationConfigurations.Where(c => c.OperationID == operationID).FirstOrDefault();
                    return record;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetOperation", SessionInfo.ExperimentID);
                throw;
            }
        }

        public bool EditOperation(Guid operationID, OperationConfiguration item)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    OperationConfiguration record = entities.OperationConfigurations.Where(p => p.OperationID == operationID).FirstOrDefault();
                    record.OperationName = item.OperationName;
                    record.OperationType = item.OperationType;
                    record.OperationSequence = item.OperationSequence;
                    record.SubOperationIDs = item.SubOperationIDs;
                    record.DisplayFlag = item.DisplayFlag;
                    record.ScriptFileName = item.ScriptFileName;
                    record.RunTime = item.RunTime;

                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->EditConfiguration", SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool IsOperationSequenceNotExist(int sequence, int type, Guid operationId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.OperationConfigurations.Where(c => c.OperationSequence == sequence && c.OperationType == type && c.OperationID != operationId);
                    if (records.Count() == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->IsOperationSequenceNotExist", SessionInfo.ExperimentID);
                throw;
            }
        }

        public bool CanDelete(Guid operationId)
        {
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "select count(*) from RotationOperate where OperationID=@OperationID";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@OperationID", operationId);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            if (reader.Read())
                            {
                                int count = (int)reader.GetValue(0);
                                if (count > 0)
                                    return false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->CanDelete", SessionInfo.ExperimentID);
                throw;
            }

            return true;
        }

        public bool UpdateActiveStatus(Guid operationId, bool status)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    OperationConfiguration record = entities.OperationConfigurations.Where(p => p.OperationID == operationId).FirstOrDefault();
                    record.ActiveStatus = status;

                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->UpdateActiveStatus", SessionInfo.ExperimentID);
                return false;
            }
        }
    }
}
