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
using WanTai.View.Control;
using WanTai.DataModel;
using WanTai.Controller;
using WanTai.Controller.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using WanTai.DataModel.Configuration;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for AddReagentsAndSupplies.xaml
    /// </summary>
    public partial class AddReagentsAndSupplies : Window
    {
        Dictionary<int, DataGrid> dataGridDictionary;
        DataTable dtReagent = new DataTable();

        public AddReagentsAndSupplies()
        {
            InitializeComponent();
            AddDataGrids();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag">flag=1 show, flag=2 add</param>
        public AddReagentsAndSupplies(int flag)
        {
            InitializeComponent();

            AddDataGrids();
            if (flag == 1)
            {
                WhenAdd();
                this.Title = "查看试剂";
            }
            else
            {
                WhenShow();
                this.Title = "查看试剂剩余量";
            }
        }

        List<CarrierBase> carrierBases = new List<CarrierBase>();
        List<PlateBase> viewPlates = new List<PlateBase>();

        private void AddDataGrids()
        {
            dataGridDictionary = new Dictionary<int, DataGrid>();
            List<ReagentSuppliesType> reagentTypes = Common.Configuration.GetReagentSuppliesTypes();
            foreach (ReagentSuppliesType reagent in reagentTypes)
            {
                short typeid = Convert.ToInt16(reagent.TypeId);
                if (typeid > 0 && typeid < 100 && typeid % 5 == 0)
                {
                    DataGrid dg = GetReagentDataGrid(reagent.TypeName);
                    dataGridDictionary.Add(typeid, dg);
                    panelPCRReagent.Children.Add(dg);
                    if (panelPCRReagent.FindName(reagent.TypeName) != null)
                        panelPCRReagent.UnregisterName(reagent.TypeName);
                    panelPCRReagent.RegisterName(reagent.TypeName, dg);
                }
            }
        }

        private void WhenAdd()
        {
            //DataGrid
            AddColumns(dtReagent);
            ReagentSuppliesConfigurationController configurationController = new ReagentSuppliesConfigurationController();
            List<ReagentAndSuppliesConfiguration> configurations = configurationController.GetAllActived().Where(P => P.ItemType < 100).OrderBy(P=>P.ItemType).ToList();
            configurationController.UpdateExperimentVolume(SessionInfo.ExperimentID, ref configurations, new short[] { 
                ConsumptionType.consume, ConsumptionType.Add, ConsumptionType.FirstAdd }, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName);
            //configurationController.UpdateExperimentVolume(SessionInfo.ExperimentID, ref configurations, new short[] { 
            //    ConsumptionType.Need }, ReagentAndSuppliesConfiguration.NeedVolumeFieldName);
            configurationController.NeededVolumeofProcessingRotations(ref configurations);
            List<ReagentAndSupply> reagents = new ReagentAndSuppliesController().GetAll(SessionInfo.ExperimentID);
            foreach (ReagentAndSuppliesConfiguration config in configurations)
            {
                if (config.ItemType >= 100) continue;
                Guid? reagentAndSuppplieID = null;
                config.Correct = config.Correct = config.NeedVolume == 0 ? true : config.CurrentVolume > config.NeedVolume * Common.Configuration.GetMinVolume();
                ReagentAndSupply reagent = reagents.FirstOrDefault(P => P.ConfigurationItemID == config.ItemID);
                if (reagent != null)
                {
                    reagentAndSuppplieID = reagent.ItemID;

                }
                dtReagent.Rows.Add(config.DisplayName, config.CurrentVolume, 0, config.Unit, config.Correct, config.ItemType, reagentAndSuppplieID, config.NeedVolume, config.NeedVolume * Common.Configuration.GetMinVolume(),config.ItemID);
            }

            dtReagent.DefaultView.Sort = "ItemType";
            dgReagent.DataContext = dtReagent.DefaultView.FindRows(0);
            foreach (short dataGridKey in dataGridDictionary.Keys)
            {
                dataGridDictionary[dataGridKey].ItemsSource = dtReagent.DefaultView.FindRows(dataGridKey);
            }

            //Drawing Plates and Carriers
            viewPlates = new Services.DeskTopService().SetReagentPosition(configurations, new CarrierController().GetCarrier(), 0);

            double lengthUnit = (this.Width - 50) / 84;
            double cooPoint = 1.4;
            panelDeskTop.Width = (this.Width - 50);
            panelDeskTop.Height = lengthUnit * 30;
            View.Services.DeskTopService desktopService = new Services.DeskTopService();
            carrierBases = desktopService.GetCarriers(900/84, cooPoint);

            foreach (CarrierBase carrier in carrierBases)
            {
                carrier.UpdatePlate(viewPlates.FindAll(P => P.ContainerName == carrier.CarrierName));
                // 修改 枪头及深孔板 js
                if (SessionInfo.WorkDeskType == "100" && carrier.CarrierName == "006")
                    continue;

                panelDeskTop.Children.Add(carrier);
            }

            int width = 0, limit = 0;
            if (SessionInfo.WorkDeskType == "100")
            {
                limit = 32;
                width = 500;
            }
            else if (SessionInfo.WorkDeskType == "150")
            {
                limit = 47;
                width = 700;
            }
            else if (SessionInfo.WorkDeskType == "200")
            {
                this.Width = 1000;
                limit = 66;
                width = 900;
            }
            if (SessionInfo.WorkDeskType == "200")
            {
                panelDeskTop.Children.Add(desktopService.DrawCoordinateOld(width, limit, lengthUnit + 1.2));
            }
            else
            {
                panelDeskTop.Children.Add(desktopService.DrawCoordinate(width, limit, lengthUnit + 1));
            }


            for (int i = 0; i < dtReagent.Rows.Count; i++)
            {
                if (!(bool)dtReagent.Rows[i]["Correct"])
                {
                    this.btnSave.Visibility = System.Windows.Visibility.Visible;
                }
            }


        }

        private void WhenShow()
        {
            WhenAdd();
            dgReagent.Columns[2].Visibility = dgReagent.Columns[4].Visibility = Visibility.Hidden;
            foreach (short dataGridKey in dataGridDictionary.Keys)
            {
                dataGridDictionary[dataGridKey].Columns[2].Visibility =
                    dataGridDictionary[dataGridKey].Columns[4].Visibility = Visibility.Hidden;
            }
            btnSave.Visibility = Visibility.Hidden;
        }

        private void AddColumns(DataTable dt)
        {
            dt.Columns.Add("DisplayName", typeof(string));
            dt.Columns.Add("CurrentVolume", typeof(double));
            dt.Columns.Add("AddVolume", typeof(double));
            dt.Columns.Add("Unit", typeof(string));
            dt.Columns.Add("Correct", typeof(bool));
            dt.Columns.Add("ItemType", typeof(short));
            dt.Columns.Add("ReagentAndSuppolieID", typeof(Guid));
            dt.Columns.Add("NeedVolume", typeof(double));
            dt.Columns.Add("MinVolume", typeof(double));
            dt.Columns.Add("ItemID", typeof(Guid));
        }

        private DataGrid GetReagentDataGrid(string itemName)
        {
            DataGrid dg = new DataGrid();
            dg.CellStyle = (Style)FindResource("Body_Content_DataGrid_Centering");
            dg.Name = itemName;
            DataGridTextColumn column0 = new DataGridTextColumn();
            column0.Header = itemName + "PCR配液";
            column0.Binding = new Binding("DisplayName");
            column0.IsReadOnly = true;

            Binding binding = new Binding("CurrentVolume");
            binding.StringFormat = "{0:0.000}";
            DataGridTextColumn column1 = new DataGridTextColumn();
            column1.Header = "剩余量";
            column1.Binding = binding;
            column1.IsReadOnly = true;

            Binding isEnabledBinding = new Binding("Correct");
            isEnabledBinding.Converter = new EnabledConverter();
            FrameworkElementFactory elementFactory2 = new FrameworkElementFactory(typeof(TextBox), "txtAddVolume");
            elementFactory2.SetBinding(TextBox.TextProperty, new Binding("AddVolume"));
            elementFactory2.SetValue(TextBox.IsEnabledProperty, isEnabledBinding);
            elementFactory2.SetValue(TextBox.MaxLengthProperty,20);
            elementFactory2.SetValue(TextBox.MaxWidthProperty,60.0);
            elementFactory2.SetValue(TextBox.WidthProperty, 60.0);
            DataTemplate template2 = new DataTemplate();
            template2.VisualTree = elementFactory2;
            DataGridTemplateColumn column2 = new DataGridTemplateColumn();
            column2.CellTemplate = template2;
            column2.Header = "添加量";

            binding = new Binding("MinVolume");
            binding.StringFormat = "{0:0.000}";
            DataGridTextColumn column3 = new DataGridTextColumn();
            column3.Header = "报警值";
            column3.Binding = binding;
            column3.IsReadOnly = true;

            DataGridTextColumn column4 = new DataGridTextColumn();
            column4.Header = "单位";
            column4.Binding = new Binding("Unit");
            column4.IsReadOnly = true;

            Binding visibilityBinding = new Binding("Correct");
            visibilityBinding.Converter = new VisibilityConverter();
            FrameworkElementFactory btnfactory = new FrameworkElementFactory(typeof(Button), "btnConfirm");
            btnfactory.SetValue(Button.ContentProperty, "确定");
            btnfactory.SetBinding(Button.VisibilityProperty, visibilityBinding);
            btnfactory.SetValue(Button.CommandParameterProperty, itemName);
            btnfactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(btnReagentConfirm_Click));
            DataTemplate template5 = new DataTemplate();
            template5.VisualTree = btnfactory;
            DataGridTemplateColumn column5 = new DataGridTemplateColumn();
            column5.CellTemplate = template5;
            column5.Visibility = Visibility.Visible;

            dg.Columns.Add(column0);
            dg.Columns.Add(column1);
            dg.Columns.Add(column2);
            dg.Columns.Add(column3);
            dg.Columns.Add(column4);
            dg.Columns.Add(column5);
            dg.CanUserAddRows = false;
            dg.CanUserDeleteRows = false;
            dg.CanUserReorderColumns = false;
            dg.CanUserResizeColumns = false;
            dg.CanUserResizeRows = false;
            dg.CanUserSortColumns = false;
            dg.AutoGenerateColumns = false;
            dg.IsReadOnly = true;
            return dg;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dtReagent.Rows.Count; i++)
            {
                if (!(bool)dtReagent.Rows[i]["Correct"])
                {
                    MessageBox.Show("请确认试剂" + dtReagent.Rows[i]["DisplayName"].ToString() + "。", "系统提示");
                    return;
                }
            }
            ReagentsAndSuppliesConsumptionController consumptionController = new ReagentsAndSuppliesConsumptionController();
            consumptionController.AddConsumption(dtReagent, 2);
            this.DialogResult = true;
            this.Close();
        }

        private void btnReagentConfirm_Click(object sender, RoutedEventArgs e)
        {
            string dataGridName = ((Button)sender).CommandParameter.ToString();
            DataGrid dataGrid = (DataGrid)panelPCRReagent.FindName(dataGridName);
            DataRowView item = (DataRowView)dataGrid.CurrentCell.Item;
            Regex r = new Regex("^([0-9]+)(.[0-9]{1,2})?$");
            FrameworkElement element = dataGrid.Columns[2].GetCellContent(item);
            DataGridTemplateColumn temple = (dataGrid.Columns[2] as DataGridTemplateColumn);
            TextBox txt = (TextBox)temple.CellTemplate.FindName("txtAddVolume", element);
            if (r.Match(txt.Text).Success)
            {
                if (Convert.ToDouble(txt.Text) + (double)item["CurrentVolume"] >= (double)item["NeedVolume"] * Common.Configuration.GetMinVolume())
                {
                    item["AddVolume"] = Convert.ToDouble(txt.Text);
                    item["Correct"] = true;
                    foreach(CarrierBase carrier in carrierBases)
                    {
                     //  carrier.ShiningStop(item["DisplayName"].ToString());
                      carrier.ShiningStop(new Guid(item["ItemID"].ToString()));
                    }
                }
                else
                {
                    MessageBoxResult msResult = MessageBox.Show("建议您至少添加" + ((double)item["NeedVolume"] * Common.Configuration.GetMinVolume() - (double)item["CurrentVolume"]).ToString("0.00") + "微升的" + item["DisplayName"].ToString()
                        + "。\n是否确认？选择“是”将完成确认，选择“否”将继续添加。",
                    "系统提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (msResult == MessageBoxResult.Yes)
                    {
                        item["AddVolume"] = Convert.ToDouble(txt.Text);
                        item["Correct"] = true;
                        foreach (CarrierBase carrier in carrierBases)
                        {
                            carrier.ShiningStop(new Guid(item["ItemID"].ToString()));
                         //  carrier.ShiningStop(item["DisplayName"].ToString());
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("请输大于零的数字，保留两位小数。", "系统提示");
            }
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 添加 工作台 
            string image = String.Format(@"/WanTag;component/Resources/Sample{0}.gif", SessionInfo.WorkDeskType);
            Sample.Source = new BitmapImage(new Uri(image, UriKind.Relative));
            // 隐藏 枪头 js
            if (SessionInfo.WorkDeskType == "100")
            {
                Sample.Margin = new Thickness(10, 0, 0, -25);
                Sample.Width = 130;
            }
            else if (SessionInfo.WorkDeskType == "150")
            {
                Sample.Margin = new Thickness(-15, 5, 0, -25);
                Sample.Width = 320;
            }
            else if (SessionInfo.WorkDeskType == "200")
            {
                Sample.Margin = new Thickness(-15, 5, 0, -27);
                Sample.Width = 440;
            }

            foreach (PlateBase plate in viewPlates)
            {
                if (plate.ItemType == 0)
                {
                    BindRelatedControls(dgReagent, plate);
                }
                else if (plate.ItemType < 100 && plate.ItemType % 5 == 0)
                {
                    List<ReagentSuppliesType> reagentSuppliesType = Common.Configuration.GetReagentSuppliesTypes();
                    ReagentSuppliesType reagentType = reagentSuppliesType.FirstOrDefault(P => P.TypeId == plate.ItemType.ToString());
                    if (reagentType != null && dataGridDictionary.ContainsKey(Convert.ToInt32(reagentType.TypeId)))
                    {
                        BindRelatedControls(dataGridDictionary[Convert.ToInt32(reagentType.TypeId)], plate);
                    }
                }
            }
            foreach (CarrierBase carrier in carrierBases)
            {
                carrier.UpdatePlate(viewPlates.FindAll(P => P.ContainerName == carrier.CarrierName));
                carrier.Scan();
            }
        }

        /// <summary>
        /// Binding Related Controls to Plate
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="plate"></param>
        private void BindRelatedControls(DataGrid dg, PlateBase plate)
        {
            for (int i = 0; i < dg.Items.Count; i++)
            {
                string strName = (dg.Columns[0].GetCellContent(dg.Items[i]) as TextBlock).Text;
                if (strName == plate.ChineseName)
                {
                    List<object> list = new List<object>();
                    list.Add(dg.Columns[0].GetCellContent(dg.Items[i]) as TextBlock);
                    list.Add(dg.Columns[1].GetCellContent(dg.Items[i]) as TextBlock);
                    list.Add((dg.Columns[2] as DataGridTemplateColumn).CellTemplate.FindName("txtAddVolume",
                        dg.Columns[2].GetCellContent(dg.Items[i])) as TextBox);
                    list.Add(dg.Columns[3].GetCellContent(dg.Items[i]) as TextBlock);
                    list.Add(dg.Columns[4].GetCellContent(dg.Items[i]) as TextBlock);
                    plate.RelatedControls = list;
                    break;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
