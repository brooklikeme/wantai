using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;

namespace WanTai.Controller
{
    public class CarrierController
    {
        public List<Carrier> GetCarrier()
        {
            List<Carrier> list = new List<Carrier>();
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    list= entities.Carriers.Where(c => c.WorkDeskType == SessionInfo.WorkDeskType).ToList();                    
                }                
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID); 
            }
            return list;
        }
    }
}
