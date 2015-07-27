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

using WanTai.Controller.Configuration;
using WanTai.DataModel;

namespace WanTai.View.Configuration
{
    /// <summary>
    /// Interaction logic for OperationConfigurationList.xaml
    /// </summary>
    public partial class OperationConfigurationList : Window
    {
        OperationConfigurationController controller = new OperationConfigurationController();
        DataTable dataTable = new DataTable();
        public OperationConfigurationList()
        {
            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("OperationID", typeof(string));
            dataTable.Columns.Add("OperationName", typeof(string));
            dataTable.Columns.Add("OperationType", typeof(string));
            dataTable.Columns.Add("ScriptFileName", typeof(string));
            dataTable.Columns.Add("OperationSequence", typeof(string));
            dataTable.Columns.Add("DisplayFlag", typeof(string));
            dataTable.Columns.Add("RunTime", typeof(int));
            dataTable.Columns.Add("SubOperations", typeof(string));
            dataTable.Columns.Add("Action", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            initDataGrid();
        }

        private void initDataGrid()
        {
            List<OperationConfiguration> recordList = controller.GetAllOperations();
            dataTable.Rows.Clear();
            foreach (OperationConfiguration record in recordList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["OperationID"] = record.OperationID;
                dRow["OperationName"] = record.OperationName;
                dRow["OperationType"] = record.OperationType == (short)OperationType.Single ? CreateOperationConfiguration.OperationTypeName[0] : CreateOperationConfiguration.OperationTypeName[1];
                dRow["ScriptFileName"] = record.ScriptFileName;
                dRow["OperationSequence"] = record.OperationSequence;
                dRow["DisplayFlag"] = (bool)record.DisplayFlag ? "是" : "否";
                if (record.RunTime !=null)
                {
                    dRow["RunTime"] = record.RunTime;
                }

                if (controller.CanDelete(record.OperationID))
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

                string subOperationIDs = record.SubOperationIDs;
                if (!string.IsNullOrEmpty(subOperationIDs))
                {
                    string subOperationNames = string.Empty;
                    string[] subOperationID_s = subOperationIDs.Split(',');
                    foreach (string id in subOperationID_s)
                    {
                        string name = controller.GetOperationName(new Guid(id));
                        if (!string.IsNullOrEmpty(name))
                        {
                            if(string.IsNullOrEmpty(subOperationNames))
                            {
                                subOperationNames = name;
                            }
                            else
                            {
                                subOperationNames = subOperationNames + "," + name;
                            }
                        }
                    }

                    dRow["SubOperations"] = subOperationNames;
                }
                
                dataTable.Rows.Add(dRow);
            }
        }       

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            string OperationID = dataTable.Rows[selectedIndex]["OperationID"].ToString();

            Button operationButton = (Button)sender;
            if (operationButton.Name.StartsWith("deactive"))
            {
                //使失效
                bool result = controller.UpdateActiveStatus(new Guid(OperationID), false);
                if (result)
                {
                    initDataGrid();
                }
            }
            else if (operationButton.Name.StartsWith("active"))
            {
                //使生效
                bool result = controller.UpdateActiveStatus(new Guid(OperationID), true);
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
                    bool result = controller.IsOperationCanBeDeleted(OperationID);
                    if (result)
                    {
                        result = controller.Delete(new Guid(OperationID));
                        WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除操作：" + dataTable.Rows[selectedIndex]["OperationName"] + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);

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
                    else
                    {
                        MessageBox.Show("此操作是其他操作的子操作，不能被删除", "系统提示");
                    }
                }
            }
        }

        private void new_button_Click(object sender, RoutedEventArgs e)
        {
            CreateOperationConfiguration configuration = new CreateOperationConfiguration();
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
            string operationID = dRow["OperationID"].ToString();
            CreateOperationConfiguration configuration = new CreateOperationConfiguration();
            configuration.SetEditedOperationId(operationID);
            bool dialogResult = (bool)configuration.ShowDialog();
            if (dialogResult)
            {
                initDataGrid();
            }
        }        
    }
}
