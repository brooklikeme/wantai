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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WanTai.Controller.EVO;
using WanTai.DataModel;
using WanTai.Controller.Configuration;
using System.Xml;
using System.Data;
using System.IO;
using System.Diagnostics;
using Microsoft.Windows.Controls.Ribbon;
using System.Threading;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean EVOClosed = false;
        public delegate void ExperimentRunStatusHandler();
        public delegate void CloseWindowHandler();
        public event WanTai.View.MainPage.SendStopRunMsg StopRunEvent;

        public MainWindow()
        {
            InitializeComponent();
            //App app = Application.Current as App;
            //mainFrame.Navigate(new Uri("MainPage.xaml", UriKind.Relative));
            //open evo ware
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            bool isOffline = false;
            btnNewExperiment.IsEnabled = false;
            CloseExperiment_button.IsEnabled = false;
            CloseLamp_button.IsEnabled = false;
            TecanMaintainDay_Button.IsEnabled = false;
            TecanMaintainWeek_Button.IsEnabled = false;
            TecanMaintainMonth_Button.IsEnabled = false;
            TecanRestoration_Button.IsEnabled = false;
            exit_button.IsEnabled = false;
            suspend_exit_button.IsEnabled = false;
            Stream imageStream = Application.GetResourceStream(new Uri("/WanTag;component/Resources/loading.gif", UriKind.Relative)).Stream;
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageStream);
            this.imageExpender1.Image = bitmap;
            SessionInfo.WorkDeskType = WanTai.Common.Configuration.GetWorkDeskType();
            SessionInfo.WaitForSuspend = false;
            SessionInfo.SystemConfigurations = new Dictionary<string,string>();
            foreach(SystemConfiguration sc_item in  new SystemConfigurationController().GetAll()) {
                SessionInfo.SystemConfigurations.Add(sc_item.ItemCode, sc_item.ItemValue);
            }
            QueryNAT_Button.Visibility = WanTai.Common.Configuration.GetShowReagentExport() ? Visibility.Visible : Visibility.Collapsed;
            SessionInfo.BatchScanTimes = 0;
            if (SessionInfo.WorkDeskType == "200") {
                SessionInfo.WorkDeskMaxSize = 72;
            }
            else {
                SessionInfo.WorkDeskMaxSize = 36;
            }
            SessionInfo.FirstStepMixing = 0;

 
            IProcessor processor = null;
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {   
                processor = Controller.EVO.ProcessorFactory.GetProcessor();
                isOffline = processor.isEVOOffline();
                processor.onOfflineStatus += new Controller.EVO.IProcessor.OnEvoError(delegate {
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                {
                    EVOOfflineStatus.Content = "脱机";
                }));  
                });
 
            };
            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                btnNewExperiment.IsEnabled = true;
                CloseExperiment_button.IsEnabled = true;
                CloseLamp_button.IsEnabled = true;
                TecanMaintainDay_Button.IsEnabled = true;
                TecanMaintainWeek_Button.IsEnabled = true;
                TecanMaintainMonth_Button.IsEnabled = true;
                TecanRestoration_Button.IsEnabled = true;
                exit_button.IsEnabled = true;
                suspend_exit_button.IsEnabled = false;
                imageExpender1.Image = null;
                imageExpender1.Dispose();
                this.mainFrame.Content = null;
                if (isOffline)
                {
                    EVOOfflineStatus.Content = "脱机";
                }
                else
                {
                    EVOOfflineStatus.Content = "联机";
                }
                // load last experment
                LoadLastExperiment();
            };            
            worker.RunWorkerAsync();
            
            Authorize();
        }

        private void Authorize()
        {
            RoleInfo userInfo = new UserInfoController().GetRoleByUserName(SessionInfo.LoginName);
            StringReader reader = new StringReader(userInfo.RoleModules);
            DataSet dataset = new DataSet();
            DataTable dataTable = new DataTable("Menu");
            DataColumn column1 = new DataColumn("menuName", typeof(string));
            column1.ColumnMapping = MappingType.Attribute;

            DataColumn column2 = new DataColumn("access", typeof(string));
            column2.ColumnMapping = MappingType.Attribute;
            dataTable.Columns.Add(column1);
            dataTable.Columns.Add(column2);
            dataset.Tables.Add(dataTable);
            dataset.ReadXml(reader, XmlReadMode.Fragment);
            reader.Close();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                string menuName = dataTable.Rows[i]["menuName"].ToString();
                string access = dataTable.Rows[i]["access"].ToString();
                object obj = this.FindName(menuName);
                if (obj is UIElement)
                {
                    ((UIElement)obj).Visibility = access == "1" ? Visibility.Visible : Visibility.Collapsed;
                    //if ((obj is RibbonButton) && access != "1")
                    //{
                    //    ((RibbonButton)obj).Width = 0;
                    //}
                }
            }
            //Thread therad = new Thread(new ThreadStart(delegate()
            //{
            //    string RotationName=string.Empty;
            //    while (CloseFalg)
            //    {
            //        if (SessionInfo.PraperRotation != null && SessionInfo.PraperRotation.RotationName != RotationName)
            //        {
            //            SessionInfo.PraperRotation.RotationName = RotationName;
            //            ShowText(SessionInfo.PraperRotation.RotationName);
            //        }
            //        Thread.Sleep(1000*10);

            //    }
            //}));
          //  therad.Start();
        }
        private delegate void ShowTextHandle(string text);

        public void LoadLastExperiment()
        {
            // resume execution
            SessionInfo.ResumeExecution = File.Exists("SessionInfo.bin");
            if (SessionInfo.ResumeExecution)
            {
                if (MessageBox.Show("上次实验未完成，是否继续执行？选择不继续会删除已执行结果!", "系统提示!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // load last session info
                    SerializeStatic serialize = new SerializeStatic();
                    serialize.Load("SessionInfo.bin");
                    // new main page
                    MainPage mainPage = new MainPage();
                    mainPage.SetEvoRestorationStatus += new MainPage.EvoRestorationStatus(SetEvoRestorationButtonStatus);
                    mainPage.AddEvoRestorationStatusEvent();
                    mainPage.ExperimentRunStatusEvent += new ExperimentRunStatusHandler(SuspendExitButtonControl);
                    mainPage.CloseWindowEvent += new CloseWindowHandler(CloseWindow);
                    StopRunEvent += new WanTai.View.MainPage.SendStopRunMsg(mainPage.SendStopRunMessage);
                    mainFrame.Content = mainPage;
                    this.Title = "WanTag 全自动核酸提取系统——实验 " + SessionInfo.CurrentExperimentsInfo.ExperimentName;
                    mainPage.ContinueExperiment();
                }
                else
                {
                    File.Delete("SessionInfo.bin");
                }
            }
        }

        private void CloseWindow()
        {
            this.Close();
        }

        private void SuspendExitButtonControl()
        {
            suspend_exit_button.IsEnabled = (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Processing);
        }

        /// <summary>
        /// 显示文本
        /// </summary>
        /// <param name="text">string</param>
        private void ShowText(string text)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.Invoke(new ShowTextHandle(this.ShowText), text);
            }
            else
            {
              //labRotation.Content = text;
            }
        }
        private void btnNewExperiment_Click(object sender, RoutedEventArgs e)
        {

            if (SessionInfo.CurrentExperimentsInfo != null)
            {
                if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Processing)
                {
                    MessageBox.Show("当前实验正在运行,请先停止!", "系统提示!");
                    return;
                }
                else
                {
                    if (SessionInfo.FirstStepMixing == 3)
                    {
                        if (MessageBox.Show("正在进行扫描与上样操作,是否确认关闭实验?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                            return;
                        if (null != StopRunEvent) StopRunEvent();
                        WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                        processor.StopScript();
                    }
                    else if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Fail || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Create
                     || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Suspend)
                    {
                        if (!SessionInfo.WaitForSuspend)
                        {
                            if (MessageBox.Show("当前实验未完成,是否继续操作?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                return;
                        }
                        WanTai.Controller.RotationInfoController rotationInfoController = new WanTai.Controller.RotationInfoController();
                        rotationInfoController.UpdataExperimentStatus(SessionInfo.CurrentExperimentsInfo.ExperimentID, true, ExperimentStatus.Fail);
                    }
                }
            }

            TecanMaintainDay_Button.IsEnabled = false;
            TecanMaintainWeek_Button.IsEnabled = false;
            TecanMaintainMonth_Button.IsEnabled = false;
          
            SessionInfo.ExperimentID = new Guid();
            SessionInfo.PraperRotation = null;
            SessionInfo.CurrentExperimentsInfo = null;
            NewExperiment newExperiment = new NewExperiment();
            ribbon.IsEnabled = false;
            mainFrame.IsEnabled = false;
            newExperiment.Topmost = true;
            newExperiment.ShowDialog();
            
            bool? newExperimentResult = newExperiment.DialogResult;
            if (newExperimentResult.HasValue && (bool)newExperimentResult)
            {
              MainPage mainPage = new MainPage();
              mainPage.SetEvoRestorationStatus+=new MainPage.EvoRestorationStatus(SetEvoRestorationButtonStatus);
              mainPage.AddEvoRestorationStatusEvent();
              mainPage.ExperimentRunStatusEvent += new ExperimentRunStatusHandler(SuspendExitButtonControl);
              mainPage.CloseWindowEvent += new CloseWindowHandler(CloseWindow);
              StopRunEvent += new WanTai.View.MainPage.SendStopRunMsg(mainPage.SendStopRunMessage);
              mainFrame.Content = mainPage;
              SessionInfo.RotationIndex = 0;
              SessionInfo.NextTurnStep = -1;
              SessionInfo.FirstStepMixing = 0;
              WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
              processor.OnNextTurnStepDispse();
                //  mainFrame.Navigate(mainPage);
                //mainFrame.Source = new Uri("MainPage.xaml", UriKind.Relative);
                // mainFrame.Navigate(new Uri("MainPage.xaml", UriKind.Relative));

              this.Title = "WanTag 全自动核酸提取系统——实验 " + newExperiment.txtExperimentName.Text;
            }
            mainFrame.IsEnabled = true;
            ribbon.IsEnabled = true;
        }

        private void SetEvoRestorationButtonStatus(bool isEnabled)
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                this.Dispatcher.Invoke(new MainPage.EvoRestorationStatus(this.SetEvoRestorationButtonStatus), isEnabled);
                return;
            }
            else
            {
                TecanRestoration_Button.IsEnabled = isEnabled;
            }
            
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt || Keyboard.Modifiers==ModifierKeys.Control ||
                Keyboard.Modifiers== ModifierKeys.Windows|| Keyboard.Modifiers==ModifierKeys.Shift
                )
            {
                e.Handled = true;
            }
            else
            {
                base.OnKeyDown(e);
            }
        }
        private void LiquidConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.LiquidConfiguration liquidConfiguration = new Configuration.LiquidConfiguration();
            liquidConfiguration.ShowDialog();
        }

        private void CloseExperiment_button_Click(object sender, RoutedEventArgs e)
        {
            if (SessionInfo.CurrentExperimentsInfo != null)
            {
                if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Processing)
                {
                    MessageBox.Show("当前实验正在运行,请先停止!", "系统提示!");
                    return;
                }
                else
                {
                    if (SessionInfo.FirstStepMixing == 3)
                    {
                        if (MessageBox.Show("正在进行扫描与上样操作,是否确认关闭实验?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                            return;
                        if (null != StopRunEvent) StopRunEvent();
                        WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                        processor.StopScript();
                    }
                    else if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Fail || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Create
                        || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Suspend)
                    {
                        if (!SessionInfo.WaitForSuspend)
                        {
                            if (MessageBox.Show("当前实验未完成,是否继续操作?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                                return;
                        }
                        WanTai.Controller.RotationInfoController rotationInfoController = new WanTai.Controller.RotationInfoController();
                        rotationInfoController.UpdataExperimentStatus(SessionInfo.CurrentExperimentsInfo.ExperimentID, true, ExperimentStatus.Fail);
                    }
                }

                TecanMaintainDay_Button.IsEnabled = true;
                TecanMaintainWeek_Button.IsEnabled = true;
                TecanMaintainMonth_Button.IsEnabled = true;
                TecanRestoration_Button.IsEnabled = true;
                SessionInfo.CurrentExperimentsInfo = null;
                SessionInfo.ExperimentID = new Guid();
                SessionInfo.PraperRotation = null;
                SessionInfo.RotationIndex = 0;
                SessionInfo.NextTurnStep = -1;
                SessionInfo.FirstStepMixing = 0;
                this.Title = "WanTag 全自动核酸提取系统";
            }
            mainFrame.Content = null;
            //mainFrame = new Frame(){  Width=10000, Height=900000   };
            //mainFrame.SetValue(Grid.RowProperty, 1);
            //mainFrame.SetValue(Grid.ColumnProperty, 2);
            //mainFrame.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            //mainFrame.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

        }

        private void ReagentSuppliesConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.ReagentSuppliesConfigurationList reagentSuppliesConfiguration = new Configuration.ReagentSuppliesConfigurationList();
            reagentSuppliesConfiguration.ShowDialog();
        }

        private void SystemConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.SystemConfigurationList systemConfigurationList = new Configuration.SystemConfigurationList();
            systemConfigurationList.ShowDialog();
        }

        private void ReportConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.ReportConfigurationList reportConfigurationList = new Configuration.ReportConfigurationList();
            reportConfigurationList.ShowDialog();
        }

        private void Operation_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.OperationConfigurationList operationConfigList = new Configuration.OperationConfigurationList();
            operationConfigList.ShowDialog();
        }

        private void QueryExperiment_Button_Click(object sender, RoutedEventArgs e)
        {
            HistoryQuery.ExperimentsViewList experimentsViewList = new HistoryQuery.ExperimentsViewList();
            experimentsViewList.ShowDialog();
        }


        private void QueryNAT_Button_Click(object sender, RoutedEventArgs e)
        {
            HistoryQuery.NATViewList natViewList = new HistoryQuery.NATViewList();
            natViewList.ShowDialog();
        }

        private void exit_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void suspend_exit_button_Click(object sender, RoutedEventArgs e)
        {
            if (suspend_exit_button.IsChecked == false)
            {
                SessionInfo.WaitForSuspend = false;
                MessageBox.Show("中断已取消!", "系统提示!");
                suspend_exit_button.Label = "中断退出";
            }
            else
            {
                SessionInfo.WaitForSuspend = true;
                MessageBox.Show("当前脚本执行完成后会中断并退出，下次启动可以继续执行!", "系统提示!");
                suspend_exit_button.Label = "取消中断";
            }                
        }

        private void CloseLamp_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IProcessor processor = ProcessorFactory.GetProcessor();
                processor.CloseLamp();
                //MessageBox.Show("关闭成功", "系统提示");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示");
            }
        }

        private void CloseEVO_button_Click(object sender, RoutedEventArgs e)
        {

            if (ProcessorFactory.HasInitedProcessor)
            {
                try
                {
                    IProcessor processor = ProcessorFactory.GetProcessor();
                    processor.Close();
                    ProcessorFactory.HasInitedProcessor = false;
                    MessageBox.Show("关闭成功", "系统提示");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "系统提示");
                }
            }
        }

        private void OpenEVOware_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fileName=Common.Configuration.GetEvoWarePath();
                string processName=fileName.Substring(fileName.LastIndexOf('\\')+1,fileName.LastIndexOf('.')-fileName.LastIndexOf('\\')-1);
                foreach( Process pro in Process.GetProcesses())
                {
                    if (pro.ProcessName == processName)
                    {
                        MessageBox.Show(processName + "已经启动。", "系统提示");
                        return;
                    }
                }
                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo.Exists)
                {
                    Process process = new Process();
                    process.StartInfo.FileName = fileName;
                    process.Start();
                }
                else
                {
                    MessageBox.Show(fileName + "不存在。", "系统提示");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统提示");
            }
        }

        private void PoolingRulesConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.PoolingRulesConfigurationList configList = new Configuration.PoolingRulesConfigurationList();
            configList.ShowDialog();
        }

        private void TestingItemsConfig_Button_Click(object sender, RoutedEventArgs e)
        {
            Configuration.TestingItemConfigurationList configList = new Configuration.TestingItemConfigurationList();
            configList.ShowDialog();
        }


        private void CreateUser_Button_Click(object sender, RoutedEventArgs e)
        {
            UserManagement.AddUser addUser = new UserManagement.AddUser();
            addUser.ShowDialog();
        }

        private void EditUser_Button_Click(object sender, RoutedEventArgs e)
        {
            UserManagement.UserList users = new UserManagement.UserList();
            users.ShowDialog();
        }

        private void EditPassword_Button_Click(object sender, RoutedEventArgs e)
        {
            UserManagement.ChangePassword changePassword = new UserManagement.ChangePassword();
            changePassword.ShowDialog();
        }

        private void LogView_Button_Click(object sender, RoutedEventArgs e)
        {
            HistoryQuery.LogViewList logViewList = new HistoryQuery.LogViewList();
            logViewList.ShowDialog();
        }

        private void TecanMaintainDay_Button_Click(object sender, RoutedEventArgs e)
        {
            TecanMaintain frm = new TecanMaintain("day");
            frm.scriptFileName = WanTai.Common.Configuration.GetMaintainDayEvoScriptName();
            frm.ShowDialog();
        }

        private void TecanMaintainWeek_Button_Click(object sender, RoutedEventArgs e)
        {
            TecanMaintain frm = new TecanMaintain("week");
            frm.scriptFileName = WanTai.Common.Configuration.GetMaintainWeekEvoScriptName();
            frm.ShowDialog();
        }
        private void TecanMaintainMonth_Button_Click(object sender, RoutedEventArgs e)
        {
            TecanMaintain frm = new TecanMaintain("month");
            frm.scriptFileName = WanTai.Common.Configuration.GetMaintainMonthEvoScriptName();
            frm.ShowDialog();
        }

        private void TecanRestoration_Button_Click(object sender, RoutedEventArgs e)
        {
            TecanRestoration frm = new TecanRestoration();
            frm.scriptFileName =  WanTai.Common.Configuration.GetTecanRestorationScriptName();
            frm.ShowDialog();
        }

        private void Help_button_Click(object sender, RoutedEventArgs e)
        {
            FileInfo fileInfo = new FileInfo("WanTagUserManual.pdf");
            if (fileInfo.Exists)
            {
                Process process = new Process();
                process.StartInfo.FileName = "WanTagUserManual.pdf";
                process.Start();
            }
            else
            {
                MessageBox.Show("aa" + "不存在。", "系统提示");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (EVOClosed) return;
            if (SessionInfo.CurrentExperimentsInfo != null)
            {
                if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Processing)
                {
                    MessageBox.Show("当前实验正在运行, 请先停止!", "系统提示!");
                    e.Cancel = true;
                    return;
                }
                else
                {
                    if (SessionInfo.FirstStepMixing == 3)
                    {
                        if (MessageBox.Show("正在进行扫描与上样操作,是否确认退出?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        {
                            e.Cancel = true;
                            return;
                        }
                        if (null != StopRunEvent) StopRunEvent();
                        WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                        processor.StopScript();
                    }

                    else if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Fail || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Create
                     || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Suspend)
                    {
                        if (!SessionInfo.WaitForSuspend)
                        {
                            if (MessageBox.Show("当前实验未完成, 是否退出?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                        WanTai.Controller.RotationInfoController rotationInfoController = new WanTai.Controller.RotationInfoController();
                        rotationInfoController.UpdataExperimentStatus(SessionInfo.CurrentExperimentsInfo.ExperimentID, true, ExperimentStatus.Fail);
                    }
                }
            }
            else
            {
                if (MessageBox.Show("确定要关闭系统?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            e.Cancel = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            Stream imageStream = Application.GetResourceStream(new Uri("/WanTag;component/Resources/loading.gif", UriKind.Relative)).Stream;
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageStream);
            mainFrame.Content = loading_content;
            this.imageExpender1.Image = bitmap;
            this.loading_title.Content = "系统关闭中……";

            btnNewExperiment.IsEnabled = false;
            CloseExperiment_button.IsEnabled = false;
            CloseLamp_button.IsEnabled = false;
            TecanMaintainDay_Button.IsEnabled = false;
            TecanMaintainWeek_Button.IsEnabled = false;
            TecanMaintainMonth_Button.IsEnabled = false;
            TecanRestoration_Button.IsEnabled = false;
            exit_button.IsEnabled = false;
            suspend_exit_button.IsEnabled = false;
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                if (!ProcessorFactory.HasClosed)
                {
                    IProcessor processor = ProcessorFactory.GetProcessor();
                    processor.Close();
                }
            };
            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {

            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                btnNewExperiment.IsEnabled = true;
                CloseExperiment_button.IsEnabled = true;
                CloseLamp_button.IsEnabled = true;
                TecanMaintainDay_Button.IsEnabled = true;
                TecanMaintainWeek_Button.IsEnabled = true;
                TecanMaintainMonth_Button.IsEnabled = true;
                TecanRestoration_Button.IsEnabled = true;
                exit_button.IsEnabled = true;
                imageExpender1.Image = null;
                imageExpender1.Dispose();
                EVOClosed = true;
                Application.Current.Shutdown();
            };
            worker.RunWorkerAsync();
        }

        private void BackupDB_Button_Click(object sender, RoutedEventArgs e)
        {
            //
            string db_name = "";
            if (InputBox("设置要备份的数据库名", "数据库名(可查看配置文件获取)：", ref db_name) == System.Windows.Forms.DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(db_name))
                {
                    System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                    sfd.FileName = db_name + "_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bak";
                    sfd.Filter = "Bak Files(*.bak)|All Files(*.*)";
                    string fileName = string.Empty;
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        fileName = sfd.FileName;
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        try
                        {
                            string connectionString = WanTai.Common.Configuration.GetConnectionString();

                            using (SqlConnection conn = new SqlConnection(connectionString))
                            {
                                conn.Open();
                                string CommandText = "backup database [" + db_name + "] to disk =" + "'" + fileName + "'";

                                using (SqlCommand cmd = new SqlCommand(CommandText, conn))
                                {
                                    cmd.CommandType = CommandType.Text;

                                    cmd.ExecuteNonQuery();
                                }
                                conn.Close();
                            }
                            System.Windows.Forms.MessageBox.Show("导出数据库成功!");
                        }
                        catch (Exception exxx)
                        {
                            System.Windows.Forms.MessageBox.Show("导出数据库失败：" + exxx.Message);
                        }
                    }
                }
            }
        }

        private void RestoreDB_Button_Click(object sender, RoutedEventArgs e)
        {
            //
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Bak Files(*.bak)|All Files(*.*)";
            string fileName = string.Empty;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = ofd.FileName;
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                string db_name = "";
                if (InputBox("设置要恢复的数据库名", "数据库名不能和已有数据库重复：", ref db_name) == System.Windows.Forms.DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(db_name))
                    {
                        try
                        {
                            string connectionString = WanTai.Common.Configuration.GetConnectionString();

                            using (SqlConnection conn = new SqlConnection(connectionString))
                            {
                                conn.Open();
                                string CommandText = "restore database [" + db_name + "] from disk =" + "'" + fileName + "'";

                                using (SqlCommand cmd = new SqlCommand(CommandText, conn))
                                {
                                    cmd.CommandType = CommandType.Text;

                                    cmd.ExecuteNonQuery();
                                }
                                conn.Close();
                            }
                            System.Windows.Forms.MessageBox.Show("恢复数据库成功!");
                        }
                        catch (Exception exxx)
                        {
                            System.Windows.Forms.MessageBox.Show("恢复数据库失败：" + exxx.Message);
                        }
                    }      
                }
            }
        }


        
        public static System.Windows.Forms.DialogResult InputBox(string title, string promptText, ref string value)
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            System.Windows.Forms.Button buttonOk = new System.Windows.Forms.Button();
            System.Windows.Forms.Button buttonCancel = new System.Windows.Forms.Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 30);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | System.Windows.Forms.AnchorStyles.Right;
            buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new System.Windows.Forms.Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            System.Windows.Forms.DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}