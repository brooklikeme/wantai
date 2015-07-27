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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.Odbc;
using System.Text.RegularExpressions;
using WanTai.View.Control;
using WanTai.DataModel;
using WanTai.Controller;
using WanTai.Controller.Configuration;
using WanTai.Common;
using System.Threading;
using WanTai.View.Services;
using WanTai.DataModel.Configuration;
using System.IO;
using System.Windows.Markup;
using System.ComponentModel;
namespace WanTai.View
{
    /// <summary>
    /// The DeskTop length:heigh=840:300
    /// 
    /// Interaction logic for DeskTop.xaml
    /// </summary>
    public partial class DeskTop : Page
    {
        private double cooPoint = 1.4;
        private double lengthUnit = 0;

        List<Control.PlateBase> ViewPlates = new List<PlateBase>();
        List<ReagentAndSuppliesConfiguration> reagentAndSupplies = new List<ReagentAndSuppliesConfiguration>();
        Dictionary<string, DataGrid> dataGridDictionary = new Dictionary<string, DataGrid>();
        List<ReagentSuppliesType> reagentSuppliesType = new List<ReagentSuppliesType>();

        SafeConvertion saveConvertion = new SafeConvertion();

        public DeskTop()
        {
            InitializeComponent();

            dataGridDictionary.Add("supplies", dgSupplies);
            dataGridDictionary.Add("reagent", dgReagent);
            reagentSuppliesType = Common.Configuration.GetReagentSuppliesTypes();
            foreach (ReagentSuppliesType reagentType in reagentSuppliesType)
            {
                short typeid = Convert.ToInt16(reagentType.TypeId);
                if (typeid != 0 && typeid < 100 && typeid % 5 == 0)
                {
                    DataGrid dg = NewTestItemGrid(reagentType.TypeName);
                    stackPanelTestItem.Children.Add(dg);
                    dataGridDictionary.Add(reagentType.TypeName, dg);
                }
            }
            btnTestItem = new Button();
            btnTestItem.Content = "添加需要量";
            btnTestItem.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            btnTestItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            btnTestItem.Height = 25;
            btnTestItem.Width = 100;
            btnTestItem.Click += new RoutedEventHandler(btnNeedVolume_Click);

            stackPanelTestItem.Children.Add(btnTestItem);
        }
        private Button btnTestItem = new Button();
        #region Private Method
        private DataGrid NewTestItemGrid(string testItem)
        {
            DataGrid dg = new DataGrid();
            dg.CellStyle = (Style)FindResource("Body_Content_DataGrid_Centering");
            DataGridTextColumn column0 = new DataGridTextColumn();
            column0.Header = testItem + " PCR配液";
            column0.Binding = new Binding("DisplayName");
            column0.CanUserReorder = false;
            column0.CanUserSort = false;
            column0.IsReadOnly = true;
            column0.MinWidth =130;
           // column0.Width = new DataGridLength(1, DataGridLengthUnitType.Star) ;
            Binding binding = new Binding("NeedVolume");
            binding.StringFormat = "{0:0.000}";
            DataGridTextColumn column1 = new DataGridTextColumn();
            column1.Header = "需求量";
            column1.Binding = binding;
            column1.CanUserReorder = false;
            column1.CanUserSort = false;
            column1.IsReadOnly = true;

            binding = new Binding("CurrentVolume");
            binding.StringFormat = "{0:0.000}";
            DataGridTextColumn column2 = new DataGridTextColumn();
            column2.Header = "剩余量";
            column2.Binding = binding;
            column2.CanUserReorder = false;
            column2.CanUserSort = false;
            column2.IsReadOnly = true;
            column2.Visibility = Visibility.Hidden;

            Binding enabledBing = new Binding("Correct");
            enabledBing.Converter = new EnabledConverter();
            FrameworkElementFactory txtfactory = new FrameworkElementFactory(typeof(TextBox), "txtAddVolume");
            txtfactory.SetBinding(TextBox.TextProperty, new Binding("FirstAddVolume"));
            txtfactory.SetValue(TextBox.MaxLengthProperty,6);
            //txtfactory.SetValue(TextBox.WidthProperty, new DataGridLength(50));
            txtfactory.SetBinding(TextBox.IsEnabledProperty, enabledBing);
            DataTemplate template3 = new DataTemplate();
            template3.VisualTree = txtfactory;
            DataGridTemplateColumn column3 = new DataGridTemplateColumn();
            column3.Header = "添加量";
            column3.CellTemplate = template3;
            column3.CanUserReorder = false;
            column3.CanUserSort = false;
            column3.Width = 50;
            //column3.MaxWidth = 60;

            DataGridTextColumn column4 = new DataGridTextColumn();
            column4.Header = "单位";
            column4.Binding = new Binding("Unit");
            column4.CanUserReorder = false;
            column4.CanUserSort = false;
            column4.IsReadOnly = true;

            Binding visibilityBinding = new Binding("Correct");
            visibilityBinding.Converter = new VisibilityConverter();
            FrameworkElementFactory btnfactory = new FrameworkElementFactory(typeof(Button), "btnConfirm");
            btnfactory.SetValue(Button.ContentProperty, "确定");
            btnfactory.SetBinding(Button.VisibilityProperty, visibilityBinding);
            btnfactory.SetValue(Button.CommandParameterProperty, testItem);
            btnfactory.AddHandler(Button.ClickEvent, new RoutedEventHandler(btnConfirm_Click));
            DataTemplate template5 = new DataTemplate();
            template5.VisualTree = btnfactory;
            DataGridTemplateColumn column5 = new DataGridTemplateColumn();
            column5.CellTemplate = template5;
            column5.Visibility = Visibility.Hidden;

            dg.Columns.Add(column0);
            dg.Columns.Add(column1);
            dg.Columns.Add(column2);
            dg.Columns.Add(column3);
            dg.Columns.Add(column4);
            dg.Columns.Add(column5);
            dg.AutoGenerateColumns = false;
            dg.SelectionMode = DataGridSelectionMode.Extended;
            dg.SelectionUnit = DataGridSelectionUnit.CellOrRowHeader;
            dg.CanUserAddRows = false;
            dg.CanUserResizeColumns = false;
            dg.CanUserResizeRows = false;
            dg.IsReadOnly = true;
            return dg;           
        }

