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
    /// Interaction logic for ReagentSuppliesConfiguration.xaml
    /// </summary>
    public partial class ReagentSuppliesConfigurationList : Window
    {
        ReagentSuppliesConfigurationController controller = new ReagentSuppliesConfigurationController();
        DataTable dataTable = new DataTable();
        public ReagentSuppliesConfigurationList()
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
            dataTable.Columns.Add("EnglishName", typeof(string));
            dataTable.Columns.Add("DisplayName", typeof(string));
            dataTable.Columns.Add("TypeName", typeof(string));
            dataTable.Columns.Add("BarcodePrefix", typeof(string));
            dataTable.Columns.Add("ContainerName", typeof(string));
            dataTable.Columns.Add("Grid", typeof(int));
            dataTable.Columns.Add("Position", typeof(int));
            dataTable.Columns.Add("CalculationFormula", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            dataTable.Columns.Add("Unit", typeof(string));
            dataTable.Columns.Add("Action", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            initDataGrid();
        }

        private void initDataGrid()
        {
            List<ReagentAndSuppliesConfiguration> recordList = controller.GetAll();
            dataTable.Rows.Clear();
            foreach (ReagentAndSuppliesConfiguration record in recordList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["ItemID"] = record.ItemID.ToString();
                dRow["EnglishName"] = record.EnglishName;
                dRow["DisplayName"] = record.DisplayName;
                dRow["TypeName"] = getTypeName((short)record.ItemType);
                dRow["BarcodePrefix"] = record.BarcodePrefix;
                dRow["ContainerName"] = record.ContainerName;
                dRow["Grid"] = record.Grid.ToString();
                dRow["Position"] = record.Position.ToString();
                dRow["CalculationFormula"] = record.CalculationFormula;
                dRow["Color"] = record.Color;
                dRow["Unit"] = record.Unit;

                if (!record.ActiveStatus)
                {
                    dRow["Action"] = "active";
                }
                else
                {
                    if (controller.CanDelete(record.ItemID))
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

        private string getTypeName(short typeId)
        {
            List<ReagentSuppliesType> typeList = WanTai.Common.Configuration.GetReagentSuppliesTypes();
            if (typeList != null)
            {
                foreach (ReagentSuppliesType type in typeList)
                {
                    if(type.TypeId == typeId.ToString())
                        return type.TypeName;
                }
            }

            return null;
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
                    WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除试剂耗材：" + dataTable.Rows[selectedIndex]["EnglishName"] + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
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
            ReagentSuppliesConfiguration configuration = new ReagentSuppliesConfiguration();
            configuration.ShowDialog();
            initDataGrid(); 
        }

        private void edit_button_Click(object sender, RoutedEventArgs e)
        {
            int selectIndex = this.dataGrid_view.SelectedIndex;
            System.Data.DataRow dRow = dataTable.Rows[selectIndex];
            string itemId = dRow["ItemID"].ToString();
            ReagentSuppliesConfiguration configuration = new ReagentSuppliesConfiguration();
            configuration.SetEditedItemId(itemId);
            configuration.ShowDialog();
            initDataGrid(); 
        }        
    }
}
