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

using WanTai.DataModel.Configuration;
using WanTai.Controller.PCR;
using WanTai.DataModel;

namespace WanTai.View.PCR
{
    /// <summary>
    /// Interaction logic for PCRTestResultViewList.xaml
    /// </summary>
    public partial class PCRTestResultViewList : Window
    {
        PCRTestResultViewListController controller = new PCRTestResultViewListController();        
        public PCRTestResultViewList()
        {           
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<RotationInfo> rotationList = controller.GetPCRResultRotation();
            foreach (RotationInfo info in rotationList)
            {
                rotation_comboBox.Items.Add(new ComboBoxItem() { Content = info.RotationName, Tag = info.RotationID });
            } 
        }        

        private void rotation_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)rotation_comboBox.SelectedItem;
            Guid rotationId = (Guid)selectedItem.Tag;
            pCRTestResultDataGridUserControl.RotationId = rotationId;
            pCRTestResultDataGridUserControl.ExperimentId = SessionInfo.ExperimentID;
            pCRTestResultDataGridUserControl.dataGrid_view.DataContext = DateTime.Now;
            reloadRotationDeleteButton();
        }        

        private void reloadRotationDeleteButton()
        {
            ComboBoxItem selectedItem = (ComboBoxItem)rotation_comboBox.SelectedItem;
            Guid rotationId = (Guid)selectedItem.Tag;
            bool result = controller.CheckRotationHasPCRTestResult(rotationId,SessionInfo.ExperimentID);

            if (result)
            {
                deleteRotationPCRTest_button.Visibility = Visibility.Visible;
            }
            else
            {
                deleteRotationPCRTest_button.Visibility = Visibility.Hidden;
            }
        }       

        private void deleteRotationPCRTest_button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult selectResult = MessageBox.Show("确定删除此轮次下导入的PCR检测结果么？", "系统提示", MessageBoxButton.YesNo);
            if (selectResult == MessageBoxResult.Yes)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)rotation_comboBox.SelectedItem;
                Guid rotationId = (Guid)selectedItem.Tag;
                bool result = controller.DeleteRotationPCRTestResult(rotationId);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除PCR检测结果" + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
                if (result)
                {
                    selectedItem = (ComboBoxItem)rotation_comboBox.SelectedItem;
                    rotationId = (Guid)selectedItem.Tag;
                    pCRTestResultDataGridUserControl.qc_result.Content = string.Empty;
                    pCRTestResultDataGridUserControl.RotationId = rotationId;
                    pCRTestResultDataGridUserControl.ExperimentId = SessionInfo.ExperimentID;
                    pCRTestResultDataGridUserControl.dataGrid_view.DataContext = DateTime.Now;
                    
                    reloadRotationDeleteButton();
                }
                else
                {
                    MessageBox.Show("删除失败!", "系统提示");
                }
            }
        }

        private void exportPCRTest_button_Click(object sender, RoutedEventArgs e)
        {
            /*
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.FileName = SessionInfo.CurrentExperimentsInfo.ExperimentName + "_" + DateTime.Now.ToString("yyyyMMdd");
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
                    bool result = new WanTai.Controller.PCR.PCRTestResultViewListController().SaveExcelFile(fileName, SessionInfo.ExperimentID);
                    if (result)
                        MessageBox.Show("导出文件成功!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出文件失败：" + ex.Message);
                }
            }*/
        }        
    }
}
