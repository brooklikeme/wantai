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
using System.Xml;

using WanTai.DataModel.Configuration;
using WanTai.Controller.PCR;
using WanTai.Controller.Configuration;
using WanTai.DataModel;

namespace WanTai.View.PCR
{
    /// <summary>
    /// Interaction logic for PCRTestResultDataGridUserControl.xaml
    /// </summary>
    public partial class PCRTestResultDataGridUserControl : UserControl
    {
        DataTable dataTable = new DataTable();
        private bool nCoV = false;
        private Guid rotationId;
        private string rotationName;
        private Guid experimentId;
        System.Collections.Generic.Dictionary<int, string> liquidTypeDictionary = new System.Collections.Generic.Dictionary<int, string>();

        public bool ncov
        {
            set { nCoV = value; }
            get { return nCoV; }
        }

        public PCRTestResultDataGridUserControl()
        {
            InitializeComponent();
        }

        public Guid RotationId
        {
            set { rotationId = value; }
            get { return rotationId; }
        }

        public string RotationName
        {
            set { rotationName = value; }
            get { return rotationName; }
        }

        public Guid ExperimentId
        {
            set { experimentId = value; }
            get { return experimentId; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // init column width
            string PCRTestResultWidths = WanTai.Common.Configuration.GetPCRTestResultWidthList();
            if (!string.IsNullOrEmpty(PCRTestResultWidths)) {
                float[] widths = Array.ConvertAll(PCRTestResultWidths.Split(','), new Converter<string, float>(float.Parse));
                if (widths.Length > 15) {
                    double rate = 1145.0 / widths.Sum();
                    this.Number.Width = widths[0] * rate;
                    this.TubeTypeName.Width = widths[1] * rate;
                    this.PCRName.Width = widths[2] * rate;
                    this.TubeBarCode.Width = widths[3] * rate;
                    this.TubePosition.Width = widths[4] * rate;
                    this.PoolingRuleName.Width = widths[5] * rate;
                    this.TestingItemName.Width = widths[6] * rate;
                    this.PCRPosition.Width = widths[7] * rate;
                    this.HBV.Width = widths[8] * rate;
                    this.HBVIC.Width = widths[9] * rate;
                    this.HCV.Width = widths[10] * rate;
                    this.HCVIC.Width = widths[11] * rate;
                    this.HIV.Width = widths[12] * rate;
                    this.HIVIC.Width = widths[13] * rate;
                    this.PCRTestResult.Width = widths[14] * rate;
                    this.SimpleTrackingResult.Width = widths[15] * rate;
                }
            }


            dataTable.Columns.Add("Number", typeof(int));
            dataTable.Columns.Add("PCRName", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            dataTable.Columns.Add("TubeID", typeof(Guid));
            dataTable.Columns.Add("TubeBarCode", typeof(string));
            dataTable.Columns.Add("TubePosition", typeof(string));
            dataTable.Columns.Add("TubeType", typeof(int));
            dataTable.Columns.Add("TubeTypeName", typeof(string));
            dataTable.Columns.Add("TubeTypeColor", typeof(string));
            dataTable.Columns.Add("TubeTypeColorVisible", typeof(string));
            dataTable.Columns.Add("PoolingRuleName", typeof(string));
            dataTable.Columns.Add("TestingItemName", typeof(string));
            dataTable.Columns.Add("PCRPlateBarCode", typeof(string));
            dataTable.Columns.Add("PCRPosition", typeof(string));
            dataTable.Columns.Add("HBV", typeof(string));
            dataTable.Columns.Add("HBVIC", typeof(string));
            dataTable.Columns.Add("HCV", typeof(string));
            dataTable.Columns.Add("HCVIC", typeof(string));
            dataTable.Columns.Add("HIV", typeof(string));
            dataTable.Columns.Add("HIVIC", typeof(string));
            dataTable.Columns.Add("PCRTestItemID", typeof(Guid));
            dataTable.Columns.Add("PCRTestResult", typeof(string));
            dataTable.Columns.Add("PCRTestContent", typeof(string));
            dataTable.Columns.Add("SimpleTrackingResult", typeof(string));            
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            List<LiquidType> LiquidTypeList = SessionInfo.LiquidTypeList = WanTai.Common.Configuration.GetLiquidTypes();
            foreach (LiquidType liquidType in LiquidTypeList)
            {
                liquidTypeDictionary.Add(liquidType.TypeId, liquidType.Color);
                if (liquidType.TypeId == (int)Tubetype.PositiveControl || liquidType.TypeId == (int)Tubetype.NegativeControl)
                {
                    StackPanel_contrast.Children.Add(new Ellipse() { Width = 15, Height = 15, Margin = new Thickness(15, 0, 0, 0), Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(liquidType.Color)) });
                    StackPanel_contrast.Children.Add(new Label() { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Content = liquidType.TypeName });
                }
            }

            loadDataGrid();
        }

        private void loadDataGrid()
        {
            string errorMessage = string.Empty;
            string reagent_batch = "";
            string qc_batch = "";
            bool has_bci = false;
            dataTable.Rows.Clear();
            if (rotationId != null && rotationId != Guid.Empty)
            {                
                PCRTestResultViewListController controller = new PCRTestResultViewListController();
                controller.QueryTubesPCRTestResult(experimentId, rotationId, dataTable, liquidTypeDictionary, WindowCustomizer.redColor, WindowCustomizer.greenColor, nCoV, out errorMessage, out reagent_batch, out qc_batch, out has_bci);
                if (nCoV)
                {
                    HBVIC.Visibility = Visibility.Hidden;
                    HCVIC.Visibility = Visibility.Hidden;
                    HIVIC.Visibility = Visibility.Hidden;
                    HBV.Header = "ORF1ab(Ct)";
                    HCV.Header = "N(Ct)";
                    HIV.Header = "IC(Ct)";
                }
                else
                {
                    HBVIC.Visibility = has_bci ? Visibility.Hidden : Visibility.Visible;
                    HCVIC.Visibility = has_bci ? Visibility.Hidden : Visibility.Visible;
                    HIVIC.Header = has_bci ? "IC(Ct)" : "HIVIC(Ct)";
                }
                ExperimentsInfo expInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId);
                this.experiment_name.Content = expInfo.ExperimentName;
                this.login_name.Content = expInfo.LoginName;
                this.sample_number.Content = controller.GetSampleNumber(expInfo.ExperimentID, rotationId);
                this.experiment_time.Content = expInfo.StartTime.ToString("yyyy/MM/dd HH:mm:ss") + "--" + Convert.ToDateTime(expInfo.EndTime).ToString("yyyy/MM/dd HH:mm:ss");
                this.instrument_type.Content = SessionInfo.GetSystemConfiguration("InstrumentType");
                this.instrument_number.Content = SessionInfo.GetSystemConfiguration("InstrumentNumber");
                string PCRTimeString = "";
                string PCRDeviceString = "";
                string PCRBarCodeString = "";

                List<Plate> plateList = controller.GetPCRPlateList(rotationId, experimentId);
                if (plateList != null && plateList.Count > 0)
                {
                    foreach (Plate plate in plateList)
                    {
                        PCRBarCodeString += PCRBarCodeString == "" ? plate.BarCode : " " + plate.BarCode;
                        XmlDocument xdoc = new XmlDocument();
                        if (null != plate.PCRContent)
                        {
                            xdoc.LoadXml(plate.PCRContent);
                            XmlNode node = xdoc.SelectSingleNode("PCRContent");
                            PCRDeviceString += PCRDeviceString == "" ? node.SelectSingleNode("PCRDevice").InnerText : " " + node.SelectSingleNode("PCRDevice").InnerText;
                            DateTime pcrStartTime, pcrEndTime;
                            string pcrStartTimeString = node.SelectSingleNode("PCRStartTime").InnerText;
                            string pcrEndTimeString = node.SelectSingleNode("PCREndTime").InnerText;
                            int spaceIndex = pcrStartTimeString.IndexOf(' ', pcrStartTimeString.IndexOf(' ') + 1);
                            if (spaceIndex > 0)
                            {
                                pcrStartTimeString = pcrStartTimeString.Substring(0, spaceIndex);
                            }
                            spaceIndex = pcrEndTimeString.IndexOf(' ', pcrEndTimeString.IndexOf(' ') + 1);
                            if (spaceIndex > 0)
                            {
                                pcrEndTimeString = pcrEndTimeString.Substring(0, spaceIndex);
                            }
                            if (DateTime.TryParse(pcrStartTimeString, out pcrStartTime))
                            {
                                pcrStartTimeString = pcrStartTime.ToString("yyyy/MM/dd HH:mm:ss");
                            }
                            if (DateTime.TryParse(pcrEndTimeString, out pcrEndTime))
                            {
                                pcrEndTimeString = pcrEndTime.ToString("yyyy/MM/dd HH:mm:ss");
                            } 
                            string timeString = pcrStartTimeString + "--" + pcrEndTimeString;
                            PCRTimeString += PCRTimeString == "" ? timeString : "\n " + timeString;
                        }
                    }
                }
                this.rotation.Content = rotationName;
                this.pcr_time.Content = PCRTimeString;
                this.pcr_device.Content = PCRDeviceString;
                this.pcr_barcode.Content = PCRBarCodeString;
                this.qc_batch.Content = qc_batch;
                this.reagent_batch.Content = reagent_batch;
              
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    this.qc_result.Content = errorMessage;
                }
            }
        }

