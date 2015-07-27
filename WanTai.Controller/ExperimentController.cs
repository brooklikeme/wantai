using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using WanTai.DataModel;

namespace WanTai.Controller
{
    public class ExperimentController
    {
        public bool ExperimentNameExists(string experimentName)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ExperimentsInfo experimentInfo = entities.ExperimentsInfoes.FirstOrDefault(p => p.ExperimentName == experimentName);
                    if (experimentInfo != null)
                        return true;
                    else return false;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name,  SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool CreateExperiment(ExperimentsInfo experimentInfo)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToExperimentsInfoes(experimentInfo);
                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool DeleteExperiment(Guid experimentID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ExperimentsInfo experimentsInfo = entities.ExperimentsInfoes.FirstOrDefault(p => p.ExperimentID == experimentID);
                    entities.ExperimentsInfoes.DeleteObject(experimentsInfo);
                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool UpdateExperiment(ExperimentsInfo experimentInfo)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ExperimentsInfo tempExperimentsInfo = entities.ExperimentsInfoes.FirstOrDefault(p => p.ExperimentID == experimentInfo.ExperimentID);
                    if (tempExperimentsInfo != null)
                    {
                        tempExperimentsInfo.ExperimentName = experimentInfo.ExperimentName;
                        tempExperimentsInfo.Remark = experimentInfo.Remark;
                        entities.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                return false;
            }
        }
        public bool UpdateExperimentState(Guid ExperimentID,int State)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ExperimentsInfo tempExperimentsInfo = entities.ExperimentsInfoes.FirstOrDefault(p => p.ExperimentID == ExperimentID);
                    if (tempExperimentsInfo != null)
                    {
                        tempExperimentsInfo.State = (short)State;
                        entities.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                return false;
            }
        }
    }
}
