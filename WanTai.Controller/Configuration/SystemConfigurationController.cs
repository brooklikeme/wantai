using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using WanTai.DataModel;
using WanTai.DataModel.Configuration;
using System.Collections;

namespace WanTai.Controller.Configuration
{
    public class SystemConfigurationController
    {

        public bool Create(SystemConfiguration configuration)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToSystemConfigurations(configuration);
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

        public List<SystemConfiguration> GetAll()
        {
            List<SystemConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.SystemConfigurations.Where(r => r.WorkDeskType == SessionInfo.WorkDeskType);
                    recordList = records.ToList<SystemConfiguration>();
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetAll", SessionInfo.ExperimentID);
                throw;
            }

            return recordList;
        }

        public bool Delete(Guid itemId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var record = entities.SystemConfigurations.Where(p => p.WorkDeskType == SessionInfo.WorkDeskType && p.ItemID == itemId).FirstOrDefault();
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

        public SystemConfiguration GetConfiguration(Guid itemId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    SystemConfiguration record = entities.SystemConfigurations.Where(p => p.WorkDeskType == SessionInfo.WorkDeskType && p.ItemID == itemId).FirstOrDefault();

                    return record;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetConfiguration", SessionInfo.ExperimentID);
                throw;
            }
        }

        public bool EditConfiguration(Guid itemId, SystemConfiguration item)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    SystemConfiguration record = entities.SystemConfigurations.Where(p => p.WorkDeskType == SessionInfo.WorkDeskType && p.ItemID == itemId).FirstOrDefault();
                    record.ItemValue = item.ItemValue;

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
    }
}