        private void dataGrid_view_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {/*
            DataGridCellInfo Cell = ((System.Windows.Controls.DataGrid)(e.Source)).CurrentCell;
            if (Cell.Column.Header.ToString() == "检测结果")
            {
                if (dataTable.Rows[dataGrid_view.SelectedIndex]["PCRTestItemID"] != null && dataTable.Rows[dataGrid_view.SelectedIndex]["PCRTestItemID"] != DBNull.Value)
                {
                    UserInfoController userInfoController = new UserInfoController();
                    RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);
                    ExperimentsInfo experimentInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId);
                    if (SessionInfo.LoginName == experimentInfo.LoginName || (userRole != null && userRole.RoleLevel > 1))
                    {
                        EditPCRTestResult editPCRResult = new EditPCRTestResult();
                        editPCRResult.SetCurrentData(dataTable.Rows[dataGrid_view.SelectedIndex]);
                        editPCRResult.ExperimentID = experimentId;
                        bool dialogResult = (bool)editPCRResult.ShowDialog();
                        if (dialogResult)
                        {
                            loadDataGrid();
                        }
                    }
                    else
                    {
                        ViewPCRTestResult viewPCRResult = new ViewPCRTestResult();
                        viewPCRResult.SetCurrentData(dataTable.Rows[dataGrid_view.SelectedIndex]);
                        viewPCRResult.ShowDialog();
                    }
                }
            }
            else
            {
                return;
            }
          */
        }

        private void dataGrid_view_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            loadDataGrid();
        }

        private void barcodequery_button_Click(object sender, RoutedEventArgs e)
        {           
            /*
            if (!string.IsNullOrEmpty(tube_barcode.Text))
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    row["Color"] = Colors.White;
                }

                DataRow[] foundRows = dataTable.Select("TubeBarCode = '" + tube_barcode.Text.Trim() + "'", "Number");
                if (foundRows != null && foundRows.Count() > 0)
                {
                    int index = 0;
                    foreach (DataRow row in foundRows)
                    {
                        if (index == 0)
                            dataGrid_view.ScrollIntoView(dataGrid_view.Items[(int)row["Number"]-1]); 
                        row["Color"] = Colors.Orange;
                        index++;
                    }
                }                
            }*/
        }
    }
}
