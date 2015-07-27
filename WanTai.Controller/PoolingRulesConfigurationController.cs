using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using WanTai.DataModel;
using System.Data;
namespace WanTai.Controller
{
    public class PoolingRulesConfigurationController
    {
        public IList<PoolingRulesConfiguration> GetActivePoolingRulesConfigurations()
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.PoolingRulesConfigurations.Where(c=>c.ActiveStatus==true).OrderBy(c => c.TubeNumber);
                    return records.ToList<PoolingRulesConfiguration>();
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPoolingRulesConfigurations", SessionInfo.ExperimentID);
                throw;
            }
        }

        public IList<PoolingRulesConfiguration> GetPoolingRulesConfigurations()
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.PoolingRulesConfigurations.OrderBy(c => c.TubeNumber);
                    return records.ToList<PoolingRulesConfiguration>();
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPoolingRulesConfigurations", SessionInfo.ExperimentID);
                throw;
            }
        }

        public PoolingRulesConfiguration GetPoolingRulesConfiguration()
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                return _WanTaiEntities.PoolingRulesConfigurations.FirstOrDefault<PoolingRulesConfiguration>();
            }
        }

        public bool Create(PoolingRulesConfiguration configuration)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToPoolingRulesConfigurations(configuration);
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

        public PoolingRulesConfiguration GetPoolingRule(Guid PoolingRulesID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    PoolingRulesConfiguration record = entities.PoolingRulesConfigurations.Where(c => c.PoolingRulesID == PoolingRulesID).FirstOrDefault();
                    return record;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetPoolingRule", SessionInfo.ExperimentID);
                throw;
            }
        }

        public bool Delete(Guid PoolingRulesID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var record = entities.PoolingRulesConfigurations.Where(c => c.PoolingRulesID == PoolingRulesID).FirstOrDefault();
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

        public bool EditPoolingRules(Guid PoolingRulesID, PoolingRulesConfiguration item)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    PoolingRulesConfiguration record = entities.PoolingRulesConfigurations.Where(p => p.PoolingRulesID == PoolingRulesID).FirstOrDefault();
                    record.PoolingRulesName = item.PoolingRulesName;
                    record.TubeNumber = item.TubeNumber;

                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->EditPoolingRules", SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool UpdatePoolingRuleActiveStatus(Guid PoolingRulesID, bool status)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    PoolingRulesConfiguration record = entities.PoolingRulesConfigurations.Where(p => p.PoolingRulesID == PoolingRulesID).FirstOrDefault();
                    record.ActiveStatus = status;

                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->UpdatePoolingRuleActiveStatus", SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool CanDelete(Guid PoolingRulesID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.TubeGroups.Where(p => p.PoolingRulesID == PoolingRulesID).Count();
                    if (records == 0)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->CanDelete", SessionInfo.ExperimentID);
                return false;
            }
        } 
    }
}
