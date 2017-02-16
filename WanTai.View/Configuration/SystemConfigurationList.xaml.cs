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
    /// Interaction logic for SystemConfiguration.xaml
    /// </summary>
    public partial class SystemConfigurationList : Window
    {
        SystemConfigurationController controller = new SystemConfigurationController();
        DataTable dataTable = new DataTable();
        public SystemConfigurationList()
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
            dataTable.Columns.Add("ItemName", typeof(string));
            dataTable.Columns.Add("ItemCode", typeof(string));
            dataTable.Columns.Add("ItemValue", typeof(string));
            dataTable.Columns.Add("Action", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            initDataGrid();
        }

        private void initDataGrid()
        {
            List<SystemConfiguration> recordList = controller.GetAll();
            dataTable.Rows.Clear();
            foreach (SystemConfiguration record in recordList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["ItemID"] = record.ItemID.ToString();
                dRow["ItemName"] = record.ItemName;
                dRow["ItemCode"] = record.ItemCode;
                dRow["ItemValue"] = record.ItemValue;
 
                dataTable.Rows.Add(dRow);
            }
        }

        private void edit_button_Click(object sender, RoutedEventArgs e)
        {
            int selectIndex = this.dataGrid_view.SelectedIndex;
            System.Data.DataRow dRow = dataTable.Rows[selectIndex];
            string itemId = dRow["ItemID"].ToString();
            SystemConfigurationDlg configurationDlg = new SystemConfigurationDlg();
            configurationDlg.SetEditedItemId(itemId);
            configurationDlg.ShowDialog();
            initDataGrid(); 
        }        
    }
}
