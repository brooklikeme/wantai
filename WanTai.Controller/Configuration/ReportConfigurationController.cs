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
    public class ReportConfigurationController
    {

        public bool Create(ReportConfiguration configuration)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToReportConfigurations(configuration);
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

        public List<ReportConfiguration> GetAll()
        {
            List<ReportConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.ReportConfigurations.Where(r => r.WorkDeskType == SessionInfo.WorkDeskType);
                    recordList = records.ToList<ReportConfiguration>();
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

        public List<ReportConfiguration> GetAllActived()
        {
            List<ReportConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.ReportConfigurations.Where(c => c.WorkDeskType == SessionInfo.WorkDeskType && c.ActiveStatus == true);
                    recordList = records.ToList<ReportConfiguration>();
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
                    var record = entities.ReportConfigurations.Where(p => p.WorkDeskType == SessionInfo.WorkDeskType && p.ItemID == itemId).FirstOrDefault();
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

        public ReportConfiguration GetConfiguration(Guid itemId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ReportConfiguration record = entities.ReportConfigurations.Where(p => p.WorkDeskType == SessionInfo.WorkDeskType && p.ItemID == itemId).FirstOrDefault();

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

        public bool EditConfiguration(Guid itemId, ReportConfiguration item)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ReportConfiguration record = entities.ReportConfigurations.Where(p => p.WorkDeskType == SessionInfo.WorkDeskType && p.ItemID == itemId).FirstOrDefault();
                    record.DisplayName = item.DisplayName;
                    record.Position = item.Position;
                    record.CalculationFormula = item.CalculationFormula;
 
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

        public bool IsFormulaRegula(string formula)
        {
            List<int> operationsOrders = new List<int>();
            operationsOrders.AddRange(new int[] { 1, 2, 3, 4 });
            MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControl();
            sc.Language = "JavaScript";
            string newFormula = formula.Replace("PCRWorkListRowCount", "1")
                .Replace("PoolCountInTotal", "1")
                .Replace("PoolingWorkListRowCount", "1")
                .Replace("TestItemCountInTotal", "1")
                .Replace("PCRPlatesCount", "1");
            formula = newFormula;
            foreach (int operationOrder in operationsOrders)
            {
                newFormula = formula.Replace("[" + operationOrder + "]", "1");
                formula = newFormula;
            }
            foreach (ReagentSuppliesType strKey in Common.Configuration.GetReagentSuppliesTypes())
            {
                int typeID = Convert.ToInt32(strKey.TypeId);
                if (typeID > 0 && typeID % 5 == 0 && formula.Contains(strKey.TypeName))
                {
                    newFormula = formula.Replace(strKey.TypeName, "1");
                    formula = newFormula;
                }
            }
            try
            {
                sc.Eval(formula);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                return false;
            }
            return true;
        }

        public bool UpdateActiveStatus(Guid itemId, bool status)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ReportConfiguration record = entities.ReportConfigurations.Where(p => p.WorkDeskType == SessionInfo.WorkDeskType && p.ItemID == itemId).FirstOrDefault();
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
