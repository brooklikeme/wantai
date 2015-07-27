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

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for PCRBarcodeQueryResult.xaml
    /// </summary>
    public partial class PCRBarcodeQueryResult : Window
    {
        DataTable dataTable = new DataTable();
        private string tubeBarcode;
        System.Collections.Generic.Dictionary<int, string> liquidTypeDictionary = new System.Collections.Generic.Dictionary<int, string>();

        public PCRBarcodeQueryResult()
        {
            InitializeComponent();

            dataTable.Columns.Add("Number", typeof(int));
            dataTable.Columns.Add("TubeID", typeof(Guid));
            dataTable.Columns.Add("TubeBarCode", typeof(string));
            dataTable.Columns.Add("TubePosition", typeof(string));
            dataTable.Columns.Add("TubeType", typeof(int));
            dataTable.Columns.Add("TubeTypeColor", typeof(string));
            dataTable.Columns.Add("TubeTypeColorVisible", typeof(string));
            dataTable.Columns.Add("PoolingRuleName", typeof(string));
            dataTable.Columns.Add("TestingItemName", typeof(string));
            dataTable.Columns.Add("PCRTestItemID", typeof(Guid));
            dataTable.Columns.Add("PCRTestResult", typeof(string));
            dataTable.Columns.Add("PCRTestContent", typeof(string));
            dataTable.Columns.Add("ExperimentID", typeof(Guid));
            dataTable.Columns.Add("ExperimentName", typeof(string));
            dataTable.Columns.Add("StartTime", typeof(string));
            dataTable.Columns.Add("EndTime", typeof(string));
            dataTable.Columns.Add("LoginName", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;            
        }

        public string TubeBarcode
        {
            set { tubeBarcode = value; }
            get { return tubeBarcode; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

            barcode.Content = tubeBarcode.Replace("_","__");
            loadDataGrid();
        }

        private void loadDataGrid()
        {            
            dataTable.Rows.Clear();
            if (!string.IsNullOrEmpty(tubeBarcode))
            {                
                PCRTestResultViewListController controller = new PCRTestResultViewListController();
                controller.QueryTubesPCRTestResultByBarcode(tubeBarcode, dataTable, liquidTypeDictionary); 
            }
        }

        private void dataGrid_view_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCellInfo Cell = ((System.Windows.Controls.DataGrid)(e.Source)).CurrentCell;
            if (Cell.Column!=null && Cell.Column.Header.ToString() == "检测结果")
            {
                if (dataTable.Rows[dataGrid_view.SelectedIndex]["PCRTestItemID"] != null && dataTable.Rows[dataGrid_view.SelectedIndex]["PCRTestItemID"] != DBNull.Value)
                {
                    Guid experimentId = (Guid)dataTable.Rows[dataGrid_view.SelectedIndex]["ExperimentID"];
                    UserInfoController userInfoController = new UserInfoController();
                    RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);
                    ExperimentsInfo experimentInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId);
                    if (SessionInfo.LoginName == experimentInfo.LoginName || (userRole != null && userRole.RoleLevel > 1))
                    {
                        PCR.EditPCRTestResult editPCRResult = new PCR.EditPCRTestResult();
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
                        PCR.ViewPCRTestResult viewPCRResult = new PCR.ViewPCRTestResult();
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

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        } 
    }
}
