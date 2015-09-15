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
using WanTai.Controller.Configuration;

namespace WanTai.View.PCR
{
    /// <summary>
    /// Interaction logic for PCRTestResultHistoryView.xaml
    /// </summary>
    public partial class PCRTestResultHistoryView : Window
    {
        public PCRTestResultHistoryView()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UserInfoController userInfoController = new UserInfoController();
            RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);
            ExperimentsInfo experimentInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(pCRTestResultDataGridUserControl.ExperimentId);
            if (SessionInfo.LoginName == experimentInfo.LoginName || (userRole != null && userRole.RoleLevel > 1))
            {
                deleteRotationPCRTest_button.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                deleteRotationPCRTest_button.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void deleteRotationPCRTest_button_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult selectResult = MessageBox.Show("确定删除此轮次下导入的PCR检测结果么？", "系统提示", MessageBoxButton.YesNo);
            if (selectResult == MessageBoxResult.Yes)
            {
                Guid rotationId = pCRTestResultDataGridUserControl.RotationId;
                bool result = new PCRTestResultViewListController().DeleteRotationPCRTestResult(rotationId);

                //WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "O"PCR?9~S" + " " + (result == true ? "?`x" : "??"), SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
                if (result)
                {                    
                    pCRTestResultDataGridUserControl.RotationId = rotationId;
                    pCRTestResultDataGridUserControl.qc_result.Content = string.Empty;
                    pCRTestResultDataGridUserControl.dataGrid_view.DataContext = DateTime.Now;
                }
                else
                {
                    MessageBox.Show("删除失败");
                }
            }
        } 
    }
}