        public void InitDeskTop(double width)
        {
            labelRotationName.Content = SessionInfo.PraperRotation == null ? "" : SessionInfo.PraperRotation.RotationName;
            lengthUnit = width / 84;
            imgKingFisher.Width = lengthUnit * 24;
            imgKingFisher.Height = lengthUnit * 24;
            imgKingFisher.Margin = new Thickness(-lengthUnit * 2 * cooPoint, lengthUnit * 2, 0, lengthUnit * 2);
            DeskTopWithGrid.Width = width;
            DeskTopWithGrid.Height = 30 * lengthUnit;

            ViewPlates.Clear();
            reagentAndSupplies.Clear();
            btnSave.IsEnabled = false;
            DrawDeskTopInGrid();
            DeskTopWithGrid.Children.Add(new View.Services.DeskTopService().DrawCoordinate(width,69, lengthUnit));
        }

        bool isFirstRotation;

        public void DrawDeskTopInGrid()
        {
            reagentAndSupplies.Clear();

            RotationInfo firstRotation = new ConfigRotationController().GetCurrentRotationInfos(SessionInfo.ExperimentID).FirstOrDefault();
            if (firstRotation == null) return;

            isFirstRotation = firstRotation.RotationID == SessionInfo.PraperRotation.RotationID;

            //Binding DataGrid
            Dictionary<short, bool> operationOrders = new OperationController().GetOperationOrders(SessionInfo.PraperRotation.OperationID);

            ReagentSuppliesConfigurationController configController = new ReagentSuppliesConfigurationController();
            reagentAndSupplies = configController.GetReagentAndSuppliesNeeded(operationOrders, SessionInfo.RotationFormulaParameters[SessionInfo.PraperRotation.RotationID]);
            if (!isFirstRotation)
            {
                configController.UpdateExperimentVolume(SessionInfo.ExperimentID, ref reagentAndSupplies, new short[]{ConsumptionType.FirstAdd,ConsumptionType.Add,
                    ConsumptionType.consume}, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName);
                configController.UpdateExperimentTotalNeedConsumptionVolume(SessionInfo.ExperimentID, ref reagentAndSupplies);
                foreach (DataGrid dg in dataGridDictionary.Values)
                {
                    dg.Columns[2].Visibility = Visibility.Visible;
                    dg.Columns[5].Visibility = Visibility.Hidden;
                }
            }

            else
            {
                foreach (DataGrid dg in dataGridDictionary.Values)
                {
                    dg.Columns[2].Visibility = Visibility.Hidden;
                    dg.Columns[5].Visibility = Visibility.Hidden;
                }
            }

            dgSupplies.ItemsSource = reagentAndSupplies.Where(P => (short)P.ItemType >= 100 && (short)P.ItemType < 200).ToList();
            dgReagent.ItemsSource = reagentAndSupplies.Where(P => (short)P.ItemType == 0).ToList();
            List<ReagentSuppliesType> reagentSuppliesType = Common.Configuration.GetReagentSuppliesTypes();
            foreach (ReagentSuppliesType reagentType in reagentSuppliesType)
            {
                short typeid = Convert.ToInt16(reagentType.TypeId);
                if (0 != typeid && typeid < 100 && typeid % 5 == 0 && dataGridDictionary.ContainsKey(reagentType.TypeName))
                {
                    dataGridDictionary[reagentType.TypeName].ItemsSource = reagentAndSupplies.Where(P => (short)P.ItemType == typeid).OrderByDescending(P => P.Grid);
                }
            }

            //Binding UserControl
            Services.DeskTopService deskTopService = new DeskTopService();
            ViewPlates.Clear();
            ViewPlates = deskTopService.SetReagentPosition(reagentAndSupplies, new CarrierController().GetCarrier(), 0);//create plates
            
            List<CarrierBase> carriers = deskTopService.GetCarriers(lengthUnit, cooPoint);

            foreach (CarrierBase c in carriers)
            {

                c.UpdatePlate(ViewPlates.FindAll(P => P.ContainerName == c.CarrierName));
                if (c.CarrierName == "001" && !isFirstRotation)
                {                    
                    List<PlateBase> greenPlate = ViewPlates.FindAll(P => P.ItemType == 101 && (P.DisplayName.Contains("1") || P.DisplayName.Contains("2")));
                    string[] plateNames = new string[greenPlate.Count];
                    int index = 0;
                    foreach (PlateBase plate in greenPlate)
                    {
                        plateNames[index] = plate.DisplayName;
                  
                        index++;
                    }
                    c.ShiningWithGreen(plateNames);
                }
                if (c.CarrierName == "006")
                {     //labelRotationName.ActualWidth 40       labDiTi1000.ActualWidth 50
                    labDiTi1000.Margin = new Thickness(c.Margin.Left + c.Width / 2 - 50 / 2 - 40 -10, 0, 0, 0);
                }
                if (c.CarrierName == "007")
                {
                    labDiTi200.Margin = new Thickness(c.Margin.Left + c.Width / 6 * 4 / 2 - 50 / 2 - 10 - labDiTi1000.Margin.Left - 50 - 50, 0, 0, 0);
                }
                DeskTopWithGrid.Children.Add(c);
            }            
        }
     
