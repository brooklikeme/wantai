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
using WanTai.Controller.Configuration;

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for ExperimentsViewList.xaml
    /// </summary>
    public partial class ExperimentsViewList : Window
    {
        ExperimentsController controller = new ExperimentsController();
        DataTable dataTable = new DataTable();
        private int current_PageNumber = 1;
        private int totalPageNumber;
        private string sortColumn;
        private string sortDirection;

        public ExperimentsViewList()
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
            dataTable.Columns.Add("ExperimentID", typeof(Guid));
            dataTable.Columns.Add("ExperimentName", typeof(string));
            dataTable.Columns.Add("StartTime", typeof(DateTime));
            dataTable.Columns.Add("EndTime", typeof(string));
            dataTable.Columns.Add("LoginName", typeof(string));
            dataTable.Columns.Add("Remark", typeof(string));
            dataTable.Columns.Add("State", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            dataTable.Columns.Add("CanDelete", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "查看实验历史记录", SessionInfo.LoginName, this.GetType().ToString(), null);
            initDataGrid();
            currentPage_textBox.Text = current_PageNumber.ToString();
        }

        private void getTotalPageNumber()
        {
            int totalCount = controller.GetExperimentsTotalCount(experimentName_textBox.Text, experimentDate_datePicker.Text, userName_textBox.Text);
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
            List<ExperimentsInfo> experimentsList = controller.GetNextExperiments(experimentName_textBox.Text, experimentDate_datePicker.Text, userName_textBox.Text, startIndex, PageSetting.RowNumberInEachPage, sortColumn, sortDirection);
            UserInfoController userInfoController = new UserInfoController();
            RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);   
            foreach (ExperimentsInfo experiment in experimentsList)
            {
                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["Number"] = startIndex;
                dRow["ExperimentID"] = experiment.ExperimentID;
                dRow["ExperimentName"] = experiment.ExperimentName;
                dRow["StartTime"] = experiment.StartTime;
                dRow["EndTime"] = experiment.EndTime.ToString();
                dRow["LoginName"] = experiment.LoginName;
                dRow["Remark"] = experiment.Remark;
                dRow["State"] = controller.ConvertEnumStatusToText((ExperimentStatus)experiment.State);
                if (startIndex % 2 == 0)
                {
                    dRow["Color"] = WindowCustomizer.alternativeColor;
                }
                else
                {
                    dRow["Color"] = WindowCustomizer.defaultColor;
                }

                if (SessionInfo.LoginName == experiment.LoginName || userRole.RoleLevel > 1)
                {
                    dRow["CanDelete"] = Visibility.Visible.ToString();
                }
                else
                {
                    dRow["CanDelete"] = Visibility.Hidden.ToString();
                }
                dataTable.Rows.Add(dRow);
                startIndex++;
            }
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
            string experimentDate = experimentDate_datePicker.Text;
            string userName = userName_textBox.Text;
            if (string.IsNullOrEmpty(experimentName) && string.IsNullOrEmpty(experimentDate) && string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("请输入查询条件", "系统提示");
                return;
            }

            dataTable.Rows.Clear();
            initDataGrid();
        }

        private void delete_button_Click(object sender, RoutedEventArgs e)
        {
           
            MessageBoxResult selectResult = MessageBox.Show("确定删除么？此操作将会删除此实验下的所有信息", "系统提示", MessageBoxButton.YesNo);
            if (selectResult == MessageBoxResult.Yes)
            {
                int selectedIndex = dataGrid_view.SelectedIndex;
                string deletedSequence = dataTable.Rows[selectedIndex]["ExperimentID"].ToString();
                if (new Guid(deletedSequence) == SessionInfo.ExperimentID)
                {
                    MessageBox.Show("当前实验不能删除!","系统提示!");
                    return;
                }
                bool result = controller.Delete(new Guid(deletedSequence));
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除实验历史记录：" + dataTable.Rows[selectedIndex]["ExperimentName"] + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);

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

        private void OnExperimentNameClick(object sender, RoutedEventArgs e)
        {
            int selectedIndex = dataGrid_view.SelectedIndex;
            Guid experimentID = (Guid)dataTable.Rows[selectedIndex]["ExperimentID"];
            string experimentName = dataTable.Rows[selectedIndex]["ExperimentName"].ToString();

            ExperimentsViewDetail detail = new ExperimentsViewDetail();
            detail.ExperimentId = experimentID;
            detail.ExperimentName = experimentName;
            detail.ShowDialog();
        }

        private void experimentDate_datePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker; 
            if (dp == null) return; var tb = GetChildOfType<System.Windows.Controls.Primitives.DatePickerTextBox>(dp); 
            if (tb == null) return; var wm = tb.Template.FindName("PART_Watermark", tb) as ContentControl; 
            if (wm == null) return; wm.Content = null;
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject 
        { 
            if (depObj == null) return null; 
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) 
            { 
                var child = VisualTreeHelper.GetChild(depObj, i); var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result; 
            } 
            return null; 
        }

        private void dataGrid_view_Sorting(object sender, DataGridSortingEventArgs e)
        {
            sortColumn = e.Column.SortMemberPath;
            if(e.Column.SortDirection == null)
                sortDirection = "asc";
            else if (e.Column.SortDirection != null && e.Column.SortDirection.Value.ToString()=="Ascending")
                sortDirection = "desc";
            else if (e.Column.SortDirection != null && e.Column.SortDirection.Value.ToString() == "Descending")
                sortDirection = "asc";
            dataTable.Rows.Clear();
            initDataGrid();
        }

        private void barcodequery_button_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tube_barcode.Text))
            {
                MessageBox.Show("请输入条码", "系统提示");
                return;
            }

            PCRBarcodeQueryResult barcodeQuery = new PCRBarcodeQueryResult();
            barcodeQuery.TubeBarcode = tube_barcode.Text.Trim();
            barcodeQuery.ShowDialog();
        }
    }
}
