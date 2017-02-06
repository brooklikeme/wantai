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
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using WanTai.DataModel;
using WanTai.DataModel.Configuration;
using WanTai.Controller;
using WanTai.Controller.Configuration;
using System.IO;

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for ExperimentsViewDetail.xaml
    /// </summary>
    public partial class ExperimentsViewDetail : Window
    {        
        DataTable dataTable = new DataTable();
        private Guid experimentId;
        private string experimentName;
        private string mixTimes;

        public ExperimentsViewDetail()
        {
            InitializeComponent();
        }

        public Guid ExperimentId
        {
            set { experimentId = value; }
        }

        public string ExperimentName
        {
            set { experimentName = value; }
        }

        public string MixTimes
        {
            set { mixTimes = value; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("Number", typeof(int));
            dataTable.Columns.Add("RotationID", typeof(Guid));
            dataTable.Columns.Add("RotationName", typeof(string));
            dataTable.Columns.Add("OperationName", typeof(string));
            dataTable.Columns.Add("TubesBatchName", typeof(string));
            dataTable.Columns.Add("TubesBatchID", typeof(string));
            dataTable.Columns.Add("State", typeof(string));
            dataTable.Columns.Add("PCRTestResult", typeof(string));
            dataTable.Columns.Add("PCRTestResultExport", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            dataTable.Columns.Add("logTitle", typeof(string)).DefaultValue = "查看日志";
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            UserInfoController userInfoController = new UserInfoController();
            RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);
            ExperimentsInfo experimentInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId);
            if (SessionInfo.LoginName == experimentInfo.LoginName || (userRole != null && userRole.RoleLevel > 1))
            {
                btnImportPCRResult.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                btnImportPCRResult.Visibility = System.Windows.Visibility.Hidden;
            }

            this.ExperimentName_label.Content = experimentName;
            initDataGrid();            
        }

        private void initDataGrid()
        {
            ConfigRotationController rotationController = new ConfigRotationController();
            List<RotationInfo> rotationList = rotationController.GetCurrentRotationInfos(experimentId);
            int startIndex = 1;
            foreach (RotationInfo rotation in rotationList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["Number"] = startIndex;
                dRow["RotationID"] = rotation.RotationID;
                dRow["RotationName"] = rotation.RotationName;
                dRow["OperationName"] = rotation.OperationName;
                dRow["TubesBatchID"] = rotation.TubesBatchID;
                dRow["State"] = rotationController.ConvertEnumStatusToText((RotationInfoStatus)rotation.State);
                dRow["logTitle"] = "查看日志";
                if (rotation.TubesBatchID != null)
                {
                    TubesBatch tubeBatch = rotationController.GetTubesBatchByID((Guid)rotation.TubesBatchID);
                    if (tubeBatch != null)
                    {
                        dRow["TubesBatchName"] = tubeBatch.TubesBatchName;
                    }

                    if (new WanTai.Controller.PCR.PCRTestResultViewListController().CheckRotationHasPCRTestResult(rotation.RotationID, experimentId))
                    {
                        dRow["PCRTestResult"] = "查看";
                        dRow["PCRTestResultExport"] = "导出";
                    }
                }

                if (startIndex % 2 == 0)
                {
                    dRow["Color"] = WindowCustomizer.alternativeColor;
                }
                else
                {
                    dRow["Color"] = WindowCustomizer.defaultColor;
                }

                dataTable.Rows.Add(dRow);
                startIndex++;
            }
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnTubesBatchNameClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            string TubesBatchID = dataTable.Rows[selectedIndex]["TubesBatchID"].ToString();
            TubesDetailView detailView = new TubesDetailView();
            detailView.ViewExperimentBatch(new Guid(TubesBatchID), SessionInfo.BatchTimes);
            detailView.ShowDialog();
        }

        private void OnLogClick(object sender, RoutedEventArgs e) {
            Guid RotatioID =new Guid((sender as TextBlock).Uid);
            string LogContent = LogInfoController.GetRotationLog(RotatioID);

            LogContentView logView = new LogContentView();
            logView.SetLogContent(LogContent);
            logView.ShowDialog();
        }
        private void OnPCRTestResultClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            Guid rotationID = (Guid)dataTable.Rows[selectedIndex]["RotationID"];
            PCR.PCRTestResultHistoryView pcrView = new PCR.PCRTestResultHistoryView();
            pcrView.pCRTestResultDataGridUserControl.RotationId = rotationID;
            pcrView.pCRTestResultDataGridUserControl.RotationName = dataTable.Rows[selectedIndex]["RotationName"].ToString();
            pcrView.pCRTestResultDataGridUserControl.ExperimentId = experimentId;
            pcrView.ShowDialog();            
        }

        private void OnPCRTestResultExportClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            Guid rotationID = (Guid)dataTable.Rows[selectedIndex]["RotationID"];
            string rotationName = dataTable.Rows[selectedIndex]["RotationName"].ToString();

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            //string experimentName = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId).ExperimentName;
            sfd.FileName = experimentName + "_" + DateTime.Now.ToString("yyyyMMdd") + (dataTable.Rows.Count > 1 ? "_" + rotationName : "");
            sfd.Filter = "pdf(*.pdf)|*.pdf|excel(*.xls)|*.xls";
            //System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            //string folderPath = string.Empty;
            //if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    folderPath = folderDialog.SelectedPath;
            //}
            string fileName = string.Empty;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = sfd.FileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    bool result = new WanTai.Controller.PCR.PCRTestResultViewListController().SaveExcelFile(fileName, experimentId, rotationID, rotationName);
                    if (result)
                    {
                        if (System.Windows.Forms.MessageBox.Show("导出文件成功! 是否打开文件?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == (System.Windows.Forms.DialogResult.Yes))
                        {
                            System.Diagnostics.Process.Start(fileName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("导出文件失败：" + ex.Message);
                }
            }
        }

        private void OnRotationNameClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            string RotationID = dataTable.Rows[selectedIndex]["RotationID"].ToString();
            string RotationName = dataTable.Rows[selectedIndex]["RotationName"].ToString();
            RotationOperatesViewDetail rotationOperationView = new RotationOperatesViewDetail();
            rotationOperationView.RotationId = new Guid(RotationID);
            rotationOperationView.RotationName = RotationName;
            rotationOperationView.ShowDialog();
        }

        private void btnReagent_Click(object sender, RoutedEventArgs e)
        {
            ReagentAndSuppliesList reagentAndSupplies = new ReagentAndSuppliesList(experimentId);
            reagentAndSupplies.ShowDialog();
        }

        private void btnImportPCRResult_Click(object sender, RoutedEventArgs e)
        {
            PCR.ImportPCRTestResultFile importPCR = new PCR.ImportPCRTestResultFile(experimentId);
            importPCR.ShowDialog();
            dataTable.Rows.Clear();
            initDataGrid();  
        }

        private void btnExportPCRResult_Click(object sender, RoutedEventArgs e)
        {
            /*
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            //string experimentName = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId).ExperimentName;
            sfd.FileName = experimentName + "_" + DateTime.Now.ToString("yyyyMMdd");
            sfd.Filter = "pdf(*.pdf)|*.pdf|excel(*.xls)|*.xls";
            //System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            //string folderPath = string.Empty;
            //if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    folderPath = folderDialog.SelectedPath;
            //}
            string fileName = string.Empty;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = sfd.FileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    bool result = new WanTai.Controller.PCR.PCRTestResultViewListController().SaveExcelFile(fileName, experimentId);
                    if (result)
                    {
                        if (System.Windows.Forms.MessageBox.Show("导出文件成功! 是否打开文件?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == (System.Windows.Forms.DialogResult.Yes))
                        {
                            System.Diagnostics.Process.Start(fileName);
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("导出文件失败：" + ex.Message);
                }
            }*/
        }

        private void btnAutoImportPCRResult_Click(object sender, RoutedEventArgs e)
        {
            PCR.ImportPCRTestResultFile importPCR = new PCR.ImportPCRTestResultFile(experimentId);
            importPCR.AutoImportPCRResults();
            dataTable.Rows.Clear();
            initDataGrid();
        }
    }
}
