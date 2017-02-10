using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;
namespace WanTai.Controller
{
    public class RotationInfoController
    {
        public List<RotationInfo> GetExperimentRotation(Guid ExperimentID)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                if (SessionInfo.BatchType == null)
                {
                    return _WanTaiEntities.RotationInfoes.Where(Rotation => Rotation.ExperimentID == ExperimentID).OrderBy(c => c.RotationSequence).ToList();
                }
                else
                {
                    return _WanTaiEntities.RotationInfoes.Where(Rotation => Rotation.ExperimentID == ExperimentID).OrderBy(c => c.RotationSequence).ToList();
                }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        
            }
        }
        /// <summary>
        /// 状态（0 Create ，10 processing 运行,20完成 finish、30失败 falled ）
        /// </summary>
        public void AddToRotationOperate(RotationOperate _RotationOperate)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                _WanTaiEntities.AddToRotationOperates(_RotationOperate);
                _WanTaiEntities.SaveChanges();
            }
        }

        public void UpdataRotationOperateStatus(RotationOperate _RotationOperate, RotationInfoStatus State)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                RotationOperate NewRotationOperate = _WanTaiEntities.RotationOperates.Where(rotationOperate => (rotationOperate.OperationID == _RotationOperate.OperationID && rotationOperate.RotationID == _RotationOperate.RotationID)).FirstOrDefault();
                if (NewRotationOperate == null)
                {
                    NewRotationOperate = new RotationOperate();
                    NewRotationOperate.RotationOperateID = WanTaiObjectService.NewSequentialGuid();
                    NewRotationOperate.StartTime = _RotationOperate.StartTime;
                    NewRotationOperate.ExperimentID = _RotationOperate.ExperimentID;
                    NewRotationOperate.OperationConfigurationReference.Value = _WanTaiEntities.OperationConfigurations.Where(Operation => Operation.OperationID == _RotationOperate.OperationID).FirstOrDefault();
                    NewRotationOperate.RotationID = _RotationOperate.RotationID;
                    NewRotationOperate.State = (short)State;
                    _WanTaiEntities.AddToRotationOperates(NewRotationOperate);
                }
                else
                {
                    NewRotationOperate.State = (short)State;
                    NewRotationOperate.EndTime = _RotationOperate.EndTime;
                    NewRotationOperate.ErrorLog = _RotationOperate.ErrorLog;
                }               

                _WanTaiEntities.SaveChanges();
            }
        }

        public void UpdataRotationStatus(Guid RotationID, RotationInfoStatus State)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                RotationInfo Rotation = _WanTaiEntities.RotationInfoes.Where(_Rotation => _Rotation.RotationID == RotationID).FirstOrDefault();
                if (Rotation != null)
                    Rotation.State = (short)State;
                
                _WanTaiEntities.SaveChanges();
            }
        }
        public void UpdataRotationStatus(Guid RotationID, Guid NextRotationID, RotationInfoStatus State, RotationInfoStatus NextState)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                RotationInfo Rotation = _WanTaiEntities.RotationInfoes.Where(_Rotation => _Rotation.RotationID == RotationID).FirstOrDefault();
                if (Rotation != null)
                    Rotation.State = (short)State;
                Rotation = _WanTaiEntities.RotationInfoes.Where(_Rotation => _Rotation.RotationID == NextRotationID).FirstOrDefault();
                if (Rotation != null)
                    Rotation.State = (short)NextState;
                _WanTaiEntities.SaveChanges();
            }
        }

        public void UpdataExperimentStatus(Guid ExperimentID, bool isFinal, ExperimentStatus State)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                ExperimentsInfo experiment = _WanTaiEntities.ExperimentsInfoes.Where(c => c.ExperimentID == ExperimentID).FirstOrDefault();
                if (experiment != null)
                {
                    experiment.State = (short)State;
                    if (isFinal)
                    {
                        experiment.EndTime = DateTime.Now;
                    }
                }

                _WanTaiEntities.SaveChanges();
            }
        }
        public void UpdataExperimentStatus(Guid ExperimentID, bool isFinal, ExperimentStatus State, Guid RotationID, RotationInfoStatus RotationState)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                ExperimentsInfo experiment = _WanTaiEntities.ExperimentsInfoes.Where(c => c.ExperimentID == ExperimentID).FirstOrDefault();
                if (experiment != null)
                {
                    experiment.State = (short)State;
                    if (isFinal)
                    {
                        experiment.EndTime = DateTime.Now;
                    }
                }

                RotationInfo Rotation = _WanTaiEntities.RotationInfoes.Where(_Rotation => _Rotation.RotationID == RotationID).FirstOrDefault();
                if (Rotation != null)
                    Rotation.State = (short)RotationState;
                _WanTaiEntities.SaveChanges();
            }
        }
        public void UpdateTubesBatch(Guid rotationID, Guid tuibesBatchID)
        {
            if (!(SessionInfo.BatchTimes > 1 && int.Parse(SessionInfo.BatchType) < SessionInfo.BatchTimes))
            {
                using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
                {
                    RotationInfo rotation = _WanTaiEntities.RotationInfoes.FirstOrDefault(P => P.RotationID == rotationID);
                    if (rotation != null)
                    {
                        rotation.TubesBatchID = tuibesBatchID;
                    }
                    _WanTaiEntities.SaveChanges();
                }
            }
        }        
    }
}
