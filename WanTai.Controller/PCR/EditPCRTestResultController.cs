using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;

using WanTai.DataModel;
using WanTai.Common;

namespace WanTai.Controller.PCR
{
    public class EditPCRTestResultController
    {
        public bool UpdatePCRTestResult(Guid itemId, string resultContent)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var result = entities.PCRTestResults.Where(c => c.ItemID == itemId);
                    if (result != null && result.Count() > 0)
                    {
                        PCRTestResult testResult = result.FirstOrDefault();
                        testResult.Result = resultContent;
                        entities.SaveChanges();
                    }
                    
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->UpdatePCRTestResult", SessionInfo.ExperimentID);
                return false;
            }            
        }

        public string QueryPCRTestResult(Guid itemId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var result = entities.PCRTestResults.Where(c => c.ItemID == itemId);
                    if (result != null && result.Count() > 0)
                    {
                        PCRTestResult testResult = result.FirstOrDefault();
                        return testResult.PCRContent;
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->QueryPCRTestResult", SessionInfo.ExperimentID);               
            }

            return null;
        }
    }
}