        private void BindRelatedControls()
        {
            foreach (PlateBase plate in ViewPlates)
            {
                if (plate.ItemType == 0)
                {
                    BindRelatedControls(dgReagent, plate);
                }
                else if (plate.ItemType < 100 && plate.ItemType % 5 == 0)
                {
                    List<ReagentSuppliesType> reagentSuppliesType = Common.Configuration.GetReagentSuppliesTypes();
                    ReagentSuppliesType reagentType = reagentSuppliesType.FirstOrDefault(P => P.TypeId == plate.ItemType.ToString());
                    if (reagentType != null && dataGridDictionary.ContainsKey(reagentType.TypeName))
                    {
                        BindRelatedControls(dataGridDictionary[reagentType.TypeName], plate);
                    }
                }
                else
                {
                    BindRelatedControls(dgSupplies, plate);
                }
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
                    list.Add((dg.Columns[3] as DataGridTemplateColumn).CellTemplate.FindName("txtAddVolume",
                        dg.Columns[3].GetCellContent(dg.Items[i])) as TextBox);
                    plate.RelatedControls = list;
                    break;
                }
            }
        }

        /// <summary>
        /// When Plates has change,call this method to update UI
        /// </summary>
        private void UpdatePlates()
        {
            foreach (UIElement uiElement in DeskTopWithGrid.Children)
            {
                if (uiElement is CarrierBase)
                {
                    CarrierBase carrier;
                    if (uiElement is View.Control.Carrier)
                    {
                        carrier = (View.Control.Carrier)uiElement;
                    }
                    else if (uiElement is View.Control.Shelf)
                    {
                        carrier = (View.Control.Shelf)uiElement;
                    }
                    else
                    {
                        carrier = (View.Control.Heater)uiElement;
                    }

                    carrier.UpdatePlate(ViewPlates.FindAll(P => P.ContainerName == carrier.CarrierName));
                }
            }
        }

        private DataTable GetScanResult(DateTime fileBeforeCreatedTime)
        {
            string fileDB = Common.Configuration.GetEvoOutputPath();
            string scanFilePath = fileDB + WanTai.Common.Configuration.GetScanResultFileName();
            if (System.IO.File.Exists(scanFilePath))
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(scanFilePath);
                DateTime fileCreatedTime = fileInfo.LastWriteTime;
                if (DateTime.Compare(fileBeforeCreatedTime, fileCreatedTime) > 0)
                {
                    throw new Exception("System can not find the scan file");
                }
            }

            DataTable dtScanResult = new DataTable();
            dtScanResult.Columns.Add(new DataColumn("Grid", typeof(int)));
            dtScanResult.Columns.Add(new DataColumn("Position", typeof(int)));
            dtScanResult.Columns.Add(new DataColumn("Barcode", typeof(string)));
            dtScanResult.Columns.Add(new DataColumn("Volume", typeof(double)));

