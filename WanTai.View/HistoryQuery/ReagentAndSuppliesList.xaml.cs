using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using WanTai.Controller.Configuration;
using WanTai.DataModel;
using WanTai.Common;
using WanTai.Controller;

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for ReagentAndSuppliesList.xaml
    /// </summary>
    public partial class ReagentAndSuppliesList : Window
    {
        ReagentSuppliesConfigurationController reagentConfigController;
        ReagentAndSuppliesController reagentController;

        public ReagentAndSuppliesList()
        {
            InitializeComponent();
        }
        public ReagentAndSuppliesList(Guid experimentID)
        {
            InitializeComponent();

            reagentConfigController = new ReagentSuppliesConfigurationController();
            reagentController = new ReagentAndSuppliesController();

            DataTable dt = new DataTable();
            dt.Columns.Add("BarCode", typeof(string));
            dt.Columns.Add("DisplayName", typeof(string));
            dt.Columns.Add("Volume", typeof(double));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("ItemType", typeof(string));
            List<ReagentAndSuppliesConfiguration> reagentConfigs = reagentConfigController.GetAll();
            reagentConfigController.UpdateExperimentVolume(experimentID, ref reagentConfigs, new short[] { 
                ConsumptionType.consume }, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName);
            reagentConfigController.UpdateExperimentVolume(experimentID, ref reagentConfigs, new short[] { 
                ConsumptionType.Need }, ReagentAndSuppliesConfiguration.NeedVolumeFieldName);
            List<ReagentAndSupply> reagents = reagentController.GetAll(experimentID);
            List<DataModel.Configuration.ReagentSuppliesType> reagentType = Common.Configuration.GetReagentSuppliesTypes();
            foreach (ReagentAndSuppliesConfiguration r in reagentConfigs)
            {
                string itemTypeName = string.Empty;
                string barCode = string.Empty;
                DataModel.Configuration.ReagentSuppliesType rType = reagentType.FirstOrDefault(P => P.TypeId == r.ItemType.ToString());
                if (rType != null)
                {
                    itemTypeName = rType.TypeName;
                }
                ReagentAndSupply reagent = reagents.FirstOrDefault(P => P.ConfigurationItemID == r.ItemID);
                if (reagent != null)
                {
                    barCode = reagent.BarCode;
                }
                if (r.ItemType == 104 || r.ItemType == 105)
                {                    
                    dt.Rows.Add(barCode, r.DisplayName, Math.Abs(r.CurrentVolume), r.Unit, itemTypeName); 
                }
                else
                {
                    dt.Rows.Add(barCode, r.DisplayName, Math.Abs(r.CurrentVolume), r.Unit, itemTypeName);
                }
            }
            dgReagentSupplies.DataContext = dt.DefaultView;
        }
    }
}
