using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;

using WanTai.DataModel;

namespace WanTai.Controller.Configuration
{
    public class LiquidConfigurationController
    {
        public bool Create(SystemFluidConfiguration record)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToSystemFluidConfigurations(record);
                    entities.SaveChanges();
                    return true;
                }
            }
            catch(Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "." + MethodBase.GetCurrentMethod(), SessionInfo.ExperimentID); 
                return false;
            }
        }

        public List<SystemFluidConfiguration> GetLiquidConfigurationByTypeId(short typeId)
        {
            List<SystemFluidConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.SystemFluidConfigurations.Where(p => p.ItemType == typeId);
                    recordList = records.ToList<SystemFluidConfiguration>();                    
                }
            }
            catch(Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "." + MethodBase.GetCurrentMethod(), SessionInfo.ExperimentID);               
            }

            return recordList;
        }

        public List<SystemFluidConfiguration> GetAllLiquidConfiguration()
        {
            List<SystemFluidConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.SystemFluidConfigurations;
                    recordList = records.ToList<SystemFluidConfiguration>();
                }
            }
            catch(Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "." + MethodBase.GetCurrentMethod(), SessionInfo.ExperimentID);              
            }

            return recordList;
        }

        public bool Delete(short typeId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.SystemFluidConfigurations.Where(p => p.ItemType == typeId);
                    foreach(SystemFluidConfiguration record in records)
                    {
                        entities.DeleteObject(record);
                    }                    

                    entities.SaveChanges();
                    return true;
                }
            }
            catch(Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "." + MethodBase.GetCurrentMethod(), SessionInfo.ExperimentID); 
                return false;
            }
        }

        public bool EditByTypeId(List<SystemFluidConfiguration> recordList, short typeId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.SystemFluidConfigurations.Where(p => p.ItemType == typeId);
                    foreach (SystemFluidConfiguration record in records)
                    {
                        entities.DeleteObject(record);
                    }

                    foreach (SystemFluidConfiguration record in recordList)
                    {
                        record.ItemID = WanTaiObjectService.NewSequentialGuid();
                        entities.AddToSystemFluidConfigurations(record);
                    }

                    entities.SaveChanges();
                    return true;
                }
            }
            catch(Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "." + MethodBase.GetCurrentMethod(), SessionInfo.ExperimentID); 
                return false;
            }
        }
    }
}
