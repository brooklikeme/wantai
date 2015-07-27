using System;
using System.Collections.Generic;
using System.Collections;
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
using System.IO;
using System.Data.OleDb;
using System.Data;

using WanTai.DataModel;
using WanTai.Controller.PCR;

namespace WanTai.View.PCR
{
    /// <summary>
    /// Interaction logic for EditPCRTestResult.xaml
    /// </summary>
    public partial class EditPCRTestResult : Window
    {
        private DataRow currentRowData;
        private Guid experimentId;
        EditPCRTestResultController controller = new EditPCRTestResultController();

        public EditPCRTestResult()
        {
            InitializeComponent();            
        }

        public void SetCurrentData(DataRow rowData)
        {
            currentRowData = rowData;
        }

        public Guid ExperimentID
        {
            set { experimentId = value; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Guid itemId = (Guid)currentRowData["PCRTestItemID"];
            label_TubeBarcode.Content = (string)currentRowData["TubeBarCode"];
            TextBlock_TubePosition.Text = (string)currentRowData["TubePosition"];
            label_TestingItemName.Content = (string)currentRowData["TestingItemName"];
            TextBlock_PCRTestContent.Text = controller.QueryPCRTestResult(itemId);
            textbox_PCRTestResult.Text = (string)currentRowData["PCRTestResult"];
        }        

        private void save_Click(object sender, RoutedEventArgs e)
        {
            Guid itemId = (Guid)currentRowData["PCRTestItemID"];
            string result = textbox_PCRTestResult.Text;
            bool updateResult = controller.UpdatePCRTestResult(itemId, result);
            WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改PCR检测结果：" + result + " " + (updateResult == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), experimentId);
            if (updateResult)
            {
                MessageBox.Show("保存成功", "系统提示");
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("保存失败", "系统提示");
            }

            this.Close();
        } 

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }        
    }    
}
