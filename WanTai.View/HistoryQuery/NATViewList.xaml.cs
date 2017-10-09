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
using System.IO;
using System.Windows.Forms;
using System.Data.OleDb;

using WanTai.DataModel;
using WanTai.Controller.HistoryQuery;
using WanTai.Controller.Configuration;

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for ExperimentsViewList.xaml
    /// </summary>
    public partial class NATViewList : Window
    {
        ExperimentsController controller = new ExperimentsController();
        DataTable dataTable = new DataTable();

        public NATViewList()
        {
            InitializeComponent();
            endDate_datePicker.SelectedDate = DateTime.Now.Date;
            beginDate_datePicker.SelectedDate = DateTime.Now.Date.AddDays(1 - DateTime.Now.Date.Day);
            if (SessionInfo.WorkDeskType == "100")
            {
                DataGridColumn colComplement = dataGrid_view.Columns.Where(c => c.Header.ToString() == "定量参考品").FirstOrDefault();
                if (colComplement != null)
                {
                    colComplement.Visibility = Visibility.Visible;
                }
                DataGridColumn colSample = dataGrid_view.Columns.Where(c => c.Header.ToString() == "样本数").FirstOrDefault();
                if (colSample != null)
                {
                    colSample.Visibility = Visibility.Visible;
                }
                DataGridColumn colMix = dataGrid_view.Columns.Where(c => c.Header.ToString() == "6混数").FirstOrDefault();
                if (colMix != null)
                {
                    colMix.Visibility = Visibility.Hidden;
                }
                DataGridColumn colSingle = dataGrid_view.Columns.Where(c => c.Header.ToString() == "单检数").FirstOrDefault();
                if (colSingle != null)
                {
                    colSingle.Visibility = Visibility.Hidden;
                }
                DataGridColumn colSplit = dataGrid_view.Columns.Where(c => c.Header.ToString() == "拆分数").FirstOrDefault();
                if (colSplit != null)
                {
                    colSplit.Visibility = Visibility.Hidden;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("Number", typeof(int));
            dataTable.Columns.Add("StartTime", typeof(string));
            dataTable.Columns.Add("NC", typeof(int));
            dataTable.Columns.Add("PC", typeof(int));
            dataTable.Columns.Add("Complement", typeof(int));
            dataTable.Columns.Add("QC", typeof(int));
            dataTable.Columns.Add("Mix", typeof(int));
            dataTable.Columns.Add("Split", typeof(int));
            dataTable.Columns.Add("Sample", typeof(int));
            dataTable.Columns.Add("Single", typeof(int));
            dataTable.Columns.Add("ReagentTheory", typeof(int));
            dataTable.Columns.Add("ReagentCost", typeof(int));
            dataTable.Columns.Add("ReagentTotal", typeof(int));
            dataTable.Columns.Add("Diti1000", typeof(int));
            dataTable.Columns.Add("Diti200", typeof(int));
            dataTable.Columns.Add("DW96", typeof(int));
            dataTable.Columns.Add("Microtiter", typeof(int));
            dataTable.Columns.Add("ReagentSlot", typeof(double));
            dataTable.Columns.Add("PCR", typeof(int));
            dataTable.Columns.Add("Color", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "查看试剂耗材历史记录", SessionInfo.LoginName, this.GetType().ToString(), null);
            initDataGrid();
        }


        private void initDataGrid()
        {
            int startIndex = 1;
            DateTime beginDate = (DateTime)beginDate_datePicker.SelectedDate;
            DateTime endDate = (DateTime)endDate_datePicker.SelectedDate;
            beginDate = beginDate.Date;
            endDate = endDate.Date;
            List<NATInfo> natList = controller.GetNATInfos(beginDate.ToString("yyyy-MM-dd 00:00:00"), endDate.ToString("yyyy-MM-dd 23:59:59"));
            int NC_SUM = 0, PC_SUM = 0, Complement_SUM = 0, QC_SUM = 0, Mix_SUM = 0, Split_SUM = 0, Single_SUM = 0, Sample_SUM = 0, ReagentTheory_SUM = 0, ReagentCost_SUM = 0
                , ReagentTotal_SUM = 0, Diti1000_SUM = 0, Diti200_SUM = 0, DW96_SUM = 0, Microtiter_SUM = 0,  PCR_SUM = 0;
            double ReagentSlot_SUM = 0.0;
            while (beginDate <= endDate) {
                int expTimes = 0;
                double ReagentSlot = 0.0;
                int NC = 0, PC = 0, Complement = 0, QC = 0, Mix = 0, Split = 0, Single = 0, Sample = 0, ReagentTheory = 0, ReagentCost = 0
                    , ReagentTotal = 0, Diti1000 = 0, Diti200 = 0, DW96 = 0, Microtiter = 0, PCR = 0;
                foreach (NATInfo natInfo in natList)
                {
                    if (natInfo.StartTime == beginDate) {
                        expTimes ++;
                        NC += natInfo.NC;
                        NC_SUM += natInfo.NC;
                        PC += natInfo.PC;
                        PC_SUM += natInfo.PC;
                        Complement += natInfo.Complement;
                        Complement_SUM += natInfo.Complement;
                        QC += natInfo.QC;
                        QC_SUM += natInfo.QC;
                        Mix += natInfo.Mix;
                        Mix_SUM += natInfo.Mix;
                        Split += natInfo.Split;
                        Split_SUM += natInfo.Split;
                        Sample += natInfo.Sample;
                        Sample_SUM += natInfo.Sample;
                        Single += natInfo.Single;
                        Single_SUM += natInfo.Single;
                        ReagentTheory += natInfo.ReagentTheory;
                        ReagentTheory_SUM += natInfo.ReagentTheory;
                        ReagentCost += natInfo.ReagentCost;
                        ReagentCost_SUM += natInfo.ReagentCost;
                        ReagentTotal += natInfo.ReagentTotal;
                        ReagentTotal_SUM += natInfo.ReagentTotal;
                        Diti1000 += natInfo.Diti1000;
                        Diti1000_SUM += natInfo.Diti1000;
                        Diti200 += natInfo.Diti200;
                        Diti200_SUM += natInfo.Diti200;
                        PCR += natInfo.PCR;
                        PCR_SUM += natInfo.PCR;
                    }
                }
                if (expTimes == 0)
                {
                    beginDate = beginDate.AddDays(1);
                    continue;
                }
                if (SessionInfo.WorkDeskType == "100")
                {
                    DW96 += expTimes * 5;
                    DW96_SUM += expTimes * 5;
                }
                else
                {
                    DW96 += expTimes * 6;
                    DW96_SUM += expTimes * 6;
                }
                Microtiter += expTimes * 1;
                Microtiter_SUM += expTimes * 1;
                ReagentSlot += expTimes * 0.57143;
                ReagentSlot_SUM += expTimes * 0.57143;

                System.Data.DataRow dRow = dataTable.NewRow();
                dRow["StartTime"] = beginDate.ToString("yyyy-MM-dd");
                dRow["Number"] = startIndex;
                dRow["NC"] = NC;
                dRow["PC"] = PC;
                dRow["Complement"] = Complement;
                dRow["QC"] = QC;
                dRow["Mix"] = Mix;
                dRow["Split"] = Split;
                dRow["Sample"] = Sample;
                dRow["Single"] = Single;
                dRow["ReagentTheory"] = ReagentTheory;
                dRow["ReagentCost"] = ReagentCost;
                dRow["ReagentTotal"] = ReagentTotal;
                dRow["Diti1000"] = Diti1000;
                dRow["Diti200"] = Diti200;
                dRow["PCR"] = PCR;
                dRow["DW96"] = DW96;
                dRow["Microtiter"] = Microtiter;
                dRow["ReagentSlot"] = ReagentSlot;
                if (startIndex % 2 == 0)
                {
                    dRow["Color"] = WindowCustomizer.alternativeColor;
                }
                else
                {
                    dRow["Color"] = WindowCustomizer.defaultColor;
                }
                dataTable.Rows.Add(dRow);
                startIndex++;

                beginDate = beginDate.AddDays(1);
            }
            // add summarize
            System.Data.DataRow sumRow = dataTable.NewRow();
            sumRow["StartTime"] = "合计";
            sumRow["NC"] = NC_SUM;
            sumRow["PC"] = PC_SUM;
            sumRow["Complement"] = Complement_SUM;
            sumRow["QC"] = QC_SUM;
            sumRow["Mix"] = Mix_SUM;
            sumRow["Split"] = Split_SUM;
            sumRow["Sample"] = Sample_SUM;
            sumRow["Single"] = Single_SUM;
            sumRow["ReagentTheory"] = ReagentTheory_SUM;
            sumRow["ReagentCost"] = ReagentCost_SUM;
            sumRow["ReagentTotal"] = ReagentTotal_SUM;
            sumRow["Diti1000"] = Diti1000_SUM;
            sumRow["Diti200"] = Diti200_SUM;
            sumRow["PCR"] = PCR_SUM;
            sumRow["DW96"] = DW96_SUM;
            sumRow["Microtiter"] = Microtiter_SUM;
            sumRow["ReagentSlot"] = (int)Math.Ceiling(ReagentSlot_SUM);
            if (startIndex % 2 == 0)
            {
                sumRow["Color"] = WindowCustomizer.alternativeColor;
            }
            else
            {
                sumRow["Color"] = WindowCustomizer.defaultColor;
            }
            dataTable.Rows.Add(sumRow);
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void export_button_Click(object sender, RoutedEventArgs e)
        {
            DateTime beginDate = (DateTime)beginDate_datePicker.SelectedDate;
            DateTime endDate = (DateTime)endDate_datePicker.SelectedDate;
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            //string experimentName = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(experimentId).ExperimentName;
            //if ((bool)cbxShowReagent.IsChecked) {
                 sfd.FileName += "试剂";
            //}
            //if ((bool)cbxShowSupply.IsChecked) {
                 sfd.FileName += "耗材";
            //}
            sfd.FileName += "汇总" + beginDate.ToString("yyyyMMdd") + "-" + endDate.ToString("yyyyMMdd");
            sfd.Filter = "pdf(*.pdf)|*.pdf|excel(*.xls)|*.xls";
            string rawFileName = sfd.FileName;

            string fileName = string.Empty;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = sfd.FileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
//                try
                {
                    string extension = System.IO.Path.GetExtension(fileName);
                    bool result = extension.Equals(".pdf") ? controller.ExportToPdf(dataTable, fileName, rawFileName) : ExportToExcel(fileName);
                    if (result)
                    {
                        if (System.Windows.Forms.MessageBox.Show("导出文件成功! 是否打开文件?", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == (System.Windows.Forms.DialogResult.Yes))
                        {
                            System.Diagnostics.Process.Start(fileName);
                        }
                    }
                }
//                catch (Exception ex)
//                {
//                    System.Windows.Forms.MessageBox.Show("导出文件失败：" + ex.Message);
 //               }
            }

        }


        private bool ExportToExcel(string fileName){
            using (OleDbConnection connection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties=\"Excel 8.0;HDR=YES;\""))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand();
                command.Connection = connection;

                string createTableSql = "create table [汇总] ([序号] nvarchar,[日期] nvarchar, [NC] Integer,[PC] Integer,[QC] integer,"
                    + "[6混数] Integer,[拆分数] Integer, [单检数] Integer,[理论试剂用量] Integer,[试剂损耗] Integer,[试剂总用量] Integer,"
                    + "[Diti1000] Integer,[Diti200] Integer,[DW96深孔板] Integer,[磁头套管] Integer,[试剂槽100ml] nvarchar, [扩增耗材] Integer)";
                if (SessionInfo.WorkDeskType == "100")
                    createTableSql = "create table [汇总] ([序号] nvarchar,[日期] nvarchar, [NC] Integer,[PC] Integer,[定量参考品] integer, [QC] integer,"
                        + "[样本数] Integer,[理论试剂用量] Integer,[试剂损耗] Integer,[试剂总用量] Integer,"
                        + "[Diti1000] Integer,[Diti200] Integer,[DW96深孔板] Integer,[磁头套管] Integer,[试剂槽100ml] nvarchar, [扩增耗材] Integer)";
                command.CommandText = createTableSql;
                command.ExecuteNonQuery();

                string insertSql = string.Empty;
                foreach (DataRow row in dataTable.Rows)
                {
                    if (SessionInfo.WorkDeskType == "100")
                    {
                        insertSql = string.Format("Insert into [汇总] (序号,日期,NC,PC,定量参考品,QC,样本数,理论试剂用量,试剂损耗,试剂总用量,Diti1000,"
                             + "Diti200,DW96深孔板,磁头套管,试剂槽100ml, 扩增耗材) "
                             + "values('{0}','{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10}, {11},{12},{13},{14},'{15}')",
                                row["Number"].ToString(),
                                row["StartTime"].ToString(),
                                row["NC"].ToString(),
                                row["PC"].ToString(),
                                row["Complement"].ToString(),
                                row["QC"].ToString(),
                                row["Sample"].ToString(),
                                row["ReagentTheory"].ToString(),
                                row["ReagentCost"].ToString(),
                                row["ReagentTotal"].ToString(),
                                row["Diti1000"].ToString(),
                                row["Diti200"].ToString(),
                                row["DW96"].ToString(),
                                row["Microtiter"].ToString(),
                                row["ReagentSlot"].ToString(),
                                row["PCR"].ToString());
                    }
                    else
                    {
                        insertSql = string.Format("Insert into [汇总] (序号,日期,NC,PC,QC,6混数,拆分数,单检数,理论试剂用量,试剂损耗,试剂总用量,Diti1000,"
                             + "Diti200,DW96深孔板,磁头套管,试剂槽100ml, 扩增耗材) "
                             + "values('{0}','{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10}, {11},{12},{13},{14},'{15}',{16})",
                                row["Number"].ToString(),
                                row["StartTime"].ToString(),
                                row["NC"].ToString(),
                                row["PC"].ToString(),
                                row["QC"].ToString(),
                                row["Mix"].ToString(),
                                row["Split"].ToString(),
                                row["Single"].ToString(),
                                row["ReagentTheory"].ToString(),
                                row["ReagentCost"].ToString(),
                                row["ReagentTotal"].ToString(),
                                row["Diti1000"].ToString(),
                                row["Diti200"].ToString(),
                                row["DW96"].ToString(),
                                row["Microtiter"].ToString(),
                                row["ReagentSlot"].ToString(),
                                row["PCR"].ToString());
                    }
                    command.CommandText = insertSql;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return true;
        }

        private void query_button_Click(object sender, RoutedEventArgs e)
        {
            dataTable.Rows.Clear();
            initDataGrid();
        }

        private void delete_button_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void OnExperimentNameClick(object sender, RoutedEventArgs e)
        {
        }

        private void beginDate_datePicker_Loaded(object sender, RoutedEventArgs e)
        {
            var dp = sender as DatePicker; 
            if (dp == null) return; var tb = GetChildOfType<System.Windows.Controls.Primitives.DatePickerTextBox>(dp); 
            if (tb == null) return; var wm = tb.Template.FindName("PART_Watermark", tb) as ContentControl; 
            if (wm == null) return; wm.Content = null;
        }

        private void endDate_datePicker_Loaded(object sender, RoutedEventArgs e)
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
    }
}
