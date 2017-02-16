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
using WanTai.Controller.Configuration;
using WanTai.DataModel;

namespace WanTai.View.Configuration
{
    /// <summary>
    /// Interaction logic for ReportConfiguration.xaml
    /// </summary>
    public partial class ReportConfigurationList : Window
    {
        ReportConfigurationController controller = new ReportConfigurationController();
        DataTable dataTable = new DataTable();
        public ReportConfigurationList()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {                      
            dataTable.Columns.Add("ItemID", typeof(string));
            dataTable.Columns.Add("DisplayName", typeof(string));
            dataTable.Columns.Add("Position", typeof(int));
            dataTable.Columns.Add("CalculationFormula", typeof(string));
            dataTable.Columns.Add("Action", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            initDataGrid();
        }

        private void initDataGrid()
        {
            List<ReportConfiguration> recordList = controller.GetAll();
            dataTable.Rows.Clear();
            foreach (ReportConfiguration record in recordList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["ItemID"] = record.ItemID.ToString();
                dRow["DisplayName"] = record.DisplayName;
                dRow["Position"] = record.Position.ToString();
                dRow["CalculationFormula"] = record.CalculationFormula;

                if (!record.ActiveStatus)
                {
                    dRow["Action"] = "active";
                }
                else
                {
                    if (true/*controller.CanDelete(record.ItemID)*/)
                    {
                        dRow["Action"] = "delete";
                    }
                    else
                    {
                        dRow["Action"] = "deactive";
                    }
                }

                dataTable.Rows.Add(dRow);
            }
        }

 
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            string ItemID = dataTable.Rows[selectedIndex]["ItemID"].ToString();

            Button operationButton = (Button)sender;
            if (operationButton.Name.StartsWith("deactive"))
            {
                //使失效
                bool result = controller.UpdateActiveStatus(new Guid(ItemID), false);
                if (result)
                {
                    initDataGrid();
                }
            }
            else if (operationButton.Name.StartsWith("active"))
            {
                //使生效
                bool result = controller.UpdateActiveStatus(new Guid(ItemID), true);
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
                    bool result = controller.Delete(new Guid(ItemID));
                    WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除试剂耗材查询配置：" + dataTable.Rows[selectedIndex]["DisplayName"] + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
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
            ReportConfigurationDlg configurationDlg = new ReportConfigurationDlg();
            configurationDlg.ShowDialog();
            initDataGrid(); 
        }

        private void edit_button_Click(object sender, RoutedEventArgs e)
        {
            int selectIndex = this.dataGrid_view.SelectedIndex;
            System.Data.DataRow dRow = dataTable.Rows[selectIndex];
            string itemId = dRow["ItemID"].ToString();
            ReportConfigurationDlg configurationDlg = new ReportConfigurationDlg();
            configurationDlg.SetEditedItemId(itemId);
            configurationDlg.ShowDialog();
            initDataGrid(); 
        }        
    }
}
