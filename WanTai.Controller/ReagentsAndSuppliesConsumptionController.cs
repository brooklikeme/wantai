using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;
using System.Data;

namespace WanTai.Controller
{
    public class ReagentsAndSuppliesConsumptionController
    {
        public void AddConsumption(ReagentsAndSuppliesConsumption consumption)
        {
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                //if (consumption.VolumeType == ConsumptionType.FirstAdd && entities.ReagentsAndSuppliesConsumptions.Where(P => P.VolumeType == ConsumptionType.FirstAdd && P.ReagentAndSupplieID == consumption.ReagentAndSupplieID).Count() != 0)
                //{
                //    //int  list = entities.ReagentsAndSuppliesConsumptions.Where(P => P.VolumeType == ConsumptionType.FirstAdd && P.ReagentAndSupplieID == consumption.ReagentAndSupplieID).Count();
                //    double volume = 0;
                //    List<ReagentsAndSuppliesConsumption> consumptions = entities.ReagentsAndSuppliesConsumptions.Where(P => P.ReagentAndSupplieID == consumption.ReagentAndSupplieID).ToList();
                //    foreach (ReagentsAndSuppliesConsumption c in consumptions)
                //    {
                //        if (c.VolumeType != ConsumptionType.Need)
                //        {
                //            volume += (double)c.Volume;
                //        }
                //    }
                //    consumption.Volume -= volume;
                //    if (consumption.Volume < 0)
                //        return;
                //    consumption.VolumeType = ConsumptionType.Add;
                //}
                //else if (consumption.VolumeType == ConsumptionType.FirstAdd)
                //{
                //    double volume = 0;
                //    ReagentAndSupply _reagent = entities.ReagentAndSupplies.Where(c => c.ItemID == consumption.ReagentAndSupplieID).FirstOrDefault();
                //    if (_reagent != null && _reagent.ItemType >= 100 && _reagent.ItemType < 200)
                //    {
                //        var allreagent = entities.ReagentAndSupplies.Where(c => c.ItemType == _reagent.ItemType && c.ExperimentID == consumption.ExperimentID);
                //        if (allreagent.Count() > 0)
                //        {
                //            List<Guid> ReagentAndSuppliesIdList = new List<Guid>();
                //            foreach (ReagentAndSupply reg in allreagent)
                //            {
                //                ReagentAndSuppliesIdList.Add(reg.ItemID);
                //            }

                //            if (entities.ReagentsAndSuppliesConsumptions.Where(P => ReagentAndSuppliesIdList.Contains<Guid>((Guid)P.ReagentAndSupplieID) && P.VolumeType == ConsumptionType.FirstAdd).Count() > 0)
                //            {
                //                List<ReagentsAndSuppliesConsumption> consumptions = entities.ReagentsAndSuppliesConsumptions.Where(P => ReagentAndSuppliesIdList.Contains<Guid>((Guid)P.ReagentAndSupplieID)).ToList();
                //                if (consumptions.Count > 0)
                //                {
                //                    foreach (ReagentsAndSuppliesConsumption c in consumptions)
                //                    {
                //                        if (c.VolumeType != ConsumptionType.Need)
                //                        {
                //                            volume += (double)c.Volume;
                //                        }
                //                    }
                //                    consumption.Volume -= volume;
                //                    if (consumption.Volume < 0)
                //                        return;
                //                    consumption.VolumeType = ConsumptionType.Add;
                //                }
                //            }
                //        }
                //    }
                //}
                entities.ReagentsAndSuppliesConsumptions.AddObject(consumption);
                entities.SaveChanges();
            }
        }
        public void UpdatePCRHeatLiquidPlateConsumption(ReagentsAndSuppliesConsumption consumption)
        {
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                ReagentsAndSuppliesConsumption Result = entities.ReagentsAndSuppliesConsumptions.Where(Reagent => Reagent.VolumeType == 1 && Reagent.RotationID == consumption.RotationID && Reagent.ReagentAndSupplieID == consumption.ReagentAndSupplieID).FirstOrDefault();
                if (Result == null)
                    entities.ReagentsAndSuppliesConsumptions.AddObject(consumption);
                else
                {
                    Result.Volume = consumption.Volume;
                    Result.UpdateTime = DateTime.Now;
                }
                entities.SaveChanges();
            }
        }

        public void UpdateConsumption(ReagentsAndSuppliesConsumption consumption)
        {
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                ReagentsAndSuppliesConsumption Result = entities.ReagentsAndSuppliesConsumptions.Where(Reagent => Reagent.VolumeType == 1 && Reagent.RotationID == consumption.RotationID && Reagent.ReagentAndSupplieID == consumption.ReagentAndSupplieID).FirstOrDefault();
                if (Result == null)
                    entities.ReagentsAndSuppliesConsumptions.AddObject(consumption);
                else
                {
                    Result.Volume = consumption.Volume;
                    Result.UpdateTime = DateTime.Now;
                }
                entities.SaveChanges();
            }
        }
        /// <summary>
        /// Get volume of one  experiment
        /// </summary>
        /// <param name="experimentID"></param>
        /// <param name="volumeType">0 need,1 consumption,2 add,3 firstAdd</param>
        /// <returns></returns>
        public List<ReagentsAndSuppliesConsumption> GetExperimentVolume(Guid experimentID, short[] volumeType)
        {
            List<ReagentsAndSuppliesConsumption> listcurrentVolume = new List<ReagentsAndSuppliesConsumption>();
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                var query =
                    entities.ReagentsAndSuppliesConsumptions.Where(P => (P.ExperimentID == experimentID && P.VolumeType != null && volumeType.Contains((short)P.VolumeType)))
                    .GroupBy(P => P.ReagentAndSupplieID);
                foreach (IGrouping<Guid?, ReagentsAndSuppliesConsumption> consumption in query)
                {
                    listcurrentVolume.Add(new ReagentsAndSuppliesConsumption
                      {
                          Volume = consumption.Sum(C => C.Volume),
                          ExperimentID = experimentID,
                          ReagentAndSupplieID = consumption.Key
                      });
                }
            }

            return listcurrentVolume;
        }


        /// <summary>
        /// NeededVolume of processing rotations
        /// </summary>
        /// <returns></returns>
        public List<ReagentsAndSuppliesConsumption> GetProcessingRotationsVolume()
        {
            List<ReagentsAndSuppliesConsumption> listNeededVolume = new List<ReagentsAndSuppliesConsumption>();
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                Dictionary<Guid, RotationInfo> processingRotation = new RotationController().GetRotations(RotationInfoStatus.Processing);
                var query =
                    entities.ReagentsAndSuppliesConsumptions.Where(P =>
                        (P.RotationID != null && processingRotation.Keys.Contains((Guid)P.RotationID)
                        && P.VolumeType == 0))
                    .GroupBy(P => P.ReagentAndSupplieID);
                foreach (IGrouping<Guid?, ReagentsAndSuppliesConsumption> consumption in query)
                {
                    listNeededVolume.Add(new ReagentsAndSuppliesConsumption
                    {
                        Volume = consumption.Sum(C => C.Volume),
                        ExperimentID = SessionInfo.ExperimentID,
                        ReagentAndSupplieID = consumption.Key
                    });
                }
            }

            return listNeededVolume;
        }

        /// <summary>
        /// Volume of one rotation
        /// </summary>
        /// <returns></returns>
        public List<ReagentsAndSuppliesConsumption> GetRotationVolume(Guid rotationID, short[] volumeType)
        {
            List<ReagentsAndSuppliesConsumption> listVolume = new List<ReagentsAndSuppliesConsumption>();
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                var query =
                    entities.ReagentsAndSuppliesConsumptions.Where(P =>
                        P.RotationID == rotationID && volumeType.Contains((short)P.VolumeType))
                    .GroupBy(P => P.ReagentAndSupplieID);
                foreach (IGrouping<Guid?, ReagentsAndSuppliesConsumption> consumption in query)
                {
                    listVolume.Add(new ReagentsAndSuppliesConsumption
                    {
                        Volume = consumption.Sum(C => C.Volume),
                        //ExperimentID = SessionInfo.ExperimentID,                        
                        ReagentAndSupplieID = consumption.Key
                    });
                }
            }

            return listVolume;
        }

        public void AddConsumption(Guid rotationID, Guid reagentAndSupplieID, double volume, short volumeType)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ReagentsAndSuppliesConsumption consumption = new ReagentsAndSuppliesConsumption();
                    consumption.ItemID = WanTaiObjectService.NewSequentialGuid();
                    consumption.Volume = volume;
                    consumption.UpdateTime = DateTime.Now;
                    consumption.ReagentAndSupplieID = reagentAndSupplieID;
                    consumption.ExperimentID = SessionInfo.ExperimentID;
                    consumption.RotationID = rotationID;
                    consumption.VolumeType = volumeType;
                    entities.ReagentsAndSuppliesConsumptions.AddObject(consumption);
                    entities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
            }
        }

        public void AddConsumption(DataTable dt, short volumeType)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (((double)dt.Rows[i]["AddVolume"]) > 0)
                        {
                            ReagentsAndSuppliesConsumption consumption = new ReagentsAndSuppliesConsumption();
                            consumption.ItemID = WanTaiObjectService.NewSequentialGuid();
                            consumption.Volume = (double)dt.Rows[i]["AddVolume"];
                            consumption.UpdateTime = DateTime.Now;
                            consumption.ReagentAndSupplieID = Guid.Parse(dt.Rows[i]["ReagentAndSuppolieID"].ToString());
                            consumption.ExperimentID = SessionInfo.ExperimentID;
                            consumption.RotationID = SessionInfo.PraperRotation.RotationID;
                            consumption.VolumeType = volumeType;
                            entities.ReagentsAndSuppliesConsumptions.AddObject(consumption);
                        }
                    }
                    entities.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
            }
        }
    }
}
