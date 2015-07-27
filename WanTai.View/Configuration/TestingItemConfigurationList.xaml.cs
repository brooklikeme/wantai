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

using WanTai.Controller;
using WanTai.DataModel;

namespace WanTai.View.Configuration
{
    /// <summary>
    /// Interaction logic for TestingItemConfigurationList.xaml
    /// </summary>
    public partial class TestingItemConfigurationList : Window
    {
        TestItemController controller = new TestItemController();
        DataTable dataTable = new DataTable();
        public TestingItemConfigurationList()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("TestingItemID", typeof(Guid));
            dataTable.Columns.Add("TestingItemName", typeof(string));
            dataTable.Columns.Add("TestingItemColor", typeof(string));
            dataTable.Columns.Add("DisplaySequence", typeof(int));
            dataTable.Columns.Add("WorkListFileName", typeof(string));
            dataTable.Columns.Add("Action", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            initDataGrid();
        }

        private void initDataGrid()
        {
            IList<TestingItemConfiguration> recordList = controller.GetTestItemConfigurations();
            dataTable.Rows.Clear();
            foreach (TestingItemConfiguration record in recordList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["TestingItemID"] = record.TestingItemID;
                dRow["TestingItemName"] = record.TestingItemName;
                dRow["TestingItemColor"] = record.TestingItemColor;
                dRow["DisplaySequence"] = record.DisplaySequence;
                dRow["WorkListFileName"] = record.WorkListFileName;

                if (controller.CanDelete(record.TestingItemID))
                {
                    dRow["Action"] = "delete";
                }
                else
                {
                    if (record.ActiveStatus)
                    {
                        dRow["Action"] = "deactive";
                    }
                    else
                    {
                        dRow["Action"] = "active";
                    }
                }

                dataTable.Rows.Add(dRow);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            string TestingItemID = dataTable.Rows[selectedIndex]["TestingItemID"].ToString();

            Button operationButton = (Button)sender;
            if (operationButton.Name.StartsWith("deactive"))
            {
                //使失效
                bool result = controller.UpdateActiveStatus(new Guid(TestingItemID), false);
                if (result)
                {
                    initDataGrid();
                }
            }
            else if (operationButton.Name.StartsWith("active"))
            {
                //使生效
                bool result = controller.UpdateActiveStatus(new Guid(TestingItemID), true);
                if (result)
                {
                    initDataGrid();
                }
            }
            else if (operationButton.Name.StartsWith("delete"))
            {
                MessageBoxResult selectResult = MessageBox.Show("确定删除么？", "系统提示", MessageBoxButton.YesNo);
                if (selectResult == MessageBoxResult.Yes)
                {

                    bool result = controller.Delete(new Guid(TestingItemID));
                    WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除检测项目：" + dataTable.Rows[selectedIndex]["TestingItemName"] + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
                    if (result)
                    {
                        dataTable.Rows[selectedIndex].Delete();
                        MessageBox.Show("删除成功", "系统提示");
                    }
                    else
                    {
                        MessageBox.Show("删除失败", "系统提示");
                    }
                }
            }
        }

        private void new_button_Click(object sender, RoutedEventArgs e)
        {
            CreateTestingItemConfiguration configuration = new CreateTestingItemConfiguration();
            bool dialogResult = (bool)configuration.ShowDialog();
            if (dialogResult)
            {
                initDataGrid();
            }
        }

        private void edit_button_Click(object sender, RoutedEventArgs e)
        {
            int selectIndex = this.dataGrid_view.SelectedIndex;
            System.Data.DataRow dRow = dataTable.Rows[selectIndex];
            string TestingItemID = dRow["TestingItemID"].ToString();
            CreateTestingItemConfiguration configuration = new CreateTestingItemConfiguration();
            configuration.SetEditedTestItemId(TestingItemID);
            bool dialogResult = (bool)configuration.ShowDialog();
            if (dialogResult)
            {
                initDataGrid();
            }
        }        
    }
}
