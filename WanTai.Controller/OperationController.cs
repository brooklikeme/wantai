using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;

namespace WanTai.Controller
{
    public class OperationController
    {
        public void GetSingleSubOperations(Guid operationID, ref List<OperationConfiguration> operations)
        {
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                OperationConfiguration operation = entities.OperationConfigurations.FirstOrDefault(P => P.OperationID == operationID);
                if (operation != null)
                {
                    if (operation.OperationType == 0)
                    {
                        operations.Add(operation);
                    }
                    else
                    {
                        string subOperationIDs = operation.SubOperationIDs;
                        if (!string.IsNullOrEmpty(subOperationIDs))
                        {
                            string[] IDs = subOperationIDs.Split(new char[] { ',' });
                            Guid guid;
                            for (int i = 0; i < IDs.Length; i++)
                            {
                                try
                                {
                                    guid = Guid.Parse(IDs[i]);
                                    GetSingleSubOperations(guid, ref operations);
                                }
                                catch (Exception ex)
                                {
                                    LogInfoController.AddLogInfo(LogInfoLevelEnum.Error,ex.Message, null, this.GetType().ToString() + "->" + "GetSingleSubOperations(Guid operationID, ref List<OperationConfiguration> operations)", null);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }

        public List<OperationConfiguration> GetAllSingleOperations()
        {
            List<OperationConfiguration> list = new List<OperationConfiguration>();
            using (WanTaiEntities entities = new WanTaiEntities()) 
            {
                list = entities.OperationConfigurations.Where(P => P.OperationType == 0).OrderBy(Operation => Operation.OperationSequence).ToList();
                return list;
            }
        }
        public List<OperationConfiguration> GetAllOperations()
        {
            List<OperationConfiguration> list = new List<OperationConfiguration>();
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                list = entities.OperationConfigurations.ToList();
                return list;
            }
        }
        public Dictionary<short, bool> GetOperationOrders(Guid operationID)
        {
            Dictionary<short, bool> operationOders = new Dictionary<short, bool>();

            List<OperationConfiguration> singleOperations = GetAllSingleOperations();
            List<OperationConfiguration> subSingleOperations = new List<OperationConfiguration>();
            GetSingleSubOperations(operationID,ref subSingleOperations);
            foreach (OperationConfiguration operation in singleOperations)
            {
                if (!operationOders.ContainsKey(operation.OperationSequence))
                    operationOders.Add(operation.OperationSequence, false);
            }
            foreach (OperationConfiguration operation in subSingleOperations)
            {
               if(operationOders.ContainsKey(operation.OperationSequence))
                    operationOders[operation.OperationSequence] = true;
            }
            return operationOders;
        }

        public Guid GetSameSequenceSubOperation(Guid operationID)
        {
            Guid subOperationID = Guid.Empty;
            using(WanTaiEntities _WanTaiEntities =new WanTaiEntities())
            {
                OperationConfiguration operation = _WanTaiEntities.OperationConfigurations.FirstOrDefault(P=>P.OperationID==operationID);
                if (operation != null || !string.IsNullOrEmpty( operation.SubOperationIDs.Trim()) )
                {
                    string[] subIDs = operation.SubOperationIDs.Split(new char[]{','});
                    List<Guid> listSubIDs = new List<Guid>();
                    for (int i = 0; i < subIDs.Count();i++ )
                    {
                        listSubIDs.Add(new Guid(subIDs[i]));
                    }
                    List<OperationConfiguration> subOperations=new List<OperationConfiguration>();
                    subOperations = _WanTaiEntities.OperationConfigurations.Where(P => listSubIDs.Contains(P.OperationID)).ToList();
                    foreach (OperationConfiguration subOperate in subOperations)
                    {
                        if (subOperate.OperationSequence == operation.OperationSequence)
                        {
                            subOperationID = subOperate.OperationID;
                            break;
                        }
                    }
                }
            }
            return subOperationID;
        }
    }
}
