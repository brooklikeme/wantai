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
    /// Interaction logic for PoolingRulesConfigurationList.xaml
    /// </summary>
    public partial class PoolingRulesConfigurationList : Window
    {
        PoolingRulesConfigurationController controller = new PoolingRulesConfigurationController();
        DataTable dataTable = new DataTable();
        public PoolingRulesConfigurationList()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("PoolingRulesID", typeof(Guid));
            dataTable.Columns.Add("PoolingRulesName", typeof(string));
            dataTable.Columns.Add("TubeNumber", typeof(int));
            dataTable.Columns.Add("Action", typeof(string));
            dataTable.Columns.Add("GroupColor", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            initDataGrid();
        }

        private void initDataGrid()
        {
            IList<PoolingRulesConfiguration> recordList = controller.GetPoolingRulesConfigurations();
            dataTable.Rows.Clear();
            foreach (PoolingRulesConfiguration record in recordList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["PoolingRulesID"] = record.PoolingRulesID;
                dRow["PoolingRulesName"] = record.PoolingRulesName;
                dRow["TubeNumber"] = record.TubeNumber;
                dRow["GroupColor"] = record.GroupColor;

                if (controller.CanDelete(record.PoolingRulesID))
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
            string PoolingRulesID = dataTable.Rows[selectedIndex]["PoolingRulesID"].ToString();

            Button operationButton = (Button)sender;
            if (operationButton.Name.StartsWith("deactive"))
            {
                //使失效
                bool result = controller.UpdatePoolingRuleActiveStatus(new Guid(PoolingRulesID), false);
                if (result)
                {
                    initDataGrid();
                }
            }
            else if (operationButton.Name.StartsWith("active"))
            {
                //使生效
                bool result = controller.UpdatePoolingRuleActiveStatus(new Guid(PoolingRulesID), true);
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
                    bool result = controller.Delete(new Guid(PoolingRulesID));
                    WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除混样方式：" + dataTable.Rows[selectedIndex]["PoolingRulesName"] + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
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
            CreatePoolingRulesConfiguration configuration = new CreatePoolingRulesConfiguration();
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
            string PoolingRulesID = dRow["PoolingRulesID"].ToString();
            CreatePoolingRulesConfiguration configuration = new CreatePoolingRulesConfiguration();
            configuration.SetEditedPoolingRuleId(PoolingRulesID);
            bool dialogResult = (bool)configuration.ShowDialog();
            if (dialogResult)
            {
                initDataGrid();
            }
        }        
    }
}
