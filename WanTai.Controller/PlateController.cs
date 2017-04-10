using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WanTai.DataModel;

namespace WanTai.Controller
{
    public class PlateController
    {
        public void UpdateRotationId(Guid tubesBatchID, Guid rotationID)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                var plates = _WanTaiEntities.Plates.Where(P => P.TubesBatchID == tubesBatchID);
                foreach (Plate plate in plates)
                {
                    plate.RotationID = rotationID;
                }
                
                _WanTaiEntities.SaveChanges();
            }
        }

        public void UpdateBarcode(string plateName,short plateType, Guid rotationID,string barCode)
        { 
            using(WanTaiEntities _WanTaiEntities=new WanTaiEntities())
            {
                var plate = _WanTaiEntities.Plates.FirstOrDefault(P=>(P.RotationID==rotationID && P.PlateType==plateType && P.PlateName==plateName));
                if (plate != null)
                {
                    plate.BarCode = barCode;
                    _WanTaiEntities.SaveChanges();
                }
            }
        }

        public void UpdateBarcode(Guid plateID, string barCode)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                var plate = _WanTaiEntities.Plates.FirstOrDefault(P => (P.PlateID == plateID));
                if (plate != null)
                {
                    plate.BarCode = barCode;
                    _WanTaiEntities.SaveChanges();
                }
            }
        }
    }
}
