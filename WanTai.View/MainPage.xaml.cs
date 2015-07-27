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
using System.ComponentModel;
using WanTai.DataModel;
using WanTai.Controller;
using WanTai.View.PCR;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MainPage : UserControl
    {
        public delegate void NextStepHandler(object sender, RoutedEventArgs e);
        public delegate void ExperimentRunViewHandler();
        public delegate void NewExperimentHandler();
        public delegate void NewNextRotationHandler();
        public delegate void ExperimentRunErrHandler();
        public delegate void ExperimentRunRutrnHandler();
        public delegate string NextStepScan(string message);
        public delegate void EvoRestorationStatus(bool isEnable);
        public event EvoRestorationStatus SetEvoRestorationStatus;
        private ExperimentRunView experimentRunView = new ExperimentRunView();
        public MainPage()
        {   
            InitializeComponent();
           // bindRunWithStartAction();
            View();
        }
        public void AddEvoRestorationStatusEvent()
        {
            if (SetEvoRestorationStatus != null)
            {
                experimentRunView.SetEvoRestorationStatus += new EvoRestorationStatus(SetEvoRestorationStatus);
            }
        }
        public void View()
        {
            bindRunWithStartAction();
            btnRun.IsEnabled = false;
            btnRecover.IsEnabled = false;
           // runSelect_listBox.IsEnabled = false;
            btnStop.IsEnabled = false;
            //btnRStart.IsEnabled = false;
            for (int i = 1; i < tabControl.Items.Count; i++)
            {
               ((TabItem)tabControl.Items[i]).IsEnabled = false;
            }
            TubesView tubesView1 = new TubesView();
         //   panelTubeView.Children.Add(tubesView1);
            tubesView1.labelRotationName.Content = "";
            tabItem1.Content = tubesView1;
            tubesView1.NextStepEvent += new NextStepHandler(Button_Click_1);
            //tubesView1.onNextStepScan += new NextStepScan(NextStepScanEvent);
            experimentRunView = new ExperimentRunView();
            ExperimentRunView.Content = experimentRunView;
            experimentRunView.NewExperimentEvent += new NewExperimentHandler(NextRotationEvent);
            experimentRunView.ExperimentRunViewEvent += new ExperimentRunViewHandler(ExperimentRunViewEvent);
            experimentRunView.ExperimentRunErrEvent += new ExperimentRunErrHandler(ExperimentRunErrEvent);
            experimentRunView.ExperimentRunRutrnEvent += new ExperimentRunRutrnHandler(ExperimentRunRutrnEvent);            
            tabControl.SelectedIndex = 0;
        }
        private void ExperimentRunRutrnEvent()
        {
            bindRunWithStartAction();
        }
        private void ExperimentRunErrEvent()
        {
            btnStop.IsEnabled = false;
            if (SessionInfo.CurrentExperimentsInfo.State != (short)ExperimentStatus.Stop)
                this.bindRunWithRestarAction();
            //btnStart.Content = "启动";
            //btnRStart.IsEnabled = true;
        }
        private void NewExperimentEvent()
        {
            View();
        }
        private void ExperimentRunViewEvent()
        {
            btnStop.IsEnabled = false;
            btnRun.IsEnabled = false;
            btnRecover.IsEnabled = false;
            bindRunWithStartAction();
           // runSelect_listBox.IsEnabled = false;
            //btnRStart.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).CommandParameter != null && (sender as Button).CommandParameter.ToString() == "ConfigRotationNext")
            {
                experimentRunView.BindView();
                return;
            }
            if (tabControl.SelectedIndex < tabControl.Items.Count)
            {
                tabControl.SelectedIndex++;
                ((TabItem)tabControl.SelectedItem).IsEnabled = true;
            }
            if (tabControl.SelectedIndex == 2)
            {
                if (SessionInfo.CurrentExperimentsInfo.State != (short)ExperimentStatus.Stop)
                {
                    DeskTop deskTop = (DeskTop)frameDeskTop.Content;
                    deskTop.InitDeskTop(900);
                }
            }
            if (tabControl.SelectedIndex == 3)
            {
                btnRun.IsEnabled = true;

                if (SessionInfo.NextTurnStep == 1)
                {
                    SessionInfo.NextTurnStep = -1;
                    server.SendMessage("NextStepRun");
                    //change the lamp and set it green
                    WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                    processor.SetLampStatus(0);
                }
                else if (SessionInfo.NextTurnStep == 99)
                {
                    SessionInfo.NextTurnStep = -1;
                    ExperimentRun();
                }
                // btnRecover.IsEnabled = false;
                //runSelect_listBox.IsEnabled = true;
            }
        }
        private void frameDeskTop_ContentRendered(object sender, EventArgs e)
        {
            DeskTop deskTop = (DeskTop)frameDeskTop.Content;
            deskTop.labelRotationName.Content = "";
            deskTop.NextStepEvent += new NextStepHandler(Button_Click_1);
        }

        private void frameRotation_ContentRendered(object sender, EventArgs e)
        {
            ConfigRotation configRotation = (ConfigRotation)frameRotation.Content;
            configRotation.NextStepEvent += new NextStepHandler(Button_Click_1);
        }

        private void btnNextRotation_Click(object sender, RoutedEventArgs e)
        {
            NextRotationEvent();
        }
        #region 2012-1-5 perry 修改运行
        private NameDpipesServer server; 
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (server != null)
                server.Dispose();
        }
        private string NextStepScanFinishedMessage;
        private bool NextStepScanFinishedFalg = false;
        private string NextStepScanEvent(string message)
        {
         //   if (message == "NextStepScanFinished") return string.Empty;
            NextStepScanFinishedFalg = false;
            server.SendMessage(message);
            NextStepScanFinishedMessage = "NextStepScanStart";
            server.start();
            DateTime dtime =DateTime.Now;
            while (DateTime.Now.Subtract(dtime).TotalMinutes < 15 && !NextStepScanFinishedFalg)
            {
                System.Threading.Thread.Sleep(1000);
                continue;
            }
            return NextStepScanFinishedMessage;
        }
        private void MessageReceived(string Message)
        {
            WanTai.Common.CommonFunction.WriteLog("MessageReceived-----MessageReceived====》" + Message);

            /**********开始第二轮扫描界面***************************************/
            if (Message.IndexOf("GoToScanPage")>=0)
            {
                this.Dispatcher.BeginInvoke(new onShowNextRotation(this.onShowNextRotationEvent), null); 
                return;
            }
           /**********第二轮扫描开始 1 成功 否则失败*************************************************************/
            if (NextStepScanFinishedMessage == "NextStepScanStart")
            {
                if (Message.IndexOf( "ScanFinished")>=0)
                    NextStepScanFinishedMessage = "NextStepScanFinished";
                else
                    NextStepScanFinishedMessage = "NextStepScanErr";
                NextStepScanFinishedFalg = true;
            }
            /********第二轮扫描完成，*********************************************/
            if (Message.IndexOf("ServerClose")>=0)
            {
                server.Dispose();
                //server.MessageReceived -= new NameDpipesServer.MessageReceivedHandler(MessageReceived);
                //server = null;
            }
        }
        private delegate void onShowNextRotation();
        private void onShowNextRotationEvent()
        {
            //WanTai.Common.CommonFunction.WriteLog("onShowNextRotationEvent");
            //
            //if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            //{
            //    this.Dispatcher.Invoke(new onShowNextRotation(this.onShowNextRotationEvent), null);
            //    return;
            //}
            //else
            //{
            SessionInfo.NextTurnStep = 0;
                WanTai.Common.CommonFunction.WriteLog("onShowNextRotationEvent-----else");
                //btnRun.IsEnabled = false;
                //btnRecover.IsEnabled = false;
                ////runSelect_listBox.IsEnabled = false;
                //btnStop.IsEnabled = false;
                //btnRStart.IsEnabled = false;
                // btnStart.Content = "启动";
                // this.bindRunWithStartAction();
                List<RotationInfo> rotations = new List<RotationInfo>();
                rotations = new ConfigRotationController().GetCurrentRotationInfos(SessionInfo.ExperimentID);
                if (rotations.Count == 0)
                {
                    MessageBox.Show("该实验没有建立轮次。", "系统提示");
                    return;
                }

                if (SessionInfo.PraperRotation == null)
                    SessionInfo.PraperRotation = rotations.FirstOrDefault();
                else
                {
                    foreach (RotationInfo rotation in rotations)
                    {
                        if (rotation.RotationSequence == SessionInfo.PraperRotation.RotationSequence + 1)
                        {
                            SessionInfo.PraperRotation = rotation;
                            break;
                        }
                    }
                }

                tabControl.SelectedIndex = 0;
                ((TabItem)tabControl.Items[2]).IsEnabled = false;

                TubesView tubesView1 = new TubesView();
                tabItem1.Content = tubesView1;
                tubesView1.onNextStepScan += new NextStepScan(NextStepScanEvent);
                tubesView1.NextStepEvent += new NextStepHandler(Button_Click_1);
               
                //open the lamp and set it green flashing
                WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                processor.SetLampStatus(2);
                tabControl.SelectedIndex = 0;
                MessageBox.Show("请为下一轮实验准备样品!", "系统提示！", MessageBoxButton.OK);
                tabControl.SelectedIndex = 0;
                //change the lamp and set it green
                processor.SetLampStatus(0);
            //}
        }
        private void NextRotationEvent()
        {
            server.start();
        }
        private void _NextRotationEvent()
        {
            btnRun.IsEnabled = false;
            btnRecover.IsEnabled = false;
            //runSelect_listBox.IsEnabled = false;
            btnStop.IsEnabled = false;
            //btnRStart.IsEnabled = false;
           // btnStart.Content = "启动";
            this.bindRunWithStartAction();
            List<RotationInfo> rotations = new List<RotationInfo>();
            rotations = new ConfigRotationController().GetCurrentRotationInfos(SessionInfo.ExperimentID);
            if (rotations.Count == 0)
            {
                MessageBox.Show("该实验没有建立轮次。", "系统提示");
                return;
            }

            if (SessionInfo.PraperRotation == null)
                SessionInfo.PraperRotation = rotations.FirstOrDefault();
            else
            {
                foreach (RotationInfo rotation in rotations)
                {
                    if (rotation.RotationSequence == SessionInfo.PraperRotation.RotationSequence+1)
                    {
                        SessionInfo.PraperRotation = rotation;
                        break;
                    }
                }
            }
            tabControl.SelectedIndex = 0;
            ((TabItem)tabControl.Items[2]).IsEnabled = false;
            TubesView tubesView1 = new TubesView();
            tabItem1.Content = tubesView1;
            tubesView1.NextStepEvent += new NextStepHandler(Button_Click_1);
            SessionInfo.NextTurnStep = 99;
        }
        #endregion
        private void PCRResultImport_button_Click(object sender, RoutedEventArgs e)
        {
            ImportPCRTestResultFile importPcrFile = new ImportPCRTestResultFile();
            importPcrFile.ShowDialog();
		}
		
		private void PCRResultView_button_Click(object sender, RoutedEventArgs e)
        {
            PCRTestResultViewList pcrView = new PCRTestResultViewList();
            pcrView.ShowDialog();
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            tabControl.SelectedIndex = 3;
            experimentRunView.Stop();
            this.bindRunWithStartAction();
            //runSelect_listBox.IsEnabled = false;    
            btnRecover.IsEnabled = false;
            btnRun.IsEnabled = false;
        }

        private void bindRunWithStartAction()
        {
            //List<RunActionItem> runActionList = new List<RunActionItem>();
            //runActionList.Add(new RunActionItem() { ActionName = "Run", ActionValue = "run" });
            //runSelect_listBox.ItemsSource = runActionList;
            //runSelect_listBox.SelectedIndex = 0;
            labRun.Content = "运行";
            labRun.Uid = "run";
            imageRun.Source = new BitmapImage(new Uri("/WanTag;component/Resources/Star.png", UriKind.Relative));
        }

        private void bindRunWithRestarAction()
        {

            //List<RunActionItem> runActionList = new List<RunActionItem>();
            //runActionList.Add(new RunActionItem() { ActionName = "Run", ActionValue = "run11" });
            //runActionList.Add(new RunActionItem() { ActionName = "Recover", ActionValue = "recover" });
            //runSelect_listBox.ItemsSource = runActionList;
            //runSelect_listBox.SelectedIndex = 0;
            labRun.Content = "运行";
            labRun.Uid = "run11";
            imageRun.Source = new BitmapImage(new Uri("/WanTag;component/Resources/Star.png", UriKind.Relative));
            btnRun.IsEnabled = true;
            btnRecover.IsEnabled = true;
        }

        private void bindRunWithPauseAction()
        {
            //List<RunActionItem> runActionList = new List<RunActionItem>();
            //runActionList.Add(new RunActionItem() { ActionName = "Pause", ActionValue = "pause" });
            //runSelect_listBox.ItemsSource = runActionList;
            //runSelect_listBox.SelectedIndex = 0;
            labRun.Content = "暂停";
            labRun.Uid = "pause";
            imageRun.Source = new BitmapImage(new Uri("/WanTag;component/Resources/Parse.gif", UriKind.Relative));
        }
        private void bindRunWithContinueAction()
        {
            //List<RunActionItem> runActionList = new List<RunActionItem>();
            //runActionList.Add(new RunActionItem() { ActionName = "Continue", ActionValue = "continue" });
            //runSelect_listBox.ItemsSource = runActionList;
            //runSelect_listBox.SelectedIndex = 0;
            labRun.Uid = "continue";
            labRun.Content = "继续";
            imageRun.Source = new BitmapImage(new Uri("/WanTag;component/Resources/Star.png", UriKind.Relative));
        }
        private void ExperimentRun(){
            if (SessionInfo.ExperimentID==Guid.Empty || new RotationInfoController().GetExperimentRotation(SessionInfo.ExperimentID).Count == 0)
                return;
            tabControl.SelectedIndex = 3;
            //btnStop.IsEnabled = true;
            //if (runSelect_listBox.SelectedValue.ToString() == "run")
            if (labRun.Uid == "run")
            {
                btnStop.IsEnabled = true;
                bool startResult = experimentRunView.Start(ExperimentRunStatus.Start);
                if (!startResult)
                    return;

                this.bindRunWithPauseAction();
                return;
            }
            if (labRun.Uid == "pause")
            //if (runSelect_listBox.SelectedValue.ToString() == "pause")
            {
                btnStop.IsEnabled = false;
                if (experimentRunView.Pause())
                    this.bindRunWithContinueAction();
                return;
            }
           // if (runSelect_listBox.SelectedValue.ToString() == "continue")
            if (labRun.Uid == "continue")
            {
                btnStop.IsEnabled = true;
                bool startResult = experimentRunView.Start(ExperimentRunStatus.Continue);
                if (!startResult)
                    return;

                this.bindRunWithPauseAction();
                return;
            }
         //   if (runSelect_listBox.SelectedValue.ToString() == "recover")
            if (labRun.Uid == "recover")
            {
                btnStop.IsEnabled = true;
                bool startResult = experimentRunView.Start(ExperimentRunStatus.Recover);
                if (!startResult)
                    return;

                this.bindRunWithPauseAction();
                return;
            }
            //if (runSelect_listBox.SelectedValue.ToString() == "run11")
            if (labRun.Uid == "run11")
            {
                btnStop.IsEnabled = true;
                btnRecover.IsEnabled = true;
                bool startResult = experimentRunView.Start(ExperimentRunStatus.ReStart);
                if (!startResult)
                    return;

                this.bindRunWithPauseAction();
                return;
            }
            //runSelect_listBox.SelectedIndex = 0;
        }
        private void btnRecover_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = true;
            bool startResult = experimentRunView.Start(ExperimentRunStatus.Recover);
            if (!startResult)
                return;
            btnRecover.IsEnabled = false;
            this.bindRunWithPauseAction();
            btnRun.IsOpen = false;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            ExperimentRun();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            server = new NameDpipesServer();
            server.MessageReceived += new NameDpipesServer.MessageReceivedHandler(MessageReceived);
        }
    }

    public class RunActionItem
    {
        private string actionName;
        public string ActionName
        {
            get { return actionName; }
            set
            {
                actionName = value;   
            }
        }
        public string ActionValue { get; set; }
    }
}
