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
        private Guid rotationId;
        private Guid experimentId;
        System.Collections.Generic.Dictionary<int, string> liquidTypeDictionary = new System.Collections.Generic.Dictionary<int, string>();        

        public PCRTestResultDataGridUserControl()
        {
            InitializeComponent();
        }

        public Guid RotationId
        {
            set { rotationId = value; }
            get { return rotationId; }
        }

        public Guid ExperimentId
        {
            set { experimentId = value; }
            get { return experimentId; }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("Number", typeof(int));
            dataTable.Columns.Add("Color", typeof(string));
            dataTable.Columns.Add("TubeID", typeof(Guid));
            dataTable.Columns.Add("TubeBarCode", typeof(string));
            dataTable.Columns.Add("TubePosition", typeof(string));
            dataTable.Columns.Add("TubeType", typeof(int));
            dataTable.Columns.Add("TubeTypeColor", typeof(string));
            dataTable.Columns.Add("TubeTypeColorVisible", typeof(string));
            dataTable.Columns.Add("PoolingRuleName", typeof(string));
            dataTable.Columns.Add("TestingItemName", typeof(string));
            dataTable.Columns.Add("PCRPlateBarCode", typeof(string));
            dataTable.Columns.Add("PCRPosition", typeof(int));
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
            dataTable.Rows.Clear();
            if (rotationId != null && rotationId != Guid.Empty)
            {                
                PCRTestResultViewListController controller = new PCRTestResultViewListController();
                controller.QueryTubesPCRTestResult(experimentId, rotationId, dataTable, liquidTypeDictionary, WindowCustomizer.redColor, WindowCustomizer.greenColor, out errorMessage);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    this.errorMessage_label.Content = errorMessage;
                }
            }
        }

        private void dataGrid_view_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
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
        }

        private void dataGrid_view_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {            
            loadDataGrid();
        }

        private void barcodequery_button_Click(object sender, RoutedEventArgs e)
        {            
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
            }
        }
    }
}
