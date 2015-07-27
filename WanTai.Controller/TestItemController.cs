using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using WanTai.DataModel;
using System.Data;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
namespace WanTai.Controller
{
    public class TestItemController
    {
        //public ObservableCollection<TestingItemConfiguration> GetPoolingRulesConfigurations()
        //{
        //    using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
        //    {
        //        ObservableCollection<TestingItemConfiguration>  Result  =new ObservableCollection<TestingItemConfiguration>();
        //        foreach (TestingItemConfiguration _TextingItem in _WanTaiEntities.TestingItemConfigurations)
        //        {
        //            Result.Add(_TextingItem);
        //        }
        //        return Result;
        //    }
        //}
        //get all active TestItemConfigurations
        public IList<TestingItemConfiguration> GetActiveTestItemConfigurations()
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
               return _WanTaiEntities.TestingItemConfigurations.Where(c=>c.ActiveStatus==true).OrderBy( c=> c.DisplaySequence).ToList<TestingItemConfiguration>();
            }
        }

        //get all TestItemConfigurations
        public IList<TestingItemConfiguration> GetTestItemConfigurations()
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                return _WanTaiEntities.TestingItemConfigurations.OrderBy(c => c.DisplaySequence).ToList<TestingItemConfiguration>();
            }
        }

        public bool Create(TestingItemConfiguration configuration)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToTestingItemConfigurations(configuration);
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

        public TestingItemConfiguration GetTestingItem(Guid TestingItemID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    TestingItemConfiguration record = entities.TestingItemConfigurations.Where(c => c.TestingItemID == TestingItemID).FirstOrDefault();
                    return record;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetTestingItem", SessionInfo.ExperimentID);
                throw;
            }
        }

        public bool Delete(Guid TestingItemID)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var record = entities.TestingItemConfigurations.Where(c => c.TestingItemID == TestingItemID).FirstOrDefault();
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

        public bool EditTestingItems(Guid TestingItemID, TestingItemConfiguration item)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    TestingItemConfiguration record = entities.TestingItemConfigurations.Where(p => p.TestingItemID == TestingItemID).FirstOrDefault();
                    record.TestingItemName = item.TestingItemName;
                    record.TestingItemColor = item.TestingItemColor;
                    record.DisplaySequence = item.DisplaySequence;
                    record.WorkListFileName = item.WorkListFileName;

                    entities.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->EditTestingItems", SessionInfo.ExperimentID);
                return false;
            }
        }

        public bool CanDelete(Guid TestingItemID)
        {            
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "select count(*) from TestItem_TubeGroup where TestingItemID=@TestingItemID";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@TestingItemID", TestingItemID);

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

        public bool UpdateActiveStatus(Guid TestingItemID, bool status)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    TestingItemConfiguration record = entities.TestingItemConfigurations.Where(p => p.TestingItemID == TestingItemID).FirstOrDefault();
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
