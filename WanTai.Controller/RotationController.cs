using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;
namespace WanTai.Controller
{
    public class RotationController
    {
        public object GetRotationIsBatch(Guid RotationID)
        {
           using (WanTai.DataModel.WanTaiEntities _WanTaiEntities=new DataModel.WanTaiEntities())
           {
               return _WanTaiEntities.RotationInfoes.Where(Rotation => Rotation.RotationID == RotationID).FirstOrDefault().TubesBatchID;
           }
        }

        public Dictionary<Guid, RotationInfo> GetRotations(RotationInfoStatus state)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                return _WanTaiEntities.RotationInfoes.Where(
                    P => (P.ExperimentID == SessionInfo.ExperimentID && P.State == (short)state)).ToDictionary(P=>P.RotationID);
            }
        }
    }
}
