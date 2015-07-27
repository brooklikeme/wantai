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

using WanTai.DataModel;
using WanTai.Controller.HistoryQuery;

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for LogViewList.xaml
    /// </summary>
    public partial class LogViewList : Window
    {
        LogViewController controller = new LogViewController();
        DataTable dataTable = new DataTable();
        private int current_PageNumber=1;
        private int totalPageNumber;

        public LogViewList()
        {
            InitializeComponent();
        } 
       
        public int CurrentPageNumber
        {
            set { current_PageNumber = value; }
            get { return current_PageNumber; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("Number", typeof(int));
            dataTable.Columns.Add("LogContent", typeof(string));
            dataTable.Columns.Add("Module", typeof(string));
            dataTable.Columns.Add("CreaterTime", typeof(string));
            dataTable.Columns.Add("LoginName", typeof(string));
            dataTable.Columns.Add("ExperimentName", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            foreach (string itemName in Enum.GetNames(typeof(LogInfoLevelEnum)))
            {
                ComboBoxItem comboxItem = new ComboBoxItem() { Content = itemName, Tag= Enum.Parse(typeof(LogInfoLevelEnum), itemName)};
                if (itemName == "Operate")
                {
                    comboxItem.IsSelected = true;
                }

                logLevel_comboBox.Items.Add(comboxItem);                
            }

            //initDataGrid();
            currentPage_textBox.Text = current_PageNumber.ToString();            
        }

        private void getTotalPageNumber()
        {
            int totalCount = controller.GetTotalCount(experimentName_textBox.Text, (LogInfoLevelEnum)(((ComboBoxItem)logLevel_comboBox.SelectedItem).Tag));
            if (totalCount > 0)
            {
                totalPageNumber = totalCount / PageSetting.RowNumberInEachPage + (totalCount % PageSetting.RowNumberInEachPage > 0 ? 1 : 0);
            }
            else
            {
                totalPageNumber = 0;
            }

            totalPageNumber_label.Content = totalPageNumber;
            totalCount_label.Content = totalCount;            
        }

        private void initDataGrid()
        {
            getTotalPageNumber();
            if (current_PageNumber > totalPageNumber)
            {
                current_PageNumber = 1;
            }

            currentPage_textBox.Text = current_PageNumber.ToString();

            int startIndex = (current_PageNumber - 1) * PageSetting.RowNumberInEachPage + 1;
            controller.GetNextLogs(experimentName_textBox.Text, startIndex, PageSetting.RowNumberInEachPage, (LogInfoLevelEnum)(((ComboBoxItem)logLevel_comboBox.SelectedItem).Tag), dataTable, WindowCustomizer.alternativeColor, WindowCustomizer.defaultColor);            
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void previousPage_button_Click(object sender, RoutedEventArgs e)
        {
            if (current_PageNumber > 1)
            {
                current_PageNumber = current_PageNumber - 1;
                dataTable.Rows.Clear();
                initDataGrid();
            }
        }

        private void nextPage_button_Click(object sender, RoutedEventArgs e)
        {
            if (current_PageNumber < totalPageNumber)
            {
                current_PageNumber = current_PageNumber + 1;
                dataTable.Rows.Clear();
                initDataGrid();
            }
        }

        private void go_button_Click(object sender, RoutedEventArgs e)
        {
            int iout = 0;
            if (string.IsNullOrEmpty(currentPage_textBox.Text) || !int.TryParse(currentPage_textBox.Text, out iout))
            {
                MessageBox.Show("请输入合法的页码", "系统提示");
                return;
            }

            getTotalPageNumber();

            if (int.Parse(currentPage_textBox.Text) <= 0)
            {
                current_PageNumber = 1;
            }
            else if (int.Parse(currentPage_textBox.Text) > totalPageNumber)
            {
                current_PageNumber = totalPageNumber;
            }
            else
            {
                current_PageNumber = int.Parse(currentPage_textBox.Text);
            }

            dataTable.Rows.Clear();
            initDataGrid();
        }

        private void query_button_Click(object sender, RoutedEventArgs e)
        {
            string experimentName = experimentName_textBox.Text;
            if (string.IsNullOrEmpty(experimentName))
            {
                MessageBox.Show("请输入实验名称", "系统提示");
                return;
            }

            dataTable.Rows.Clear();
            initDataGrid();
        }

        private void logLevel_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            current_PageNumber = 1;
            dataTable.Rows.Clear();
            initDataGrid();
        }

        private void dataGrid_view_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCellInfo Cell = ((System.Windows.Controls.DataGrid)(e.Source)).CurrentCell;
            if (Cell.Column.Header.ToString() == "内容")
            {
                DataRow row =  dataTable.Rows[dataGrid_view.SelectedIndex];
                if (row != null && row["LogContent"]!=null)
                {
                    LogContentView view = new LogContentView();
                    view.SetLogContent(row["LogContent"].ToString());
                    view.ShowDialog();
                }
            }
            else
            {
                return;
            }
        }
    }
}