            DataTable dt = Common.CommonFunction.ReadScanFile(fileDB, "select * from " + WanTai.Common.Configuration.GetScanResultFileName());
            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string strResult = dt.Rows[i][0].ToString();
                    string[] results = strResult.Split(new char[] { ';' });
                    dtScanResult.Rows.Add(Convert.ToInt32(results[0]), Convert.ToInt32(results[1]), results[6], 1);
                }
            }
            return dtScanResult;
        }

        private void GetDetectedLiquid(DateTime fileBeforeCreateTime)
        {
            string fileDB = Common.Configuration.GetEvoOutputPath();
            string fileName = Common.Configuration.GetLiquidDetection();
            string str = "-";
            System.IO.FileInfo fileinfo = new System.IO.FileInfo(fileDB + fileName);
            DataTable dt = new DataTable();

            if (fileinfo.Exists)
            {
                if (fileinfo.LastWriteTime.CompareTo(fileBeforeCreateTime) > 0)
                {
                    dt = Common.CommonFunction.ReadScanFile(fileDB, "select * from " + fileName);
                    if (dt.Rows.Count == 0)
                        return;
                    foreach (ReagentAndSuppliesConfiguration reagent in reagentAndSupplies)
                    {
                        if (reagent.ItemType != 0)
                            continue;
                        string columneName = reagent.EnglishName.Replace(str, string.Empty);
                        Regex regex = new Regex("^" + columneName + "(\\s[1-9])?([0-9]*)$");
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            if (regex.IsMatch(dt.Columns[i].ColumnName))
                            {
                                if (!(dt.Rows[0][i] is DBNull))
                                {
                                    reagent.FirstAddVolume += Convert.ToDouble(dt.Rows[0][i]);
                                }
                                Control.PlateBase plate = ViewPlates.FirstOrDefault(P => (P.ChineseName == reagent.DisplayName));
                                if (plate != null)
                                {
                                    plate.FirstAddVolume = reagent.FirstAddVolume;
                                    plate.Correct = plate.FirstAddVolume >= (plate.NeedVolume + plate.CurrentVolume);
                                }
                            }
                        }
                    }
                }
            }
        }

        private DataTable GetDitiScanResult(DateTime fileBeforeCreatedTime)
        {
            string fileDB = Common.Configuration.GetEvoOutputPath();

            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(fileDB);
            System.IO.FileInfo[] fileinfos = directoryInfo.GetFiles("scan_*.csv");

            DataTable dtScanResult = new DataTable();
            dtScanResult.Columns.Add(new DataColumn("Grid", typeof(int)));
            dtScanResult.Columns.Add(new DataColumn("Position", typeof(int)));
            dtScanResult.Columns.Add(new DataColumn("TypeName", typeof(string)));
            dtScanResult.Columns.Add(new DataColumn("Barcode", typeof(string)));
            dtScanResult.Columns.Add(new DataColumn("Volume", typeof(double)));

            DataTable dt;
            if (fileinfos.Count() > 0)
            {
                foreach (System.IO.FileInfo fileInfo in fileinfos)
                {
                    if (DateTime.Compare(fileBeforeCreatedTime, fileInfo.LastWriteTime) > 0)
                    {
                        continue;
                    }

                    dt = Common.CommonFunction.ReadScanFile(fileDB, "select * from " +
                    fileInfo.Name);
                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string strResult = dt.Rows[i][0].ToString();
                            string[] results = strResult.Split(new char[] { ';' });
                            dtScanResult.Rows.Add(Convert.ToInt32(results[0]), Convert.ToInt32(results[1]), results[3], results[6], 1);
                        }
                    }
                }
            }
            return dtScanResult;
        }

        private DataTable GetHeaterScanResult(DateTime fileBeforeCreatedTime)
        {
            DataTable dt = new DataTable();
            string pcrFilePath = WanTai.Common.Configuration.GetEvoOutputPath() + WanTai.Common.Configuration.GetPCRDetection();
            if (System.IO.File.Exists(pcrFilePath))
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(pcrFilePath);
                DateTime fileCreatedTime = fileInfo.LastWriteTime;
                if (DateTime.Compare(fileBeforeCreatedTime, fileCreatedTime) > 0)
                {
                    return dt;
                }
                string fileDB = WanTai.Common.Configuration.GetEvoOutputPath();
                string fileName = WanTai.Common.Configuration.GetPCRDetection();
                dt = CommonFunction.ReadScanFile(fileDB, "select * from " + fileName);
            }
            return dt;
        }
        /// <summary>
        /// Check plates by Scan Result,and Update Volume to DataGrid
        /// </summary>
        /// <param name="dtScan"></param>
        private void CheckByScanResult(DataTable dtScan)
        {
            for (int i = 0; i < dtScan.Rows.Count; i++)
            {
                foreach (Control.PlateBase plate in ViewPlates)
                {
                    if (plate.ItemType < 100)
                        continue;
                    if (plate.CarrierGrid == (int)dtScan.Rows[i]["Grid"]
                        && plate.Position == (int)dtScan.Rows[i]["Position"]
                        && dtScan.Rows[i]["Barcode"] != null
                        && dtScan.Rows[i]["Barcode"].ToString().StartsWith(plate.BarcodePrefix)
                    )
                    {
                        plate.Barcode = dtScan.Rows[i]["Barcode"].ToString();
                        plate.FirstAddVolume = 0;
                        plate.Correct = true;
                        ReagentAndSuppliesConfiguration reagent = reagentAndSupplies.FirstOrDefault(P => P.DisplayName == plate.ChineseName);
                        if (reagent != null)
                        {
                            reagent.FirstAddVolume += (double)dtScan.Rows[i]["Volume"];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check DitiPlate by Scan Result,and Update Volume(Plate Count) to DataGrid
        /// </summary>
        /// <param name="dtScan"></param>    
        private void CheckDitiByScanResult(DataTable dtScan)
        {
            int Diti1000 = 0;
            int Diti200 = 0;
            string Diti1000BarcodePrefix = new ReagentSuppliesConfigurationController().GetBarcodePrefix(DiTiType.DiTi1000);
            string Diti200BarcodePrefix = new ReagentSuppliesConfigurationController().GetBarcodePrefix(DiTiType.DiTi200);
            PlateBase plate = null;
            for (int i = 0; i < dtScan.Rows.Count; i++)
            {
                if (Diti1000BarcodePrefix != null && dtScan.Rows[i]["Barcode"].ToString().StartsWith(Diti1000BarcodePrefix) && dtScan.Rows[i]["TypeName"].ToString() == "DiTi 1000ul")
                {
                    switch (Diti1000)
                    {
                        case 0:
                            plate = ViewPlates.FirstOrDefault(
                                P => (P.ContainerName == "005" && P.Position == 1));
                            break;
                        case 1:
                            plate = ViewPlates.FirstOrDefault(
                               P => (P.ContainerName == "005" && P.Position == 2));
                            break;
                        default:
                            plate = ViewPlates.FirstOrDefault(
                               P => (P.ContainerName == "006" && P.Position == Diti1000));
                            break;
                    }
                    Diti1000++;
                }
                else if (Diti200BarcodePrefix != null && dtScan.Rows[i]["Barcode"].ToString().StartsWith(Diti200BarcodePrefix) && dtScan.Rows[i]["TypeName"].ToString() == "DiTi 200 ul")
                {
                    switch (Diti200)
                    {
                        case 0:
                            plate = ViewPlates.FirstOrDefault(
                                P => (P.ContainerName == "005" && P.Position == 3));
                            break;
                        default:
                            plate = ViewPlates.FirstOrDefault(
                               P => (P.ContainerName == "007" && P.Position == Diti200 + 1));
                            break;
                    }
                    Diti200++;
                }
                else
                {
                    continue;
                }

                if (plate != null)
                {
                    plate.Correct = true;
                    plate.Barcode = dtScan.Rows[i]["Barcode"].ToString();
                    plate.FirstAddVolume = 0;
                }

                foreach (ReagentAndSuppliesConfiguration reagent in reagentAndSupplies)
                {
                    if (dtScan.Rows[i]["Barcode"].ToString().StartsWith(reagent.BarcodePrefix))
                    {
                        reagent.FirstAddVolume += (double)dtScan.Rows[i]["Volume"];
                        break;
                    }
                }
            }
        }

        private void CheckHeaterScanResult(DataTable dtScan)
        {
            if (dtScan.Rows.Count == 0) return;
            string str1 = "+";
            string str2 = "-";

            List<PlateBase> PCRLiquidTups = ViewPlates.FindAll(P => (P.ItemType > 0 && P.ItemType < 100 && P.ItemType % 5 == 0));
            foreach (PlateBase tub in PCRLiquidTups)
            {
                string liquidName = tub.EnglishName.Replace(str1, string.Empty).Replace(str2, string.Empty);
                ReagentSuppliesType liquidType = reagentSuppliesType.FirstOrDefault(P => P.TypeId == tub.ItemType.ToString());
                if (liquidType != null)
                {
                    liquidName = liquidType.TypeName + " " + liquidName;
                }
                if (dtScan.Columns.Contains(liquidName))
                {
                    double columnValue = saveConvertion.GetSafeDouble(dtScan.Rows[0][liquidName]);
                    tub.FirstAddVolume = columnValue;
                    tub.Correct = tub.FirstAddVolume >= (tub.NeedVolume + tub.CurrentVolume);
                    ReagentAndSuppliesConfiguration reagent = reagentAndSupplies.FirstOrDefault(P => P.BarcodePrefix == tub.BarcodePrefix);
                    if (reagent != null)
                        reagent.FirstAddVolume += tub.FirstAddVolume;
                }
            }
        }
        #endregion

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string gridName = ((Button)sender).CommandParameter.ToString();
            ReagentAndSuppliesConfiguration selectedItem = (ReagentAndSuppliesConfiguration)dataGridDictionary[gridName].CurrentCell.Item;
            FrameworkElement eletem = dataGridDictionary[gridName].Columns[3].GetCellContent(selectedItem);
            DataGridTemplateColumn temple = (dataGridDictionary[gridName].Columns[3] as DataGridTemplateColumn);
            TextBox txt = (TextBox)temple.CellTemplate.FindName("txtAddVolume", eletem);
            ConfirmAddVolume(txt.Text.Trim(), selectedItem);
            btnSave.IsEnabled = reagentAndSupplies.FirstOrDefault(P => P.Correct == false) == null;
        }

        private bool ConfirmAddVolume(string addVolume, ReagentAndSuppliesConfiguration item)
        {
            Regex r;
            string regexMessage;
            if (item.ItemType < 100)
            {
                r = new Regex("^([0-9]+)(\\.([0-9])+)?$");
                regexMessage = "格式错误，请输入非零数字";
            }
            else
            {
                r = new Regex("^([0-9]+)$");
                regexMessage = "格式错误，请输入正整数";
            }
            if (!r.Match(addVolume).Success)
            {
                MessageBox.Show(regexMessage, "系统提示");
                return false;
            }
            item.FirstAddVolume = Math.Round(Convert.ToDouble(addVolume), 3);

            if (isFirstRotation)
            {
                if (item.FirstAddVolume < item.NeedVolume)
                {
                    string message = item.DisplayName + "为" + addVolume.ToString() + item.Unit + "建议量为" +
                    (item.NeedVolume).ToString() + item.Unit + "。是否继续确认？“是”确认，“否”继续添加。";
                    MessageBoxResult msResult = MessageBox.Show(message, "系统提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (msResult == MessageBoxResult.No)
                    {
                        return false;
                    }
                }
            }
            else
            {
                //check if 剩余量>总需求量-消耗量
                if (item.CurrentVolume < (item.TotalNeedValueAndConsumption + item.NeedVolume))
                {
                    //check if 当前量>总需求量-消耗量
                    if ((item.FirstAddVolume + item.CurrentVolume) < (item.TotalNeedValueAndConsumption + item.NeedVolume))
                    {
                        string message = item.DisplayName + "为" + addVolume.ToString() + item.Unit + "建议量为" +
                        (item.TotalNeedValueAndConsumption + item.NeedVolume - item.CurrentVolume).ToString() + item.Unit + "。是否继续确认？“是”确认，“否”继续添加。";
                        MessageBoxResult msResult = MessageBox.Show(message, "系统提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (msResult == MessageBoxResult.No)
                        {
                            return false;
                        }
                    }
                }
            }


            item.Correct = true;
            if (ViewPlates.FirstOrDefault(P => (P.ChineseName == item.DisplayName && P.ItemType == item.ItemType)) != null)
            {
                ViewPlates.FirstOrDefault(P => (P.ChineseName == item.DisplayName && P.ItemType == item.ItemType)).FirstAddVolume = item.FirstAddVolume;
            }

            foreach (UIElement uiElement in DeskTopWithGrid.Children)
            {
                if (uiElement is CarrierBase)
                {
                    ((CarrierBase)uiElement).ShiningStop(item.ItemID);
                }
            }
            return true;
        }

        public event MainPage.NextStepHandler NextStepEvent;

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            btnScan.IsEnabled = btnSave.IsEnabled = btnManual.IsEnabled = false;
            if (NextStepEvent != null)
            {
                SessionInfo.NextButIndex = 4;
                NextStepEvent(sender, e);
            }
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataGrid dg in dataGridDictionary.Values)
            {
                foreach (ReagentAndSuppliesConfiguration item in dg.Items)
                {
                    string strAddVolume = ((dg.Columns[3] as DataGridTemplateColumn).CellTemplate.FindName("txtAddVolume", dg.Columns[3].GetCellContent(item)) as TextBox).Text.Trim();
                    if (item.Correct)
                    {
                        continue;
                    }
                    else
                    {
                        if (ConfirmAddVolume(strAddVolume, item))
                        {
                            continue;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            btnSave.IsEnabled = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            //MessageBoxResult msResult = MessageBox.Show("确认保存么", "系统提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
            //if (msResult == MessageBoxResult.No)
            //{
            //    return;
            //}

            ReagentsAndSuppliesConsumptionController consumptionController = new ReagentsAndSuppliesConsumptionController();
            ReagentAndSuppliesController reagentController = new ReagentAndSuppliesController();
            PlateController plateController = new PlateController();
            foreach (PlateBase plate in ViewPlates)
            {
                if (plate.ItemType == 101 && (plate.DisplayName.EndsWith("1") || plate.DisplayName.EndsWith("2")))
                {
                    string plateName = "";
                    if (plate.DisplayName.EndsWith("1"))
                        plateName = PlateName.DWPlate1;
                    if (plate.DisplayName.EndsWith("2"))
                        plateName = PlateName.DWPlate2;
                    plateController.UpdateBarcode(plateName, 0, SessionInfo.PraperRotation.RotationID, plate.Barcode);
                }
                ReagentAndSupply reagent = new ReagentAndSupply();
                if ((isFirstRotation) || (!isFirstRotation && (plate.ItemType >= 100 && plate.ItemType < 200)))
                {
                    reagent.ItemID = WanTaiObjectService.NewSequentialGuid();
                    reagent.BarCode = plate.Barcode;
                    reagent.ItemType = plate.ItemType;
                    reagent.ExperimentID = SessionInfo.ExperimentID;
                    reagent.ConfigurationItemID = plate.ConfigurationItemID;
                    reagentController.AddReagentAndSupplies(reagent);
                }
                else
                {
                    reagent.ItemID = new ReagentAndSuppliesController().GetReagentID(SessionInfo.ExperimentID, plate.BarcodePrefix);
                    if (reagent.ItemID == Guid.Empty)
                    {
                        reagent.ItemID = WanTaiObjectService.NewSequentialGuid();
                        reagent.BarCode = plate.Barcode;
                        reagent.ItemType = plate.ItemType;
                        reagent.ExperimentID = SessionInfo.ExperimentID;
                        reagent.ConfigurationItemID = plate.ConfigurationItemID;
                        reagentController.AddReagentAndSupplies(reagent);
                    }
                }

                ReagentsAndSuppliesConsumption calcReagentConsumption = new ReagentsAndSuppliesConsumption();
                calcReagentConsumption.ItemID = WanTaiObjectService.NewSequentialGuid();
                if (plate.ActualSavedVolume > 0)
                {
                    calcReagentConsumption.Volume = plate.ActualSavedVolume;
                }
                else
                {
                    calcReagentConsumption.Volume = plate.NeedVolume;
                }

                calcReagentConsumption.UpdateTime = DateTime.Now;
                calcReagentConsumption.ExperimentID = SessionInfo.ExperimentID;
                calcReagentConsumption.RotationID = SessionInfo.PraperRotation.RotationID;
                calcReagentConsumption.VolumeType = ConsumptionType.Need;
                calcReagentConsumption.ReagentAndSupplieID = reagent.ItemID;

                consumptionController.AddConsumption(calcReagentConsumption);

                ReagentsAndSuppliesConsumption scanReagentConsumption = new ReagentsAndSuppliesConsumption();
                scanReagentConsumption.ItemID = WanTaiObjectService.NewSequentialGuid();
                scanReagentConsumption.UpdateTime = DateTime.Now;
                scanReagentConsumption.ExperimentID = SessionInfo.ExperimentID;
                scanReagentConsumption.RotationID = SessionInfo.PraperRotation.RotationID;
                scanReagentConsumption.VolumeType = ConsumptionType.FirstAdd;

                if (reagent.ItemType == DiTiType.DiTi200 || reagent.ItemType == DiTiType.DiTi1000)
                {
                    scanReagentConsumption.Volume = plate.FirstAddVolume * 96;
                }
                else
                {
                    scanReagentConsumption.Volume = plate.FirstAddVolume;
                }

                scanReagentConsumption.ReagentAndSupplieID = reagent.ItemID;
                consumptionController.AddConsumption(scanReagentConsumption);
            }

            btnScan.IsEnabled = false;
            btnManual.IsEnabled = false;
            btnSave.IsEnabled = false;
            btnNext.IsEnabled = true;
            btnSupplies.IsEnabled = false;
            btnReagent.IsEnabled = false;
            btnTestItem.IsEnabled = false;
            SessionInfo.NextButIndex = 3;
            ////MessageBox.Show("保存成功！", "系统提示");
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            LoadFrom loadFrom = new LoadFrom();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            bool WaitFalg = true;
            string ErrMsg = "";
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                DateTime fileBeforeCreatedTime = WanTai.Controller.EVO.ProcessorFactory.GetDateTimeNow();
                Services.DeskTopService desktopService = new Services.DeskTopService();
                try
                {
                    desktopService.CallScanScript();
                }
                catch (Exception ex)
                {

                    MessageBox.Show("扫描错误！", "系统提示");
                    return;
                }

                //get data from scan files, check plates ,and update scan volume of reagents
                foreach (ReagentAndSuppliesConfiguration reagent in reagentAndSupplies)
                {
                    reagent.FirstAddVolume = 0;
                }
                foreach (PlateBase plate in ViewPlates)
                {
                    plate.FirstAddVolume = 0;
                }
                DataTable dtScan = GetScanResult(fileBeforeCreatedTime);
                DataTable dtDitiScan = GetDitiScanResult(fileBeforeCreatedTime);
                DataTable dtHeaterScan = GetHeaterScanResult(fileBeforeCreatedTime);

                CheckByScanResult(dtScan);
                CheckDitiByScanResult(dtDitiScan);
                CheckHeaterScanResult(dtHeaterScan);
                GetDetectedLiquid(fileBeforeCreatedTime);

                //if any plate of a reagent is incorrect this reagent is incorrect
                foreach (ReagentAndSuppliesConfiguration reagent in reagentAndSupplies)
                {
                    List<PlateBase> plates = ViewPlates.FindAll(P => (P.ChineseName == reagent.DisplayName && P.ItemType == reagent.ItemType));
                    reagent.Correct = !plates.Exists(P => !P.Correct);

                    //只更新第一個Plate的值
                    if (reagent.ItemType >= 100 && reagent.ItemType < 200)
                    {
                        PlateBase firstPlate = ViewPlates.FirstOrDefault(P => (P.ChineseName == reagent.DisplayName && P.ItemType == reagent.ItemType));
                        if (firstPlate != null)
                        {
                            firstPlate.FirstAddVolume = reagent.FirstAddVolume;
                        }

                    }
                }
                worker.ReportProgress(1, ErrMsg);
                while (WaitFalg)
                    Thread.Sleep(1000);
            };
            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                //to show the last column which contains confirm buttons
                foreach (DataGrid dg in dataGridDictionary.Values)
                {
                    dg.Columns[5].Visibility = Visibility.Visible;
                }

                //incorrect plates and corresponding reagents will shine
                BindRelatedControls();
                UpdatePlates();
                foreach (UIElement uiElement in DeskTopWithGrid.Children)
                {
                    if (uiElement is CarrierBase)
                    {
                        ((CarrierBase)uiElement).ShiningStop();
                        ((CarrierBase)uiElement).Scan();
                    }
                }
                loadFrom.Close();
                btnSave.IsEnabled = reagentAndSupplies.FirstOrDefault(P => P.Correct == false) == null;
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {

            };

            worker.RunWorkerAsync();
            loadFrom.ShowDialog();

            /*******************************************
            //call script and generate scan files
            System.Windows.Input.Cursor currentCurson = this.Cursor;
            this.Cursor = Cursors.Wait;
            //call script and generate scan files
            DateTime fileBeforeCreatedTime = WanTai.Controller.EVO.ProcessorFactory.GetDateTimeNow();
            Services.DeskTopService desktopService = new Services.DeskTopService();
            try
            {
                desktopService.CallScanScript();
            }
            catch (Exception ex)
            {
                MessageBox.Show("扫描错误！");
            }

            //get data from scan files, check plates ,and update scan volume of reagents
            foreach (ReagentAndSuppliesConfiguration reagent in reagentAndSupplies)
            {
                reagent.FirstAddVolume = 0;
            }
            foreach (PlateBase plate in ViewPlates)
            {
                plate.FirstAddVolume = 0;
            }
            DataTable dtScan = GetScanResult(fileBeforeCreatedTime);
            DataTable dtDitiScan = GetDitiScanResult(fileBeforeCreatedTime);
            DataTable dtHeaterScan = GetHeaterScanResult(fileBeforeCreatedTime);

            CheckByScanResult(dtScan);
            CheckDitiByScanResult(dtDitiScan);
            CheckHeaterScanResult(dtHeaterScan);
            GetDetectedLiquid(fileBeforeCreatedTime);

            //if any plate of a reagent is incorrect this reagent is incorrect
            foreach (ReagentAndSuppliesConfiguration reagent in reagentAndSupplies)
            {
                List<PlateBase> plates = ViewPlates.FindAll(P => (P.ChineseName == reagent.DisplayName && P.ItemType == reagent.ItemType));
                reagent.Correct = !plates.Exists(P => !P.Correct);
            }

            //to show the last column which contains confirm buttons
            foreach (DataGrid dg in dataGridDictionary.Values)
            {
                dg.Columns[5].Visibility = Visibility.Visible;
            }

            //incorrect plates and corresponding reagents will shine
            BindRelatedControls();
            UpdatePlates();
            foreach (UIElement uiElement in DeskTopWithGrid.Children)
            {
                if (uiElement is CarrierBase)
                {
                    ((CarrierBase)uiElement).Scan();
                }
            }

            Cursor = currentCurson;
            btnSave.IsEnabled = reagentAndSupplies.FirstOrDefault(P => P.Correct == false) == null;
            ****************************************************************************************************/
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            APIHelper.SetHook();
            
            BindRelatedControls();
            UpdatePlates();
            if (SessionInfo.NextButIndex == 2)
            {
                btnManual.IsEnabled = btnScan.IsEnabled = true;
                btnSupplies.IsEnabled = true;
                btnReagent.IsEnabled = true;
                btnTestItem.IsEnabled = true;
            }
            else
            {
                btnManual.IsEnabled = btnScan.IsEnabled = false;
                btnSupplies.IsEnabled = false;
                btnReagent.IsEnabled = false;
                btnTestItem.IsEnabled = false;
            }
            btnNext.IsEnabled = false;
            if(SessionInfo.NextButIndex==3)
              btnNext.IsEnabled = true;

            if (SessionInfo.NextTurnStep == 1)
                btnScan.IsEnabled = false;
      
        }
        private void btnNeedVolume_Click(object sender, RoutedEventArgs e)
        {
          //  dataGridDictionary.Add("supplies", dgSupplies);
          //  dataGridDictionary.Add("reagent", dgReagent);
            Button btn = sender as Button;
            DataGrid dg =null;
            if (btn.Name == "btnSupplies")
                dg = dataGridDictionary["supplies"];
            if (btn.Name == "btnReagent")
                dg = dataGridDictionary["reagent"];
            if (dg != null)
            {
                foreach (ReagentAndSuppliesConfiguration item in dg.Items)
                {
                    double addedVolume = 0;
                    if (isFirstRotation)
                    {
                        addedVolume = item.NeedVolume;
                    }
                    else if (item.CurrentVolume < (item.TotalNeedValueAndConsumption + item.NeedVolume) && (item.FirstAddVolume + item.CurrentVolume) < (item.TotalNeedValueAndConsumption + item.NeedVolume))
                    {
                        addedVolume = item.TotalNeedValueAndConsumption + item.NeedVolume - item.CurrentVolume;
                    }

                    addedVolume = Math.Ceiling(addedVolume);

                    ((dg.Columns[3] as DataGridTemplateColumn).CellTemplate.FindName("txtAddVolume", dg.Columns[3].GetCellContent(item)) as TextBox).Text = addedVolume.ToString();
                }
            }
            else
            {
                foreach (DataGrid dataGrid in dataGridDictionary.Values)
                {
                    if (dataGrid.Name == "dgSupplies" || dataGrid.Name == "dgReagent") continue;
                    foreach (ReagentAndSuppliesConfiguration item in dataGrid.Items)
                    {
                        double addedVolume = 0;
                        if (isFirstRotation)
                        {
                            addedVolume = item.NeedVolume;
                        }
                        else if (item.CurrentVolume < (item.TotalNeedValueAndConsumption + item.NeedVolume) && (item.FirstAddVolume + item.CurrentVolume) < (item.TotalNeedValueAndConsumption + item.NeedVolume))
                        {
                            addedVolume = item.TotalNeedValueAndConsumption + item.NeedVolume - item.CurrentVolume;
                        }

                        ((dataGrid.Columns[3] as DataGridTemplateColumn).CellTemplate.FindName("txtAddVolume", dataGrid.Columns[3].GetCellContent(item)) as TextBox).Text = addedVolume.ToString();
                    }
                }
            }
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            APIHelper.UnHook();
        }
    }

    public class VisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool visible = (bool)value;
            return visible ? "Hidden" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnabledConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isEnabled = (bool)value;
            return isEnabled ? "False" : "True";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
