using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using WanTai.Controller;
using WanTai.Controller.Configuration;
using WanTai.DataModel;

namespace WanTai.View.Services
{
    public class ReagentWarningArgs : EventArgs
    {
        public List<ReagentAndSuppliesConfiguration> CurrentList { get; set; }
    }
    public delegate void ReagentWarningHandler(object sender, ReagentWarningArgs e);

    public class ReagentDetectService
    {
        public ReagentSuppliesConfigurationController reagentConfigController = new ReagentSuppliesConfigurationController();
        public ReagentsAndSuppliesConsumptionController reagentConsumptionController = new ReagentsAndSuppliesConsumptionController();
        public ReagentAndSuppliesController reagentController = new ReagentAndSuppliesController();
        public List<ReagentAndSuppliesConfiguration> CurrentList = new List<ReagentAndSuppliesConfiguration>();
        private double min = Common.Configuration.GetMinVolume();
        private int t = Common.Configuration.GetWarningSleepTime();
        public event ReagentWarningHandler RiseWarning;
        public event ReagentWarningHandler RiseNormal;
        public static bool RunningLiquidThread = false;

        public void CalacCorrentVolume()
        {
            ReagentSuppliesConfigurationController configurationController = new ReagentSuppliesConfigurationController();
            CurrentList = configurationController.GetAll();
            bool normalLevel = true;
            while (RunningLiquidThread)
            {
                configurationController.UpdateExperimentVolume(SessionInfo.ExperimentID, ref CurrentList, new short[] { 1, 2, 3 }, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName);
                configurationController.NeededVolumeofProcessingRotations(ref CurrentList);
                normalLevel = CheckCurrentVolume();
                if (normalLevel)
                {
                    RiseNormal(this, null);
                    Thread.Sleep(t);
                }
                else
                {
                    if (RiseWarning != null)
                    {
                        ReagentWarningArgs args = new ReagentWarningArgs();
                        args.CurrentList = CurrentList;
                        RiseWarning(this, args);
                    }
                }
            }
        }

        public bool CalacCorrentVolumeIsValid()
        {
            ReagentSuppliesConfigurationController configurationController = new ReagentSuppliesConfigurationController();
            CurrentList = configurationController.GetAll();
            bool normalLevel = true;

            configurationController.UpdateExperimentVolume(SessionInfo.ExperimentID, ref CurrentList, new short[] { 1, 2, 3 }, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName);
            configurationController.NeededVolumeofProcessingRotations(ref CurrentList);
            normalLevel = CheckCurrentVolume();
            return normalLevel;   
        }

        /// <summary>
        /// 只是检查试剂
        /// </summary>
        /// <returns></returns>
        private bool CheckCurrentVolume()
        {
            bool flag = true;
            foreach (ReagentAndSuppliesConfiguration confi in CurrentList)
            {
                if (confi.ItemType < 100 && confi.NeedVolume > 0 && confi.CurrentVolume <= confi.NeedVolume * min)
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        public bool CheckCurrentVolume(List<ReagentAndSuppliesConfiguration> _CurrentList)
        {
            bool flag = true;
            foreach (ReagentAndSuppliesConfiguration confi in _CurrentList)
            {
                if (confi.ItemType < 100 && confi.NeedVolume > 0 && confi.CurrentVolume <= confi.NeedVolume * min)
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 每个操作完成调用此方法添加耗材消耗量
        /// </summary>
        /// <param name="rotationID"></param>
        /// <param name="operationID"></param>
        public void AddSuppliesConsumption(Guid rotationID, Guid operationID, FormulaParameters formulaParameters)
        {
            Dictionary<short, bool> operation = new OperationController().GetOperationOrders(operationID);
            List<ReagentAndSuppliesConfiguration> suppliseConfig = reagentConfigController.GetAll().FindAll(P => P.ItemType >= 100 && P.ItemType < 200);
            List<ReagentAndSupply> supplise = reagentController.GetReagentID(rotationID).FindAll(P => P.ItemType >= 100 && P.ItemType < 200);
            foreach(ReagentAndSuppliesConfiguration supplyconfig in suppliseConfig)
            {
                ReagentAndSupply supply = supplise.FirstOrDefault(P => P.ConfigurationItemID == supplyconfig.ItemID);
                if (supply != null)
                {
                    Guid supplyID = supply.ItemID;
                    double consumption = reagentConfigController.CalcVolume(supplyconfig.CalculationFormula, operation, formulaParameters);
                    reagentConsumptionController.AddConsumption(rotationID, supplyID, 0-consumption, ConsumptionType.consume);
                }
            }
        }
    }
}
