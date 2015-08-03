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
    public class ReagentSuppliesConfigurationController
    {
        public List<string> GetCarriers()
        {
            List<string> recordList = new List<string>();
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.Carriers.OrderBy(c => c.CarrierName);
                    foreach (Carrier carrier in records)
                    {
                        recordList.Add(carrier.CarrierName);
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetCarriers", SessionInfo.ExperimentID);
                throw;
            }

            return recordList;
        }

        public bool Create(ReagentAndSuppliesConfiguration configuration)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    entities.AddToReagentAndSuppliesConfigurations(configuration);
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

        public List<ReagentAndSuppliesConfiguration> GetAll()
        {
            List<ReagentAndSuppliesConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.ReagentAndSuppliesConfigurations;
                    recordList = records.ToList<ReagentAndSuppliesConfiguration>();
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

        public List<ReagentAndSuppliesConfiguration> GetAllActived()
        {
            List<ReagentAndSuppliesConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.ReagentAndSuppliesConfigurations.Where(c => c.ActiveStatus == true);
                    recordList = records.ToList<ReagentAndSuppliesConfiguration>();
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
                    var record = entities.ReagentAndSuppliesConfigurations.Where(p => p.ItemID == itemId).FirstOrDefault();
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

        public ReagentAndSuppliesConfiguration GetConfiguration(Guid itemId)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ReagentAndSuppliesConfiguration record = entities.ReagentAndSuppliesConfigurations.Where(p => p.ItemID == itemId).FirstOrDefault();

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

        public bool EditConfiguration(Guid itemId, ReagentAndSuppliesConfiguration item)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ReagentAndSuppliesConfiguration record = entities.ReagentAndSuppliesConfigurations.Where(p => p.ItemID == itemId).FirstOrDefault();
                    record.EnglishName = item.EnglishName;
                    record.DisplayName = item.DisplayName;
                    record.Position = item.Position;
                    record.Grid = item.Grid;
                    record.Unit = item.Unit;
                    record.ItemType = item.ItemType;
                    record.CalculationFormula = item.CalculationFormula;
                    record.ContainerName = item.ContainerName;
                    record.BarcodePrefix = item.BarcodePrefix;
                    record.Color = item.Color;

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

        /// <summary>
        /// volume of experiment
        /// </summary>
        /// <param name="list"></param>
        /// <param name="list">0 need,1 consumption,2 add,3 firstAdd</param>
        /// <param name="volumeFiled">the filed to update</param>
        public void UpdateExperimentVolume(Guid experimentID, ref List<ReagentAndSuppliesConfiguration> list, short[] volumeType, string volumeFiled)
        {
            ReagentsAndSuppliesConsumptionController consumptionController = new ReagentsAndSuppliesConsumptionController();
            List<ReagentsAndSuppliesConsumption> consumptions = consumptionController.GetExperimentVolume(experimentID, volumeType);
            ReagentAndSuppliesController reagentController = new ReagentAndSuppliesController();
            List<ReagentAndSupply> reagents = reagentController.GetAll(experimentID);
            foreach (ReagentAndSuppliesConfiguration config in list)
            {
                double volume = 0;
                List<ReagentAndSupply> listReagent = reagents.Where(P => P.ConfigurationItemID == config.ItemID).ToList();
                if (listReagent != null && listReagent.Count > 0)
                {
                    foreach (ReagentAndSupply reagent in listReagent)
                    {
                        ReagentsAndSuppliesConsumption consumption = consumptions.FirstOrDefault(P => P.ReagentAndSupplieID == reagent.ItemID);
                        if (consumption != null && consumption.Volume != null)
                        {
                            volume += (double)consumption.Volume;
                        }
                    }

                    if (config.ItemType == DiTiType.DiTi200 || config.ItemType == DiTiType.DiTi1000)
                    {
                        config.CurrentActualVolume = volume;
                        volume = volume / 96;
                        volume = Math.Ceiling(volume * 10) / 10;
                    }

                    //if (volume < 0)
                    //    volume = 0.0;

                    Type type = typeof(ReagentAndSuppliesConfiguration);
                    FieldInfo filedInfo = type.GetField(volumeFiled, BindingFlags.NonPublic | BindingFlags.Instance);
                    if (filedInfo != null)
                    {
                        filedInfo.SetValue(config, volume);
                    }
                }
            }
        }

        /// <summary>
        /// volume of experiment
        /// </summary>
        /// <param name="list"></param>
        /// <param name="list">0 need,1 consumption,2 add,3 firstAdd</param>
        /// <param name="volumeFiled">the filed to update</param>
        public void UpdateExperimentTotalNeedConsumptionVolume(Guid experimentID, ref List<ReagentAndSuppliesConfiguration> list)
        {
            ReagentsAndSuppliesConsumptionController consumptionController = new ReagentsAndSuppliesConsumptionController();
            List<ReagentsAndSuppliesConsumption> consumptions = consumptionController.GetExperimentVolume(experimentID, new short[] { ConsumptionType.Need, ConsumptionType.consume });
            ReagentAndSuppliesController reagentController = new ReagentAndSuppliesController();
            List<ReagentAndSupply> reagents = reagentController.GetAll(experimentID);
            foreach (ReagentAndSuppliesConfiguration config in list)
            {
                double volume = 0;
                List<ReagentAndSupply> listReagent = reagents.Where(P => P.ConfigurationItemID == config.ItemID).ToList();
                if (listReagent != null && listReagent.Count > 0)
                {
                    foreach (ReagentAndSupply reagent in listReagent)
                    {
                        ReagentsAndSuppliesConsumption consumption = consumptions.FirstOrDefault(P => P.ReagentAndSupplieID == reagent.ItemID);
                        if (consumption != null && consumption.Volume != null)
                        {
                            volume += (double)consumption.Volume;
                        }
                    }

                    if (config.ItemType == DiTiType.DiTi200 || config.ItemType == DiTiType.DiTi1000)
                    {
                        volume = volume / 96;
                        volume = Math.Ceiling(volume * 10) / 10;
                    }

                    config.TotalNeedValueAndConsumption = volume;
                }
            }
        }

        /// <summary>
        /// NeededVolume of Processing rotations
        /// </summary>
        /// <param name="list"></param>
        public void NeededVolumeofProcessingRotations(ref List<ReagentAndSuppliesConfiguration> list)
        {
            ReagentsAndSuppliesConsumptionController consumptionController = new ReagentsAndSuppliesConsumptionController();
            List<ReagentsAndSuppliesConsumption> consumptions = consumptionController.GetProcessingRotationsVolume();
            ReagentAndSuppliesController reagentController = new ReagentAndSuppliesController();
            List<ReagentAndSupply> reagents = reagentController.GetAll(SessionInfo.ExperimentID);
            foreach (ReagentAndSuppliesConfiguration config in list)
            {
                double neededVolume = 0;
                List<ReagentAndSupply> listReagent = reagents.Where(P => P.ConfigurationItemID == config.ItemID).ToList();
                if (listReagent != null && listReagent.Count > 0)
                {
                    foreach (ReagentAndSupply reagent in listReagent)
                    {
                        ReagentsAndSuppliesConsumption consumption = consumptions.FirstOrDefault(P => P.ReagentAndSupplieID == reagent.ItemID);
                        if (consumption != null && consumption.Volume != null)
                        {
                            neededVolume += (double)consumption.Volume;
                        }
                    }
                    config.NeedVolume = neededVolume;
                }
            }
        }

        /// <summary>
        /// Volume of one rotation
        /// </summary>
        /// <param name="list">0 need,1 consumption,2 add,3 firstAdd</param>
        public List<ReagentAndSuppliesConfiguration> GetRotationVolume(Guid experimentID, Guid rotationID, short[] volumeType, string volumeFiled)
        {
            ReagentsAndSuppliesConsumptionController consumptionController = new ReagentsAndSuppliesConsumptionController();
            List<ReagentsAndSuppliesConsumption> consumptions = consumptionController.GetRotationVolume(rotationID, volumeType);
            ReagentAndSuppliesController reagentController = new ReagentAndSuppliesController();
            List<ReagentAndSupply> reagents = reagentController.GetAll(experimentID);
            ReagentSuppliesConfigurationController configurationController = new ReagentSuppliesConfigurationController();
            List<ReagentAndSuppliesConfiguration> list = configurationController.GetAllActived();
            foreach (ReagentAndSuppliesConfiguration config in list)
            {
                double volume = 0;
                List<ReagentAndSupply> listReagent = reagents.Where(P => P.ConfigurationItemID == config.ItemID).ToList();
                if (listReagent != null && listReagent.Count > 0)
                {
                    foreach (ReagentAndSupply reagent in listReagent)
                    {
                        ReagentsAndSuppliesConsumption consumption = consumptions.FirstOrDefault(P => P.ReagentAndSupplieID == reagent.ItemID);
                        if (consumption != null && consumption.Volume != null)
                        {
                            volume += (double)consumption.Volume;
                        }
                    }
                }
                Type type = typeof(ReagentAndSuppliesConfiguration);
                FieldInfo fieldInfo = type.GetField(volumeFiled, BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(config, volume);
                }
            }
            return list;
        }

        /// <summary>
        /// ReagentAndSuppliesConfiguration of one rotation
        /// </summary>
        /// <param name="operationsOrders"></param>
        /// <returns></returns>
        public List<ReagentAndSuppliesConfiguration> GetReagentAndSuppliesNeeded(Dictionary<short, bool> operationsOrders, FormulaParameters formulaParameters)
        {
            List<ReagentAndSuppliesConfiguration> list = new List<ReagentAndSuppliesConfiguration>();
            try
            {
                using (WanTaiEntities wanTaiEntites = new WanTaiEntities())
                {
                    list = wanTaiEntites.ReagentAndSuppliesConfigurations.Where(c => c.ActiveStatus == true).ToList();
                }
                foreach (ReagentAndSuppliesConfiguration r in list)
                {
                    // 修改 枪头 js @ReagentAndSuppliesConfiguration
                    double volumne = CalcVolume(r.CalculationFormula, operationsOrders, formulaParameters);
                    r.NeedVolume = r.ItemType >= 100 ? Math.Ceiling(volumne) : volumne;
                    if (r.ItemType == DiTiType.DiTi200 || r.ItemType == DiTiType.DiTi1000)
                    {
                        r.ActualSavedVolume = volumne;
                        r.NeedVolume = Math.Ceiling(volumne / 96 * 10) / 10;
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                throw;
            }
            return list;
        }

        public List<ReagentAndSuppliesConfiguration> GetReagentAndSuppliesNeeded(short[] itemTypes, Dictionary<short, bool> operationsOrders, FormulaParameters formulaParameters)
        {
            List<ReagentAndSuppliesConfiguration> list = new List<ReagentAndSuppliesConfiguration>();
            try
            {
                using (WanTaiEntities wanTaiEntites = new WanTaiEntities())
                {
                    list = wanTaiEntites.ReagentAndSuppliesConfigurations.Where(c => c.ActiveStatus == true && itemTypes.Contains((short)c.ItemType)).ToList();
                }
                foreach (ReagentAndSuppliesConfiguration r in list)
                {
                    double volumne = CalcVolume(r.CalculationFormula, operationsOrders, formulaParameters);
                    r.NeedVolume = r.ItemType >= 100 ? Math.Ceiling(volumne) : volumne;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                throw;
            }
            return list;
        }



        public double CalcVolume(string formula, Dictionary<short, bool> operationsOrders, FormulaParameters formulaParameters)
        {
            double volume = 0;
            MSScriptControl.ScriptControl sc = new MSScriptControl.ScriptControl();
            sc.Language = "JavaScript";
            string newFormula = "";
            string tempFormula = "";

            foreach (short operationOrder in operationsOrders.Keys)
            {
                tempFormula = operationsOrders[operationOrder] ?
                    formula.Replace("[" + operationOrder + "]", "1") :
                    formula.Replace("[" + operationOrder + "]", "0");
                formula = tempFormula;
            }

            newFormula = formula.Replace("PCRWorkListRowCount", formulaParameters.PCRWorkListRowCount.ToString())
               .Replace("PoolCountInTotal", formulaParameters.PoolCountInTotal.ToString())
               .Replace("PoolingWorkListRowCount", formulaParameters.PoolingWorkListRowCount.ToString())
               .Replace("TestItemCountInTotal", formulaParameters.TestItemCountInTotal.ToString())
               .Replace("PCRPlatesCount", formulaParameters.PCRPlatesCount.ToString());
            formula = newFormula;

            foreach (string strKey in formulaParameters.PoolCountOfTestItem.Keys)
            {
                if (formula.Contains(strKey))
                {
                    newFormula = formula.Replace(strKey, formulaParameters.PoolCountOfTestItem[strKey].ToString());
                    formula = newFormula;
                }
            }
            volume = new Common.SafeConvertion().GetSafeDouble(sc.Eval(formula));
            return volume;
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

        public string GetBarcodePrefix(short itemType)
        {
            string barcodePrefix = null;
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                ReagentAndSuppliesConfiguration reagentConfig = entities.ReagentAndSuppliesConfigurations.FirstOrDefault(P => P.ItemType == itemType);
                if (reagentConfig != null)
                    barcodePrefix = reagentConfig.BarcodePrefix;
                else
                {
                    LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, itemType.ToString() + "doesn't exist ein ReagentAndSuppliesConfiguration", "", this.GetType().Name, null);
                }
            }
            return barcodePrefix;
        }

        public List<ReagentAndSuppliesConfiguration> GetByItemType(Guid ExpId)
        {
            short[] ItemTypes = GetItemType(ExpId);
            List<ReagentAndSuppliesConfiguration> recordList = null;
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    var records = entities.ReagentAndSuppliesConfigurations;
                    

                    for (int i = 0; i < ItemTypes.Length; i++)
                    {
                        List<ReagentAndSuppliesConfiguration> recordsubList = records.ToList<ReagentAndSuppliesConfiguration>().FindAll(P => P.ItemType == ItemTypes[i]);

                        if (recordList == null)
                        {
                            recordList = recordsubList;
                        }
                        else
                        {
                            for (int j = 0; j < recordsubList.Count; j++)
                            {
                                recordList.Add(recordsubList[j]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetByItemType", SessionInfo.ExperimentID);
                throw;
            }

            return recordList;
        }

        private short[] GetItemType(Guid expId)
        {
            ArrayList arr = new ArrayList();
            string connectionString = WanTai.Common.Configuration.GetConnectionString();
            string commandText = "select distinct ras.ItemType";
            commandText += " from   ReagentsAndSuppliesConsumption rasc";
            commandText += " inner join ReagentAndSupplies ras";
            commandText += " on rasc.ReagentAndSupplieId=ras.ItemId";
            commandText += " where  rasc.VolumeType=0";
            commandText += " and    rasc.ExperimentId=@ExperimentId";
            commandText += " and    ras.ItemType>0";
            commandText += " and    ras.ItemType<100";
            commandText += " and    ras.ItemType%5=0";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(commandText, conn))
                {
                    cmd.Parameters.AddWithValue("@ExperimentId", expId);

                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                    {
                        while (reader.Read())
                        {
                            arr.Add((short)reader.GetValue(0));
                        }
                    }
                }
            }

            short[] ItemTypes = new short[arr.Count + 1];
            ItemTypes[0]=(short)ReagentType.GeneralReagent;
            for (int i = 0; i < arr.Count; i++)
            {
                ItemTypes[i+1] = (short)arr[i];
            }

            return ItemTypes;
        }

        public bool CanDelete(Guid itemId)
        {
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                string commandText = "select count(*) from ReagentAndSupplies where ConfigurationItemID=@ConfigurationItemID";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@ConfigurationItemID", itemId);

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

        public bool UpdateActiveStatus(Guid itemId, bool status)
        {
            try
            {
                using (WanTaiEntities entities = new WanTaiEntities())
                {
                    ReagentAndSuppliesConfiguration record = entities.ReagentAndSuppliesConfigurations.Where(p => p.ItemID == itemId).FirstOrDefault();
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
