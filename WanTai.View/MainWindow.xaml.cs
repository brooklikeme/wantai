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
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean EVOClosed = false;

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
            TecanMaintain_Button.IsEnabled = false;
            TecanRestoration_Button.IsEnabled = false;
            exit_button.IsEnabled = false;
            Stream imageStream = Application.GetResourceStream(new Uri("/WanTag;component/Resources/loading.gif", UriKind.Relative)).Stream;
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageStream);
            this.imageExpender1.Image = bitmap;
            SessionInfo.WorkDeskType = WanTai.Common.Configuration.GetWorkDeskType();
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
                TecanMaintain_Button.IsEnabled = true;
                TecanRestoration_Button.IsEnabled = true;
                exit_button.IsEnabled = true;
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
                    MessageBox.Show("当前实验正在运行,请先停止!","系统提示!");
                    return;
                }
                if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Fail || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Create
                 || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Suspend)
                {
                    if(MessageBox.Show("当前实验未完成,是否继续操作?","系统提示!",MessageBoxButton.YesNo)!= MessageBoxResult.Yes)
                     return;
                    WanTai.Controller.RotationInfoController rotationInfoController = new WanTai.Controller.RotationInfoController();
                    rotationInfoController.UpdataExperimentStatus(SessionInfo.CurrentExperimentsInfo.ExperimentID, true, ExperimentStatus.Fail);
                }
            }

            TecanMaintain_Button.IsEnabled = false;
          
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
              mainFrame.Content = mainPage;
              SessionInfo.BatchIndex = 0;
              SessionInfo.NextTurnStep = -1;
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
                if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Fail || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Create
                    || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Suspend)
                {
                    if (MessageBox.Show("当前实验未完成,是否继续操作?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                        return;
                    WanTai.Controller.RotationInfoController rotationInfoController = new WanTai.Controller.RotationInfoController();
                    rotationInfoController.UpdataExperimentStatus(SessionInfo.CurrentExperimentsInfo.ExperimentID, true, ExperimentStatus.Fail);
                }
                TecanMaintain_Button.IsEnabled = true;
                TecanRestoration_Button.IsEnabled = true;
                SessionInfo.CurrentExperimentsInfo = null;
                SessionInfo.ExperimentID = new Guid();
                SessionInfo.PraperRotation = null;
                SessionInfo.BatchIndex = 0;
                SessionInfo.NextTurnStep = -1;
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

        private void exit_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

        private void TecanMaintain_Button_Click(object sender, RoutedEventArgs e)
        {
            TecanMaintain frm = new TecanMaintain();
            frm.scriptFileName = WanTai.Common.Configuration.GetMaintainEvoScriptName();
            frm.ShowDialog();
            //MaintainEvo maintainEvo = new MaintainEvo();
            //maintainEvo.ShowDialog();
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
                if (SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Fail || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Create
                 || SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Suspend)
                {
                    if (MessageBox.Show("当前实验未完成, 是否退出?", "系统提示!", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                    WanTai.Controller.RotationInfoController rotationInfoController = new WanTai.Controller.RotationInfoController();
                    rotationInfoController.UpdataExperimentStatus(SessionInfo.CurrentExperimentsInfo.ExperimentID, true, ExperimentStatus.Fail);
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
            TecanMaintain_Button.IsEnabled = false;
            TecanRestoration_Button.IsEnabled = false;
            exit_button.IsEnabled = false;
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
                TecanMaintain_Button.IsEnabled = true;
                TecanRestoration_Button.IsEnabled = true;
                exit_button.IsEnabled = true;
                imageExpender1.Image = null;
                imageExpender1.Dispose();
                EVOClosed = true;
                Application.Current.Shutdown();
            };
            worker.RunWorkerAsync();
        }
    }
}