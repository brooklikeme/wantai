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
using WanTai.DataModel;
using System.IO;
using System.Windows.Markup;
using System.Threading;
using System.ComponentModel;
using WanTai.Controller;
using System.Windows.Media.Animation;
using WanTai.View.Services;
using WanTai.Controller.Configuration;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for ExperimentRunView.xaml
    /// </summary>
    public partial class ExperimentRunView : UserControl
    {
        public ExperimentRunView()
        {
            InitializeComponent();
            if (SessionInfo.ResumeExecution)
            {
                CurrentRotation = SessionInfo.CurrentRotation;
                NexRotation = SessionInfo.NexRotation;
            }
        }        
        private Guid ExperimentID { get { return SessionInfo.ExperimentID; } }
        private List<RotationInfo> _ExperimentRotation;
        private List<RotationInfo> ExperimentRotation {
            get { 
               if(_ExperimentRotation==null)
                   _ExperimentRotation=new WanTai.Controller.RotationInfoController().GetExperimentRotation(ExperimentID);
               return _ExperimentRotation;
            }
            set {_ExperimentRotation=value;}
        }
        private List<OperationConfiguration> _OperationConfigurations;
        private List<OperationConfiguration> OperationConfigurations{
            get { 
               if(_OperationConfigurations==null)
                   _OperationConfigurations = new WanTai.Controller.OperationController().GetAllOperations();
               return _OperationConfigurations;
            }
        }
        private RotationInfo CurrentRotation { get; set; }
        private RotationInfo FinishedRotation;
        private bool NexRotation { get; set; }
        private Dictionary<Guid, Dictionary<string, object>> rotationFormulaParameters = new Dictionary<Guid, Dictionary<string, object>>();
        private int SubOperation(Guid OperationID, int RunTime, int OperationSequence, ref List<OperationConfiguration> Operations)
        {
            OperationConfiguration  Operation=  OperationConfigurations.Where(operation => operation.OperationID == OperationID).FirstOrDefault();
            if (Operation == null) return RunTime;

            foreach (string operationID in Operation.SubOperationIDs.Split(','))
            {
                if (string.IsNullOrEmpty(operationID)) continue;
                OperationConfiguration SubOperationConfiguration = OperationConfigurations.Where(operation => operation.OperationID == new Guid(operationID)).FirstOrDefault();
                if (SubOperationConfiguration == null) continue;
                if (!string.IsNullOrEmpty(SubOperationConfiguration.ScriptFileName))
                  Operations.Add(SubOperationConfiguration);
                if (SubOperationConfiguration.OperationType == 0)
                {
                    if (OperationSequence < SubOperationConfiguration.OperationSequence)
                       RunTime += (int)SubOperationConfiguration.RunTime;
                    OperationSequence = SubOperationConfiguration.OperationSequence;
                }
                else
                    RunTime = SubOperation(new Guid(operationID), RunTime, OperationSequence, ref Operations);
          
            }
            return RunTime;
        }
        private void AddSubOperationChildren(int RunIndex,ref int ColumnIndex,Guid OperationID,ref int OperationSequence, List<OperationConfiguration> Operations,int TotalRuntime)
        {
            OperationConfiguration Operation = OperationConfigurations.Where(operation => operation.OperationID == OperationID).FirstOrDefault();
            if (Operation == null) return ;
            if (TotalRuntime == 0) TotalRuntime = (int)Operation.RunTime;
            if (Operation.OperationType == (short)OperationType.Single)
            {
                if (!string.IsNullOrEmpty(Operation.ScriptFileName) && OperationSequence < Operation.OperationSequence)
                    Operations.Add(Operation);
                if (Operation.OperationType == (short)OperationType.Single)
                {
                    if (OperationSequence < Operation.OperationSequence)
                    {
                        AddOperationChildren(Operation, RunIndex, TotalRuntime, ref ColumnIndex);
                        OperationSequence = Operation.OperationSequence;
                    }
                }
            }
            else
            {
                foreach (string operationID in Operation.SubOperationIDs.Split(','))
                {
                    if (string.IsNullOrEmpty(operationID)) continue;
                    OperationConfiguration SubOperationConfiguration = OperationConfigurations.Where(operation => operation.OperationID == new Guid(operationID)).FirstOrDefault();
                    if (SubOperationConfiguration == null) continue;
                    if (!string.IsNullOrEmpty(SubOperationConfiguration.ScriptFileName) && OperationSequence < SubOperationConfiguration.OperationSequence)
                        Operations.Add(SubOperationConfiguration);
                    if (SubOperationConfiguration.OperationType == (short)OperationType.Single)
                    {
                        if (OperationSequence < SubOperationConfiguration.OperationSequence)
                        {
                            AddOperationChildren(SubOperationConfiguration, RunIndex, TotalRuntime, ref ColumnIndex);
                            OperationSequence = SubOperationConfiguration.OperationSequence;
                        }
                    }
                    else
                        AddSubOperationChildren(RunIndex, ref ColumnIndex, new Guid(operationID), ref OperationSequence, Operations, TotalRuntime);

                }
            }
        }
        private void AddOperationChildren(OperationConfiguration SubOperationConfiguration, int RunIndex, int TotalRuntime, ref int ColumnIndex)
        {
            Grid grid = stackPanel.FindName("grid" + RunIndex.ToString()) as Grid;
            if (grid != null)
            {
                ProgressBar progressBar = new ProgressBar();
                // progressBar.Name = CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID.ToString();
              //  float WidthScale = (float)SubOperationConfiguration.RunTime / (float)TotalRuntime;
               float WidthScale = (float)SubOperationConfiguration.RunTime;
                progressBar.Uid = WidthScale.ToString();
                if (Resources.Contains("ProgressBarStyle" + SubOperationConfiguration.StyleIndex.ToString()))
                    progressBar.Style = Resources["ProgressBarStyle" + SubOperationConfiguration.StyleIndex.ToString()] as Style;
                else if (Resources.Contains("ProgressBarStyle" + (SubOperationConfiguration.StyleIndex % 5 == 0 ? 5 : SubOperationConfiguration.StyleIndex % 5).ToString()))
                    progressBar.Style = Resources["ProgressBarStyle" + (SubOperationConfiguration.StyleIndex % 5 == 0 ? 5 : SubOperationConfiguration.StyleIndex % 5).ToString()] as Style;

                else
                    progressBar.Style = Resources["ProgressBarStyle1"] as Style;
                // progressBar.Width = (stackPanel.ActualWidth -120) * WidthScale;
                progressBar.Value = 0;
                progressBar.Maximum = (int)SubOperationConfiguration.RunTime * 60;
                progressBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(WidthScale, GridUnitType.Star) });
                grid.Children.Add(progressBar);
                progressBar.SetValue(Grid.RowProperty, RunIndex - 1);
                progressBar.SetValue(Grid.ColumnProperty, ColumnIndex);
               // string ProgressBarName = "ProgressBar" + RunIndex.ToString() + SubOperationConfiguration.OperationType.ToString() + SubOperationConfiguration.OperationSequence.ToString();
                string ProgressBarName = "ProgressBar" + RunIndex.ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", "");
                if (grid.FindName(ProgressBarName) != null)
                    grid.UnregisterName(ProgressBarName);
                grid.RegisterName(ProgressBarName, progressBar);                
                StackPanel sPanel = new StackPanel();
                sPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                sPanel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                sPanel.Orientation = Orientation.Vertical;
                Label labOperationName = new Label();
                labOperationName.Content = SubOperationConfiguration.OperationName;
                labOperationName.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                labOperationName.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                sPanel.Children.Add(labOperationName);
               // grid.Children.Add(labOperationName);
                
                //labOperationName.SetValue(Grid.RowProperty, RunIndex - 1);
                //labOperationName.SetValue(Grid.ColumnProperty, ColumnIndex);

                Label lab = new Label();
                lab.Content = "00:00:00";
                lab.Uid = "0";
                lab.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                lab.VerticalAlignment = System.Windows.VerticalAlignment.Center;
              //  grid.Children.Add(lab);
                sPanel.Children.Add(lab);
                grid.Children.Add(sPanel);
                sPanel.SetValue(Grid.RowProperty, RunIndex - 1);
                sPanel.SetValue(Grid.ColumnProperty, ColumnIndex++);
                //string LableName = "lab" + RunIndex.ToString() + SubOperationConfiguration.OperationType.ToString() + SubOperationConfiguration.OperationSequence.ToString();
                string LableName = "lab" + RunIndex.ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", ""); ;
                if (grid.FindName(LableName) != null)
                    grid.UnregisterName(LableName);
                grid.RegisterName(LableName, lab);
            }
        }
        public void BindView()
        {
            btnAddReagent.IsEnabled = true;
            btnTrash.IsEnabled = false;
            _PCRHeatLiquidPlate = null;
            _ExperimentReagentAndSupply = null;
            _TestingItem = null;
            _ExperimentRotation = null;
            _OperationConfigurations = null;
            labRotation.FontSize = 10;
            labRotation.Content = "共" + ExperimentRotation.Count.ToString() + "个轮次";
            spOperationView.Children.Clear();
            var SingleOperations = OperationConfigurations.Where(Operation => Operation.OperationType == 0).OrderBy(Operation => Operation.OperationSequence);
            int OperationStyleIndex = 1;
            foreach (OperationConfiguration Operation in SingleOperations)
            {
                Label labOperationName = new Label();
                labOperationName.FontSize = 10;
                labOperationName.Content = Operation.OperationName;
                labOperationName.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                labOperationName.Margin = new Thickness(4, 0, 0, 0);
                spOperationView.Children.Add(labOperationName);

                Label labOperationLegend = new Label();
                labOperationLegend.Content = Operation.RunTime.ToString();
                labOperationLegend.FontSize = 10;
                labOperationLegend.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                labOperationLegend.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                labOperationLegend.Height = 24;
                labOperationLegend.Width = 20;
                if (Resources.Contains("Operation" + OperationStyleIndex.ToString()))
                    labOperationLegend.Background = Resources["Operation" + OperationStyleIndex.ToString()] as LinearGradientBrush;
                else if (Resources.Contains("Operation" + (OperationStyleIndex % 5 == 0 ? 5 :OperationStyleIndex % 5).ToString()))
                    labOperationLegend.Background = Resources["Operation" + (OperationStyleIndex % 5 == 0 ? 5 : OperationStyleIndex % 5).ToString()] as LinearGradientBrush;
                else
                    labOperationLegend.Background = Resources["Operation1"] as LinearGradientBrush;

                Operation.StyleIndex = OperationStyleIndex++;
                labOperationLegend.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                labOperationLegend.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                spOperationView.Children.Add(labOperationLegend);
            }
            int RunIndex = 0;
            if (!SessionInfo.ResumeExecution)
            {
                CurrentRotation = null;
                NexRotation = false;
            }
            if (stackPanel.Children.Count > 0)
                stackPanel.Children.Clear();
           // StackPanel stackPanel = new StackPanel();
            foreach (RotationInfo Rotation in ExperimentRotation)
            {
              //  if (RunIndex > 0)
                    stackPanel.Children.Add(new Border() { BorderBrush = new SolidColorBrush(Colors.Silver), Height = 20, BorderThickness = new Thickness(0, 2, 0, 2) });
                Rotation.RunIndex = RunIndex++;
                if (!NexRotation && CurrentRotation != null && (Rotation.State == (short)RotationInfoStatus.Create || Rotation.State == (short)RotationInfoStatus.Suspend))
                    NexRotation = true;

                if (CurrentRotation == null && (Rotation.State == (short)RotationInfoStatus.Create || Rotation.State == (short)RotationInfoStatus.Suspend || Rotation.State == (short)RotationInfoStatus.Fail))
                    CurrentRotation = Rotation;
                Grid grid = new Grid();
                grid.Background = new SolidColorBrush(Colors.Cornsilk);
                grid.Height = 60;
                grid.RowDefinitions.Add(new RowDefinition() { Height =new GridLength(60) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });

                //StackPanel panel = new StackPanel();
                //// panel.Name = "panel"+RunIndex.ToString();
                //panel.Background = new SolidColorBrush(Colors.Cornsilk);
                //panel.Orientation = Orientation.Horizontal;
                //panel.Height = 60;
                //panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                Button btn = new Button();
                btn.Content = Rotation.RotationName;
                //btn.Width = 120;
                btn.Uid = Rotation.RotationID.ToString();
                btn.Click += new RoutedEventHandler(delegate(object sender, RoutedEventArgs e)
                {
                    object BatchID = new RotationController().GetRotationIsBatch(new Guid((sender as Button).Uid));
                    if (BatchID != null)
                    {
                        TubesDetailView detailView = new TubesDetailView();
                        // detailView.BatchID = (Guid)BatchID;
                        detailView.ViewExperimentBatch((Guid)BatchID, SessionInfo.BatchTimes);
                        detailView.ShowDialog();
                    }
                });
                grid.Children.Add(btn);
                btn.SetValue(Grid.RowProperty,RunIndex-1);
                btn.SetValue(Grid.ColumnProperty, 0);
                stackPanel.Children.Add(grid);
                if (stackPanel.FindName("grid" + RunIndex.ToString()) != null)
                    stackPanel.UnregisterName("grid" + RunIndex.ToString());
                stackPanel.RegisterName("grid" + RunIndex.ToString(), grid);
                //List<OperationConfiguration> Operations = new List<OperationConfiguration>();
                int ColumnIndex = 1;
                int OperationSequence = 0;
                AddSubOperationChildren(RunIndex, ref ColumnIndex, Rotation.OperationID, ref OperationSequence, Rotation.Operations=new List<OperationConfiguration>(), 0);
            }
            stackPanel.Children.Add(new Border() { BorderBrush = new SolidColorBrush(Colors.Silver),  BorderThickness = new Thickness(0, 2, 0, 0) });
            //RotationRunView.Child = stackPanel;
        }
        private List<TestingItemConfiguration> _TestingItem;
        private List<TestingItemConfiguration> TestingItem
        {
            get
            {
                if (_TestingItem == null)
                    _TestingItem = new TestItemController().GetTestItemConfigurations().ToList();
                return _TestingItem;
            }
        }
        private Dictionary<string, string> lVariables { get; set; }
        private DateTime ExperimentStartTime;
        private BackgroundWorker worker;
        private RotationInfoStatus RunFalg = RotationInfoStatus.Processing;//10运行 15暂停 20完成 30停止
        public void Stop()
        {
            SessionInfo.CurrentExperimentsInfo.State = (short)ExperimentStatus.Stop;
            WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
            processor.StopScript();
            ///todo: need to update database
            RunFalg = RotationInfoStatus.Stop;
            RotationInfoController controller = new RotationInfoController();
            controller.UpdataRotationOperateStatus(CurrentRotation.CurrentRotationOperate, RunFalg);
            controller.UpdataRotationStatus(CurrentRotation.RotationID, RunFalg);
            controller.UpdataExperimentStatus(ExperimentID, true, (ExperimentStatus)((int)RunFalg));
            worker.CancelAsync();
            btnAddReagent.IsEnabled = false;
        }

        public bool Pause()
        {
            SessionInfo.CurrentExperimentsInfo.State = (short)ExperimentStatus.Suspend;
            WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
            if (processor.PauseScript())
            {
                RunFalg = RotationInfoStatus.Suspend;
                new RotationInfoController().UpdataRotationOperateStatus(CurrentRotation.CurrentRotationOperate, RunFalg);
                worker.CancelAsync();
                return true;
            }
            else
            {
                MessageBox.Show("当前状态无法暂停！", "系统提示！");
                return false;
            }
        }
        private string RunScrtiptName = "";
        private DateTime RunScriptTime = DateTime.Now;
        private string EvoOutputPath =WanTai.Common.Configuration.GetEvoOutputPath();
        private string EvoVariableOutputPath=WanTai.Common.Configuration.GetEvoVariableOutputPath();
        public event MainPage.ExperimentRunViewHandler ExperimentRunViewEvent;
        public event MainPage.NewExperimentHandler NewExperimentEvent;
        public event MainPage.ExperimentRunErrHandler ExperimentRunErrEvent;
        public event MainPage.ExperimentRunRutrnHandler ExperimentRunRutrnEvent;
        public event MainPage.EvoRestorationStatus SetEvoRestorationStatus;
        public event MainPage.SuspendOkHandler SuspendOkEvent;
        private void WriteTimesCSV(int Value)
        {
            using (FileStream DWFile = new FileStream(EvoVariableOutputPath + "Times.CSV", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                {
                    DWStreamWriter.WriteLine("NextMix_Times");
                    DWStreamWriter.WriteLine(Value.ToString());
                }
            }
        }

        public event WanTai.View.MainPage.SendNextStepRunMsg NextStepRunEvent;

        /// <summary>
        /// falg 0 启动 1 暂停 2 继续(执行完剩余脚本才结束方法) 10 出错后继续 11出错后重新启动
        /// 混样脚本  (提取+PCR配液+混样)脚本  样品分装脚本  三个脚本
        /// </summary>
        /// <param name="falg"></param>
        public bool Start(ExperimentRunStatus falg)
        {
            btnTrash.IsEnabled = false;
            RotationInfoController rotationInfoController = new RotationInfoController();
            if (CurrentRotation == null)
            {
                MessageBox.Show("当前没有轮次可以运行！", "系统提示");
                if (ExperimentRunRutrnEvent != null)
                    ExperimentRunRutrnEvent();
                return false;
            }

            if (CurrentRotation.RunIndex == 0 && CurrentRotation.CurrentOperationIndex == 0)
            {
                WriteTimesCSV(NexRotation ? 1 : 0);
            }
       
            #region 2012-1-5 perry 改流程时修改


            #endregion
            #region 修改参数文件
            if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Grouping && CurrentRotation.RunIndex > 0 && falg == ExperimentRunStatus.Start)
            {

                string SampleNumberFileName = WanTai.Common.Configuration.GetEvoVariableOutputPath() + ExperimentRotation[CurrentRotation.RunIndex].RotationID.ToString() + WanTai.Common.Configuration.GetSampleNumberFileName();
                if (File.Exists(WanTai.Common.Configuration.GetEvoVariableOutputPath() + WanTai.Common.Configuration.GetSampleNumberFileName()))
                    File.Delete(WanTai.Common.Configuration.GetEvoVariableOutputPath() + WanTai.Common.Configuration.GetSampleNumberFileName());
                if (File.Exists(SampleNumberFileName))
                    File.Move(SampleNumberFileName, WanTai.Common.Configuration.GetEvoVariableOutputPath() + WanTai.Common.Configuration.GetSampleNumberFileName());

                string MixSampleNumberFileName = WanTai.Common.Configuration.GetEvoVariableOutputPath() + ExperimentRotation[CurrentRotation.RunIndex].RotationID.ToString() + WanTai.Common.Configuration.GetMixSampleNumberFileName();
                if (File.Exists(WanTai.Common.Configuration.GetEvoVariableOutputPath() + WanTai.Common.Configuration.GetMixSampleNumberFileName()))
                    File.Delete(WanTai.Common.Configuration.GetEvoVariableOutputPath() + WanTai.Common.Configuration.GetMixSampleNumberFileName());
                if (File.Exists(MixSampleNumberFileName))
                    File.Move(MixSampleNumberFileName, WanTai.Common.Configuration.GetEvoVariableOutputPath() + WanTai.Common.Configuration.GetMixSampleNumberFileName());
                string CSVPath = WanTai.Common.Configuration.GetWorkListFilePath();

                foreach (TestingItemConfiguration Testing in TestingItem)
                {
                    if (File.Exists(CSVPath + Testing.WorkListFileName) && !(SessionInfo.BatchTimes > 1 && int.Parse(SessionInfo.BatchType) == SessionInfo.BatchTimes))
                    {
                        File.Delete(CSVPath + Testing.WorkListFileName);
                    }

                    if (File.Exists(CSVPath + ExperimentRotation[CurrentRotation.RunIndex].RotationID.ToString() + Testing.WorkListFileName))
                    {
                        if (File.Exists(CSVPath + Testing.WorkListFileName))
                        {
                            File.Delete(CSVPath + Testing.WorkListFileName);
                        };
                        File.Move(CSVPath + ExperimentRotation[CurrentRotation.RunIndex].RotationID.ToString() + Testing.WorkListFileName,
                            CSVPath + Testing.WorkListFileName);
                    }
                }
            }
            #endregion
            SessionInfo.CurrentExperimentsInfo.State = (short)ExperimentStatus.Processing;
            #region 运行脚本
            ExperimentStartTime = WanTai.Controller.EVO.ProcessorFactory.GetDateTimeNow();
            bool RunReturnValue = false;
            RunFalg = RotationInfoStatus.Processing;

            bool isParseProgress = false;
            string errorMessage = null;
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state)
            {
                CurrentRotation.CurrentRotationOperate = new RotationOperate();
                CurrentRotation.CurrentRotationOperate.StartTime = DateTime.Now;
                CurrentRotation.CurrentRotationOperate.RotationID = CurrentRotation.RotationID;
                CurrentRotation.CurrentRotationOperate.ExperimentID = ExperimentID;
                CurrentRotation.CurrentRotationOperate.OperationID = CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID;
                if (SetEvoRestorationStatus != null)
                {
                    SetEvoRestorationStatus(false);
                }
                WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                processor.onReceiveError += new Controller.EVO.IProcessor.OnEvoError(delegate { isParseProgress = true; });
                processor.onSendErrorResponse += new Controller.EVO.IProcessor.OnEvoError(delegate { isParseProgress = false; });
                #region 2012-1-5 perry 修改运行
                
                if (!processor.isOnNextTurnStep())
                  processor.onOnNextTurnStepHandler += new Controller.EVO.IProcessor.OnNextTurnStep(NextTurnStep);
                #endregion 
                bool ContinueFalg = false;
                if (falg == ExperimentRunStatus.Continue)
                    ContinueFalg = true;
            RunScript:
                if (falg == ExperimentRunStatus.Start || falg == ExperimentRunStatus.Recover || falg == ExperimentRunStatus.ReStart)
                {
                    CurrentRotation.CurrentRotationOperate.State = (short)RotationInfoStatus.Processing;  
                    List<OperationConfiguration> RunOperation = CurrentRotation.Operations;
                    if (!ContinueFalg)
                    {
                        //  List<OperationConfiguration> RunOperation = OperationConfigurations.Where(Operation => !string.IsNullOrEmpty(Operation.ScriptFileName)).OrderBy(Operation => Operation.OperationSequence).ToList();
                        if (CurrentRotation.RunIndex == 0 && CurrentRotation.CurrentOperationIndex == 0)
                        {
                            //first run
                            rotationInfoController.UpdataExperimentStatus(ExperimentID, false, ExperimentStatus.Processing, CurrentRotation.RotationID, RotationInfoStatus.Processing);
                            //rotationInfoController.UpdataRotationStatus(CurrentRotation.RotationID, RotationInfoStatus.Processing);
                        }
                        rotationInfoController.UpdataRotationOperateStatus(CurrentRotation.CurrentRotationOperate, RotationInfoStatus.Processing);
                    }
                    ContinueFalg = false;
                    bool hasAddSameSequenceVolume = false;
                    /*****一个操作有多个脚本运行，**************************/
                    foreach (string scriptFileName in RunOperation[CurrentRotation.CurrentOperationIndex].ScriptFileName.Split(','))
                    {
                        try
                        {
                            WanTai.Common.CommonFunction.WriteLog(RunFalg.ToString() + ";ExperimentName:" + SessionInfo.CurrentExperimentsInfo.ExperimentName + "RunIndex:" + CurrentRotation.RunIndex.ToString() + ";Script--" + scriptFileName);
                            WanTai.Common.CommonFunction.WriteLog("RunOperationCount:" + RunOperation.Count() + ";CurrentOperationCount:" + CurrentRotation.Operations.Count() + ";CurrentOperationIndex:" + CurrentRotation.CurrentOperationIndex);

                            // limit index error
                            if (CurrentRotation.CurrentOperationIndex >= CurrentRotation.Operations.Count())
                            {
                                MessageBox.Show("数组指针越界，请将日志保存下来联系管理员!", "系统提示");
                                WanTai.Common.CommonFunction.WriteLog("数组指针越界，请将日志保存下来联系管理员!");
                                continue;
                            }

                            //
                            DateTime scriptStartTime =RunScriptTime= ExperimentStartTime = WanTai.Controller.EVO.ProcessorFactory.GetDateTimeNow();
                            WanTai.Common.CommonFunction.WriteLog("Debug point 1");
                            if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].IsExistsRunScript == null)
                                CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].IsExistsRunScript = string.Empty;
                            if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].IsExistsRunScript.IndexOf(CurrentRotation.RotationID.ToString() + scriptFileName) > -1)
                            {
                                continue;
                            }
                            bool isFirstStepMixing = false;
                            if (CurrentRotation.RunIndex == 0 && CurrentRotation.CurrentOperationIndex == 0)
                            {
                                isFirstStepMixing = true;
                                if (SessionInfo.FirstStepMixing == 3) {
                                    // send message to namedpipe client
                                    if (NextStepRunEvent != null)
                                        NextStepRunEvent();
                                    while (SessionInfo.FirstStepMixing == 3)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    RunReturnValue = SessionInfo.FirstStepMixing == 5;
                                }
                            }
                            /******Stop Suspend 都返回 True,只有错才返回 False******************************/
                            bool canRecover = false;
                            if (falg == ExperimentRunStatus.Recover)
                            {
                                short errotLineNumber = 0;
                                canRecover = processor.CheckCanRecover(scriptFileName, out errotLineNumber);
                                if (canRecover)
                                {
                                    SessionInfo.RuningScriptName = scriptFileName;
                                    RunReturnValue = processor.RecoverScript((RunScrtiptName = scriptFileName), errotLineNumber);
                                    falg = ExperimentRunStatus.Start;                                    
                                }
                            }

                            if (!canRecover && !isFirstStepMixing)
                            {
                                SessionInfo.RuningScriptName = scriptFileName;
                                RunReturnValue = processor.StartScript((RunScrtiptName = scriptFileName));
                            }

                            if (RunFalg == RotationInfoStatus.Stop || RunFalg == RotationInfoStatus.Suspend || RunFalg == RotationInfoStatus.Fail)
                            {
                                WanTai.Common.CommonFunction.WriteLog(RunFalg.ToString() + ";Script--" + scriptFileName);
                                return;
                            }
                            WanTai.Common.CommonFunction.WriteLog("Debug point 2");
                            WanTai.Common.CommonFunction.WriteLog(RunReturnValue.ToString()+";"+RunFalg.ToString() + ";Script--" + scriptFileName);
                            if (RunReturnValue)
                            {
                              //  RunFalg = RotationInfoStatus.Finish;
                                /*****一个操作有多个脚本，运行完的脚本要记录，防止暂停后，继续是再次运行已运行过的脚本****************************/
                                CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].IsExistsRunScript += CurrentRotation.RotationID.ToString() + scriptFileName + ";";
                                try
                                {
                                    if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Single)
                                    {
                                        ///SampleTrackingController perrywang
                                        SampleTrackingController.SampleTracking(ExperimentID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, CurrentRotation.RotationID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence, new Guid(), scriptStartTime, false);
                                    }
                                    else
                                    {
                                        ///SampleTrackingController perrywang
                                        SampleTrackingController.SampleTracking(ExperimentID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, CurrentRotation.RotationID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence, (NexRotation ? ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID : new Guid()), scriptStartTime, true);
                                    }

                                    Thread.Sleep(5000);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Save sample  tracking error: " + e.Message, "系统提示");
                                    LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, e.Message + Environment.NewLine + e.StackTrace, SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
                                }
                                WanTai.Common.CommonFunction.WriteLog("Debug point 3");
                                if (!hasAddSameSequenceVolume && CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Grouping)
                                {
                                    Guid subOpertionWithSameEquence = new OperationController().GetSameSequenceSubOperation(CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID);
                                    if (subOpertionWithSameEquence != Guid.Empty && SessionInfo.RotationFormulaParameters.ContainsKey(CurrentRotation.RotationID))
                                        new Services.ReagentDetectService().AddSuppliesConsumption(CurrentRotation.RotationID, subOpertionWithSameEquence, SessionInfo.RotationFormulaParameters[CurrentRotation.RotationID]);
                                    hasAddSameSequenceVolume = true;
                                }
                            }
                            else
                            {
                                RunFalg = RotationInfoStatus.Fail;
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            errorMessage = e.Message;
                            MessageBox.Show("run script error：" + e.Message, "系统提示");
                            WanTai.Common.CommonFunction.WriteLog(e.Message);
                            WanTai.Common.CommonFunction.WriteLog(e.StackTrace);
                            RunFalg = RotationInfoStatus.Fail;                            
                            return;
                        }
                        Thread.Sleep(10000);
                    }
                }

                if (falg == ExperimentRunStatus.Continue)
                {
                    CurrentRotation.CurrentRotationOperate.State = (short)RotationInfoStatus.Processing;
                    rotationInfoController.UpdataRotationOperateStatus(CurrentRotation.CurrentRotationOperate, RotationInfoStatus.Processing);
                    
                    try
                    {
                        WanTai.Common.CommonFunction.WriteLog(RunReturnValue.ToString() + ";" + RunFalg.ToString() + ";Continue Start; Script --------" + RunScrtiptName+"  RotationID---" +CurrentRotation.RotationID.ToString());
                        RunReturnValue = processor.ResumeScript();
                        if (RunFalg == RotationInfoStatus.Stop || RunFalg == RotationInfoStatus.Suspend || RunFalg == RotationInfoStatus.Fail)
                        {
                            WanTai.Common.CommonFunction.WriteLog(RunFalg.ToString() + ";Script--" + RunScrtiptName);
                            return;
                        }
                        if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].IsExistsRunScript == null)
                            CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].IsExistsRunScript = string.Empty;

                        CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].IsExistsRunScript += CurrentRotation.RotationID.ToString() + RunScrtiptName + ";";
                        try
                        {
                            if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Single)
                            {
                                ///SampleTrackingController perrywang
                                SampleTrackingController.SampleTracking(ExperimentID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, CurrentRotation.RotationID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence, new Guid(), RunScriptTime, false);
                            }
                            else
                            {
                                ///SampleTrackingController perrywang
                                SampleTrackingController.SampleTracking(ExperimentID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, CurrentRotation.RotationID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence, (NexRotation ? ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID : new Guid()), RunScriptTime, true);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Save sample  tracking error: " + e.Message, "系统提示");
                            LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, e.Message + Environment.NewLine + e.StackTrace, SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
                        }
                      
                    }
                    catch (Exception e)
                    {
                        errorMessage = e.Message;
                        MessageBox.Show("resume script error" + e.Message, "系统提示");
                        RunFalg = RotationInfoStatus.Fail;
                        return;

                        //throw;
                    }
                    WanTai.Common.CommonFunction.WriteLog(RunReturnValue.ToString() + ";" + RunFalg.ToString() + ";Continue End" );
                    falg = ExperimentRunStatus.Start;
                    goto RunScript;
                }
                if (RunFalg == RotationInfoStatus.Suspend || RunFalg == RotationInfoStatus.Stop || RunFalg == RotationInfoStatus.Fail) return;
        
                if (RunReturnValue)
                {
                    RunFalg = RotationInfoStatus.Finish;
                }
                else
                {
                    RunFalg = RotationInfoStatus.Fail;
                }

            }));
            #endregion
            #region  扫描试剂耗材文件
            Thread thread = new Thread(new ThreadStart(delegate
            {                
                while (RunFalg == RotationInfoStatus.Processing)
                {
                    Thread.Sleep(1000);
                    ReadReagentsAndSuppliesConsumption(CurrentRotation.RotationID);
                    Thread.Sleep(1000);
                    DetectReagent(CurrentReagentAndSupply);
                }
            }));
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
            #endregion
           /******试剂耗材报警*********************************/
            //DetectReagent();
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            /*****调研更新UI***************************************/
            #region 调研更新UI
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                while (RunFalg == RotationInfoStatus.Processing)
                {
                    if (worker.CancellationPending)
                    {
                        args.Cancel = true;
                        return;
                    }
                    if (isParseProgress)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    if (worker.IsBusy)
                        worker.ReportProgress(1, 0);
                    Thread.Sleep(1000);
                }
                if (SetEvoRestorationStatus != null)
                {
                    SetEvoRestorationStatus(true);
                }
            };
            #endregion
            
            // 更新已完成数据
            if (SessionInfo.ResumeExecution) {
                foreach (RotationInfo finishedRotation in ExperimentRotation)
                {
                    foreach (OperationConfiguration finishedOperation in finishedRotation.Operations)
                    {
                        if (finishedRotation.RunIndex < CurrentRotation.RunIndex
                            || finishedOperation.OperationSequence < CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                        {
                            if (finishedOperation.OperationType == 0)
                            {
                                // 单一操作
                                Grid grid = (stackPanel.FindName("grid" + (finishedRotation.RunIndex + 1).ToString()) as Grid);
                                if (grid != null)
                                {
                                    ProgressBar bar = grid.FindName("ProgressBar" + (finishedRotation.RunIndex + 1).ToString() + finishedOperation.OperationID.ToString().Replace("-", "")) as ProgressBar;
                                    if (bar != null)
                                    {
                                        bar.Value = (int)finishedOperation.RunTime * 60;
                                        bar.Tag = ExperimentRunStatus.Finish;
                                    }
                                    Label lab = grid.FindName("lab" + (finishedRotation.RunIndex + 1).ToString() + finishedOperation.OperationID.ToString().Replace("-", "")) as Label;
                                    if (lab != null)
                                    {
                                        lab.Content = "(已完成)";
                                    }
                                }
                            }
                            else
                            {
                                // 组合操作
                                foreach (string operationID in finishedOperation.SubOperationIDs.Split(','))
                                {
                                    if (string.IsNullOrEmpty(operationID)) continue;
                                    OperationConfiguration SubOperationConfiguration = OperationConfigurations.Where(operation => operation.OperationID == new Guid(operationID)).FirstOrDefault();
                                    if (SubOperationConfiguration == null) continue;
                                    Grid grid = (stackPanel.FindName("grid" + (finishedRotation.RunIndex + 1).ToString()) as Grid);
                                    if (grid != null)
                                    {
                                        ProgressBar bar = grid.FindName("ProgressBar" + (finishedRotation.RunIndex + 1).ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", "")) as ProgressBar;
                                        if (bar != null)
                                        {
                                            bar.Value = (int)SubOperationConfiguration.RunTime * 60;
                                            bar.Tag = ExperimentRunStatus.Finish;
                                            Label lab = grid.FindName("lab" + (finishedRotation.RunIndex + 1).ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", "")) as Label;
                                            if (lab != null)
                                            {
                                                lab.Content = "(已完成)";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /***********实时更新UI********************************************************************/
            #region 实时更新UI
            DateTime fileBeforeCreatedTime = WanTai.Controller.EVO.ProcessorFactory.GetDateTimeNow();
            DateTime dtime = DateTime.Now;
            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                if (string.IsNullOrEmpty(labRunTime.Uid))
                    labRunTime.Uid = "0";
                int RunSeconds = int.Parse(labRunTime.Uid) + 1;
                labRunTime.Uid = RunSeconds.ToString();
                TimeSpan ts = new TimeSpan(0, 0, RunSeconds);
                labRunTime.Content=((int)ts.TotalHours).ToString("00")+":"  + ts.Minutes.ToString("00")+":"  + ts.Seconds.ToString("00") ;
               // labRunTime.Content = (RunSeconds / 3600).ToString("00") + ":" + (RunSeconds / 60).ToString("00") + ":" + (RunSeconds % 60).ToString("00");
                /**********更新单一操作UI********************************************/
                if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == 0)
                {
                    Grid grid = (stackPanel.FindName("grid" + (CurrentRotation.RunIndex + 1).ToString()) as Grid);
                    if (grid != null)
                    {
                        ProgressBar bar = grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + 1).ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID.ToString().Replace("-", "")) as ProgressBar;
                       // ProgressBar bar = grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + 1).ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType.ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence.ToString()) as ProgressBar;
                        if (bar != null)
                        {
                            if (falg == ExperimentRunStatus.ReStart && bar.DataContext != null)
                            {
                                if ((Convert.ToDateTime(bar.DataContext)) < dtime)
                                {
                                    bar.Value = 0;
                                    bar.DataContext = DateTime.Now;
                                }
                            }
                            if (falg == ExperimentRunStatus.ReStart && bar.DataContext == null)
                            {
                                bar.Value = 0;
                                bar.DataContext = DateTime.Now;
                            }

                            bar.Value += 1;
                        }
                        Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + 1).ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID.ToString().Replace("-", "")) as Label;
                      //  Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + 1).ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType.ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence.ToString()) as Label;
                        if (lab != null)
                        {
                            if (falg == ExperimentRunStatus.ReStart && lab.DataContext != null)
                            {
                                if ((Convert.ToDateTime(lab.DataContext)) < dtime)
                                {
                                    lab.Uid = "0";
                                    lab.DataContext = DateTime.Now;
                                }
                            }
                            if (falg == ExperimentRunStatus.ReStart && lab.DataContext == null)
                            {
                                lab.Uid = "0";
                                lab.DataContext = DateTime.Now;
                            }
                            RunSeconds = int.Parse(lab.Uid) + 1;
                            ts = new TimeSpan(0, 0, RunSeconds);
                            lab.Content = ((int)ts.TotalHours).ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                            //  lab.Content = (RunSeconds / 3600).ToString("00") + ":" + (RunSeconds / 60).ToString("00") + ":" + (RunSeconds % 60).ToString("00");
                            lab.Uid = RunSeconds.ToString();
                        }
                    }
                }
                else
                {/*****更新组合操作********************************************************************/
                    foreach (string operationID in CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].SubOperationIDs.Split(','))
                    {
                        if (string.IsNullOrEmpty(operationID)) continue;
                        OperationConfiguration SubOperationConfiguration = OperationConfigurations.Where(operation => operation.OperationID == new Guid(operationID)).FirstOrDefault();
                        if (SubOperationConfiguration == null) continue;
                        if (SubOperationConfiguration.OperationSequence == CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                            CurrentRotation.CurrentOperationColtrlName =  SubOperationConfiguration.OperationID.ToString().Replace("-","");
                        int index = 1;
                        /******判断是否下一轮次操作***********************************************/
                        if (SubOperationConfiguration.OperationSequence < CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                        {
                            if (NexRotation)
                                index = 2;
                            else continue;
                        }
                        bool isStartFileExist = false;
                        bool isFinishedFileExist = false;
                        if (System.IO.File.Exists(EvoOutputPath + SubOperationConfiguration.StartOperationFileName))
                        {
                            System.IO.FileInfo fileInfo = new System.IO.FileInfo(EvoOutputPath + SubOperationConfiguration.StartOperationFileName);
                            DateTime fileModifiedTime = fileInfo.LastWriteTime;
                            if (DateTime.Compare(fileBeforeCreatedTime, fileModifiedTime) <= 0)
                                isStartFileExist = true;
                        }
                        if (System.IO.File.Exists(EvoOutputPath + SubOperationConfiguration.EndOperationFileName))
                        {
                            System.IO.FileInfo fileInfo = new System.IO.FileInfo(EvoOutputPath + SubOperationConfiguration.EndOperationFileName);
                            DateTime fileModifiedTime = fileInfo.LastWriteTime;
                            if (DateTime.Compare(fileBeforeCreatedTime, fileModifiedTime) <= 0)
                                isFinishedFileExist = true;
                        }
                        Grid grid = (stackPanel.FindName("grid" + (CurrentRotation.RunIndex + index).ToString()) as Grid);
                        if (grid != null)
                        {
                            ProgressBar bar = grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", "")) as ProgressBar;
                           // ProgressBar bar = grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationType.ToString() + SubOperationConfiguration.OperationSequence.ToString()) as ProgressBar;
                            if (bar != null)
                            {
                                if (bar.Tag !=null && falg!=ExperimentRunStatus.ReStart) continue;
                                if (falg == ExperimentRunStatus.Continue && bar.Value > 0)
                                    isStartFileExist = true;
                                if (falg == ExperimentRunStatus.Continue && bar.Tag!=null)
                                    isFinishedFileExist = true;
                                if (isStartFileExist && !isFinishedFileExist || SubOperationConfiguration.OperationSequence == CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                                {
                                    if (falg == ExperimentRunStatus.ReStart && bar.DataContext != null)
                                    {
                                        if ((Convert.ToDateTime(bar.DataContext)) < dtime)
                                        {
                                            bar.Value = 0;
                                            bar.DataContext = DateTime.Now;
                                            bar.Tag = null;
                                        }
                                    }
                                    if (falg == ExperimentRunStatus.ReStart && bar.DataContext == null)
                                    {
                                        bar.Value = 0;
                                        bar.DataContext = DateTime.Now;
                                    }
                                    bar.Value += 1;
                                    //if (!File.Exists(EvoOutputPath + SubOperationConfiguration.EndOperationFileName) || SubOperationConfiguration.OperationSequence == CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                                    //{
                                    Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", "")) as Label;
                                    //Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationType.ToString() + SubOperationConfiguration.OperationSequence.ToString()) as Label;
                                    if (lab != null)
                                    {
                                        if (falg == ExperimentRunStatus.ReStart && lab.DataContext == null)
                                        {
                                            lab.Uid = "0";
                                            lab.DataContext = DateTime.Now;
                                        }
                                        if (falg == ExperimentRunStatus.ReStart && lab.DataContext == null)
                                        {
                                            lab.Uid = "0";
                                            lab.DataContext = DateTime.Now;
                                        }
                                        RunSeconds = int.Parse(lab.Uid) + 1;
                                        ts = new TimeSpan(0, 0, RunSeconds);
                                        lab.Content = ((int)ts.TotalHours).ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00");
                                        //lab.Content = (RunSeconds / 3600).ToString("00") + ":" + (RunSeconds / 60).ToString("00") + ":" + (RunSeconds % 60).ToString("00");
                                        lab.Uid = RunSeconds.ToString();
                                    }
                                }
                                if (isFinishedFileExist && SubOperationConfiguration.OperationSequence != CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                                {
                                    bar.Value = (int)SubOperationConfiguration.RunTime*60;
                                    bar.Tag = ExperimentRunStatus.Finish;
                                    
                                    Guid currentOperationRotation = (NexRotation &&SubOperationConfiguration.OperationSequence < CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence) ?
                                        ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID : CurrentRotation.RotationID;
                                    if (SessionInfo.RotationFormulaParameters.ContainsKey(currentOperationRotation))
                                       new Services.ReagentDetectService().AddSuppliesConsumption(currentOperationRotation, SubOperationConfiguration.OperationID, SessionInfo.RotationFormulaParameters[currentOperationRotation]);
                                }
                            }
                        }
                    }
                }
               
            };
            #endregion
            /*****操作结束（带脚本操作  混样、提取、试剂模板封装）********************************************/
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                //if (!rotationFormulaParameters.ContainsKey(CurrentRotation.RotationID))
                //{
                //    Dictionary<string, object> formulaParameters = new Dictionary<string, object>();
                //    formulaParameters.Add("PCRWorkListRowCount", FormulaParameters.PCRWorkListRowCount);
                //    formulaParameters.Add("PoolCountInTotal", FormulaParameters.PoolCountInTotal);
                //    formulaParameters.Add("PoolingWorkListRowCount", FormulaParameters.PoolingWorkListRowCount);
                //    formulaParameters.Add("TestItemCountInTotal", FormulaParameters.TestItemCountInTotal);
                //    var PoolCountOfTestItem = new Dictionary<string, int>(FormulaParameters.PoolCountOfTestItem);
                //    formulaParameters.Add("PoolCountOfTestItem", PoolCountOfTestItem);
                //    rotationFormulaParameters.Add(CurrentRotation.RotationID, formulaParameters);
                //}

                //stop the running scan liquid thread
                ReagentDetectService.RunningLiquidThread = false;

                if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Grouping)
                {
                    string ErrMsg;
                    LogInfoController.InstertKingFisherLog(ExperimentID, dtime, DateTime.Now, out ErrMsg);
                    if (ErrMsg != "success")
                        MessageBox.Show(ErrMsg, "系统提示");
                }
                CurrentRotation.CurrentRotationOperate.EndTime = DateTime.Now;
                CurrentRotation.CurrentRotationOperate.OperationID = CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID;
                CurrentRotation.CurrentRotationOperate.State = (short)RunFalg;
                if (!RunReturnValue || RunFalg == RotationInfoStatus.Fail || RunFalg == RotationInfoStatus.Stop)
                {
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        CurrentRotation.CurrentRotationOperate.ErrorLog = errorMessage;
                    }
                    else
                    {
                        CurrentRotation.CurrentRotationOperate.ErrorLog = "Run finished with errors";
                    }
                    if (File.Exists(WanTai.Common.Configuration.GetEvoVariableOutputPath() + WanTai.Common.Configuration.GetKingFisherScriptName()))
                    {
                        WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                        processor.AboutKingFisher();
                        processor.Close();
                    }
                }
                rotationInfoController.UpdataRotationOperateStatus(CurrentRotation.CurrentRotationOperate, RunFalg);
                if (RunFalg == RotationInfoStatus.Suspend) return;
                string ErrMessage = "失败";
                if (RunFalg == RotationInfoStatus.Stop)
                {
                    ErrMessage = "停止";

                  //  rotationInfoController.UpdataExperimentStatus(ExperimentID, true, ExperimentStatus.Stop, CurrentRotation.RotationID, RotationInfoStatus.Stop);
                }
                if (!RunReturnValue || RunFalg == RotationInfoStatus.Stop || RunFalg== RotationInfoStatus.Fail)//停止 执行返回False 就退出，不执行下一轮次
                {
                    SessionInfo.CurrentExperimentsInfo.State = (short)RunFalg;
                    if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == 0)
                    {
                        Grid grid = (stackPanel.FindName("grid" + (CurrentRotation.RunIndex + 1).ToString()) as Grid);
                        if (grid != null)
                        {
                            Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + 1).ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID.ToString().Replace("-", "")) as Label;
                            //  Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + 1).ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType.ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence.ToString()) as Label;
                            if (lab != null)
                                lab.Content += ErrMessage;
                        }
                    }
                    else
                    {
                        foreach (string operationID in CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].SubOperationIDs.Split(','))
                        {
                            if (string.IsNullOrEmpty(operationID)) continue;
                            OperationConfiguration SubOperationConfiguration = OperationConfigurations.Where(operation => operation.OperationID == new Guid(operationID)).FirstOrDefault();
                            if (SubOperationConfiguration == null) continue;
                            if (SubOperationConfiguration.OperationSequence == CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                                CurrentRotation.CurrentOperationColtrlName = CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType.ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence.ToString();
                            int index = 1;
                            if (SubOperationConfiguration.OperationSequence < CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                            {
                                if (NexRotation)
                                    index = 2;
                                else continue;
                            }
                            Grid grid = (stackPanel.FindName("grid" + (CurrentRotation.RunIndex + index).ToString()) as Grid);
                            if (grid != null)
                            {
                                ProgressBar bar = grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", "")) as ProgressBar;
                                //ProgressBar bar = grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationType.ToString() + SubOperationConfiguration.OperationSequence.ToString()) as ProgressBar;
                                if (bar != null)
                                {
                                    if (bar.Tag != null) continue;
                                    Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationID.ToString().Replace("-", "")) as Label;
                                    // Label lab = grid.FindName("lab" + (CurrentRotation.RunIndex + index).ToString() + SubOperationConfiguration.OperationType.ToString() + SubOperationConfiguration.OperationSequence.ToString()) as Label;
                                    if (lab != null)
                                        lab.Content += ErrMessage;
                                }
                            }
                        }
                    }
                    if (RunFalg == RotationInfoStatus.Stop)
                        SessionInfo.CurrentExperimentsInfo.State = (short)ExperimentStatus.Stop;

                    if (ExperimentRunErrEvent != null)
                        ExperimentRunErrEvent();

                    return;
                }
                #region 
                /////add DITI and plate consumpution
                //short DWPlateType = 101;
                //short DW96TipCombType = 102;
                //short PCRPlateType = 103;
                //short[] SuppliesTypes = new short[] { DiTiType.DiTi1000, DiTiType.DiTi200, DWPlateType, DW96TipCombType, PCRPlateType};
                //WanTai.Controller.Configuration.ReagentSuppliesConfigurationController reagentSuppliesConfigurationController = new WanTai.Controller.Configuration.ReagentSuppliesConfigurationController();
                //ReagentAndSuppliesController suppliesController = new ReagentAndSuppliesController();                

                //if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Single)
                //{
                //    ///SampleTrackingController perrywang
                //    SampleTrackingController.SampleTracking(ExperimentID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, CurrentRotation.RotationID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence, new Guid(), fileBeforeCreatedTime); 
                //    if (rotationFormulaParameters.ContainsKey(CurrentRotation.RotationID))
                //    {
                //        FormulaParameters.PCRWorkListRowCount = (int)rotationFormulaParameters[CurrentRotation.RotationID]["PCRWorkListRowCount"];
                //        FormulaParameters.PoolCountInTotal = (int)rotationFormulaParameters[CurrentRotation.RotationID]["PoolCountInTotal"];
                //        FormulaParameters.PoolingWorkListRowCount = (int)rotationFormulaParameters[CurrentRotation.RotationID]["PoolingWorkListRowCount"];
                //        FormulaParameters.TestItemCountInTotal = (int)rotationFormulaParameters[CurrentRotation.RotationID]["TestItemCountInTotal"];
                //        FormulaParameters.PoolCountOfTestItem = (Dictionary<string, int>)rotationFormulaParameters[CurrentRotation.RotationID]["PoolCountOfTestItem"];
                //    }

                //    Dictionary<short, bool> operations = new Dictionary<short,bool>();
                //    List<OperationConfiguration> allSingles = new OperationController().GetAllSingleOperations();
                //    foreach (OperationConfiguration singles in allSingles)
                //    {
                //        if (singles.OperationSequence == CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                //        {
                //            operations.Add(singles.OperationSequence, true);
                //        }
                //        else
                //        {
                //            operations.Add(singles.OperationSequence, false);
                //        }
                //    }
                    
                //    List<ReagentAndSuppliesConfiguration> SuppliesList = reagentSuppliesConfigurationController.GetReagentAndSuppliesNeeded(SuppliesTypes, operations);
                //    foreach (ReagentAndSuppliesConfiguration config in SuppliesList)
                //    {
                //        if (config.NeedVolume > 0)
                //        {
                //            List<ReagentAndSupply> ReagentAndSupplyList = suppliesController.GetAllByTypeAndRotationId((int)config.ItemType, CurrentRotation.RotationID);
                //            if (config.ItemType == DiTiType.DiTi1000 || config.ItemType == DiTiType.DiTi200)
                //            {
                //                AddReagentsAndSuppliesConsumption(ReagentAndSupplyList.FirstOrDefault().ItemID, CurrentRotation.RotationID, config.NeedVolume);
                //            }
                //            else if (config.ItemType == DWPlateType || config.ItemType == DW96TipCombType || config.ItemType == PCRPlateType)
                //            {
                //                for(int i=0;i<config.NeedVolume;i++)
                //                {
                //                    if((i+1)<ReagentAndSupplyList.Count)
                //                        AddReagentsAndSuppliesConsumption(ReagentAndSupplyList[i].ItemID, CurrentRotation.RotationID, 1);
                //                }
                //            }
                //        }
                //    }
                //}
                //else if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Grouping)
                //{
                //    ///SampleTrackingController perrywang
                //    SampleTrackingController.SampleTracking(ExperimentID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, CurrentRotation.RotationID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence, (NexRotation ? ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID : new Guid()), fileBeforeCreatedTime); 
                //    if (rotationFormulaParameters.ContainsKey(CurrentRotation.RotationID))
                //    {
                //        FormulaParameters.PCRWorkListRowCount = (int)rotationFormulaParameters[CurrentRotation.RotationID]["PCRWorkListRowCount"];
                //        FormulaParameters.PoolCountInTotal = (int)rotationFormulaParameters[CurrentRotation.RotationID]["PoolCountInTotal"];
                //        FormulaParameters.PoolingWorkListRowCount = (int)rotationFormulaParameters[CurrentRotation.RotationID]["PoolingWorkListRowCount"];
                //        FormulaParameters.TestItemCountInTotal = (int)rotationFormulaParameters[CurrentRotation.RotationID]["TestItemCountInTotal"];
                //        FormulaParameters.PoolCountOfTestItem = (Dictionary<string, int>)rotationFormulaParameters[CurrentRotation.RotationID]["PoolCountOfTestItem"];
                //    }

                //    List<OperationConfiguration> operationConfigs = new List<OperationConfiguration>();
                //    new OperationController().GetSingleSubOperations(CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, ref operationConfigs);
                //    Dictionary<short, bool> curOperations = new Dictionary<short, bool>();
                //    Dictionary<short, bool> nextOperations = new Dictionary<short, bool>();
                //    List<OperationConfiguration> allSingles = new OperationController().GetAllSingleOperations();
                //    foreach (OperationConfiguration config in operationConfigs)
                //    {
                //        if(config.OperationSequence< CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence)
                //            nextOperations.Add(config.OperationSequence, true);
                //        else
                //            curOperations.Add(config.OperationSequence, true);
                //    }
                //    foreach (OperationConfiguration singles in allSingles)
                //    {
                //        if (!curOperations.ContainsKey(singles.OperationSequence))
                //            curOperations.Add(singles.OperationSequence, false);

                //        if (!nextOperations.ContainsKey(singles.OperationSequence))
                //            nextOperations.Add(singles.OperationSequence, false);
                //    }

                //    //record current consumption
                //    List<ReagentAndSuppliesConfiguration> SuppliesList = reagentSuppliesConfigurationController.GetReagentAndSuppliesNeeded(SuppliesTypes, curOperations);
                //    foreach (ReagentAndSuppliesConfiguration config in SuppliesList)
                //    {
                //        if (config.NeedVolume > 0)
                //        {
                //            List<ReagentAndSupply> ReagentAndSupplyList = suppliesController.GetAllByTypeAndRotationId((int)config.ItemType, CurrentRotation.RotationID);
                //            if (config.ItemType == DiTiType.DiTi1000 || config.ItemType == DiTiType.DiTi200)
                //            {
                //                AddReagentsAndSuppliesConsumption(ReagentAndSupplyList.FirstOrDefault().ItemID, CurrentRotation.RotationID, config.NeedVolume);
                //            }
                //            else if (config.ItemType == DWPlateType || config.ItemType == DW96TipCombType || config.ItemType == PCRPlateType)
                //            {
                //                for (int i = 0; i < config.NeedVolume; i++)
                //                {
                //                    if ((i + 1) < ReagentAndSupplyList.Count)
                //                        AddReagentsAndSuppliesConsumption(ReagentAndSupplyList[i].ItemID, CurrentRotation.RotationID, 1);
                //                }
                //            }
                //        }
                //    }

                //    //check if has next rotation
                //    if (CurrentRotation.RunIndex + 1 < ExperimentRotation.Count && nextOperations.ContainsValue(true))
                //    {
                //        //record next rotation consumption
                //        SuppliesList = reagentSuppliesConfigurationController.GetReagentAndSuppliesNeeded(SuppliesTypes, nextOperations);
                //        foreach (ReagentAndSuppliesConfiguration config in SuppliesList)
                //        {
                //            if (config.NeedVolume > 0)
                //            {
                //                List<ReagentAndSupply> ReagentAndSupplyList = suppliesController.GetAllByTypeAndRotationId((int)config.ItemType, ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID);
                //                if (config.ItemType == DiTiType.DiTi1000 || config.ItemType == DiTiType.DiTi200)
                //                {
                //                    AddReagentsAndSuppliesConsumption(ReagentAndSupplyList.FirstOrDefault().ItemID, ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID, config.NeedVolume);
                //                }
                //                else if (config.ItemType == DWPlateType || config.ItemType == DW96TipCombType || config.ItemType == PCRPlateType)
                //                {
                //                    for (int i = 0; i < config.NeedVolume; i++)
                //                    {
                //                        if ((i + 1) < ReagentAndSupplyList.Count)
                //                            AddReagentsAndSuppliesConsumption(ReagentAndSupplyList[i].ItemID, ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID, 1);
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion
                else
                {
                    if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == (short)OperationType.Single)
                    {
                        if (SessionInfo.RotationFormulaParameters.ContainsKey(CurrentRotation.RotationID)) 
                            new Services.ReagentDetectService().AddSuppliesConsumption(CurrentRotation.RotationID, CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID, SessionInfo.RotationFormulaParameters[CurrentRotation.RotationID]);
                    }
                    else
                    {                        
                        //Guid subOpertionWithSameEquence = new OperationController().GetSameSequenceSubOperation(CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID);
                        //if (subOpertionWithSameEquence != Guid.Empty && SessionInfo.RotationFormulaParameters.ContainsKey(CurrentRotation.RotationID)) 
                        //    new Services.ReagentDetectService().AddSuppliesConsumption(CurrentRotation.RotationID, subOpertionWithSameEquence, SessionInfo.RotationFormulaParameters[CurrentRotation.RotationID]);
                    }
                }
                Grid _grid = (stackPanel.FindName("grid" + (CurrentRotation.RunIndex + 1).ToString()) as Grid);
                if (CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType == 0)
                {
                    ProgressBar bar = _grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + 1) + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationID.ToString().Replace("-", "")) as ProgressBar;
                    //ProgressBar bar = _grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + 1) + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationType.ToString() + CurrentRotation.Operations[CurrentRotation.CurrentOperationIndex].OperationSequence.ToString()) as  ProgressBar;
                    if (bar != null)
                        bar.Value = bar.Maximum;
                }
                else
                {
                    ProgressBar bar = _grid.FindName("ProgressBar" + (CurrentRotation.RunIndex + 1) + CurrentRotation.CurrentOperationColtrlName) as ProgressBar;
                    if (bar != null)
                        bar.Value = bar.Maximum;
                }

                if (CurrentRotation.CurrentOperationIndex >= (CurrentRotation.Operations.Count - 1))
                {
                    FinishedRotation = CurrentRotation;
                    if (NexRotation) //读下一个轮次
                    {
                        rotationInfoController.UpdataRotationStatus(CurrentRotation.RotationID, ExperimentRotation[CurrentRotation.RunIndex + 1].RotationID, RotationInfoStatus.Finish, RotationInfoStatus.Processing);
                        CurrentRotation = ExperimentRotation[CurrentRotation.RunIndex + 1];
                        CurrentRotation.CurrentOperationIndex = 1;
                        if (CurrentRotation.RunIndex < (ExperimentRotation.Count - 1))
                            NexRotation = true;
                        else
                            NexRotation = false;
                       
                        WriteTimesCSV(NexRotation ? 1 : 0);
                    }
                    else //最后一个轮次
                    {
                        rotationInfoController.UpdataExperimentStatus(ExperimentID, true, ExperimentStatus.Finish, CurrentRotation.RotationID, RotationInfoStatus.Finish);
                        if (ExperimentRunViewEvent != null)
                            ExperimentRunViewEvent();

                        btnTrash_Click(true, null);
                        SessionInfo.CurrentExperimentsInfo.State = (short)ExperimentStatus.Finish;
                        MessageBox.Show("当前实验运行完成!", "系统提示");
                        return;
                    }
                    btnTrash.Uid = "Error";
                    btnTrash.IsEnabled = true;
                    btnTrash_Click(null, null);
                }
                else
                    CurrentRotation.CurrentOperationIndex++;
                SessionInfo.CurrentExperimentsInfo.State = (short)ExperimentStatus.Suspend;
                if (SessionInfo.WaitForSuspend)
                {
                    // save current status
                    SessionInfo.CurrentRotation = CurrentRotation;
                    SessionInfo.NexRotation = NexRotation;

                    SerializeStatic serialize = new SerializeStatic();
                    serialize.Save("SessionInfo.bin");

                    MessageBox.Show("当前脚本执行完毕，中断成功!确定后退出系统!", "系统提示");
                    SuspendOkEvent();
                    return;
                }
                else
                {
                    Start(ExperimentRunStatus.Start);
                }
            };
            worker.RunWorkerAsync();

            return true;
        }
        /// <summary>
        /// 提取开始下一轮次扫描时判断脚本名称
        /// </summary>
        public string NextTurnStepScrtipName
        {
            get { return WanTai.Common.Configuration.GoToNextTurnScripts(); }
        }
        /// <summary>
        /// perry 2012-1-5 提取开始跳转到扫描
        /// </summary>
        /// <param name="RunTime"></param>
        private void NextTurnStep(string ScriptName)
        {
            if (NexRotation && NextTurnStepScrtipName.Contains(ScriptName) && SessionInfo.NextTurnStep == -1)
            {
                if (NewExperimentEvent != null)
                    NewExperimentEvent();
                //DateTime RunTime = WanTai.Controller.EVO.ProcessorFactory.GetDateTimeNow();
                //WanTai.Controller.TubesController tubesController = new TubesController();
                //Thread thread = new Thread(new ThreadStart(delegate
                //{
                //    while (!tubesController.GetNextTurnStep(RunTime, 0) && SessionInfo.CurrentExperimentsInfo.State == (short)ExperimentStatus.Processing)
                //    {
                //        Thread.Sleep(1000);
                //    }
                //    if (NewExperimentEvent != null)
                //    {
                //        NewExperimentEvent();
                //        SessionInfo.NextTurnStep = 0;
                //    }
                //}));
                //thread.IsBackground = true;
                //thread.Start();
            }
        }
        private void AddReagentsAndSuppliesConsumption(Guid _ReagentAndSupplieID, Guid _RotationID, double _Volume)
        {
            ReagentsAndSuppliesConsumption Reagents = new ReagentsAndSuppliesConsumption()
            {
                ExperimentID = ExperimentID,
                ItemID = WanTaiObjectService.NewSequentialGuid(),
                ReagentAndSupplieID = _ReagentAndSupplieID,
                RotationID = _RotationID,
                UpdateTime = DateTime.Now,
                Volume = _Volume,
                VolumeType = 1
            };
            new ReagentsAndSuppliesConsumptionController().AddConsumption(Reagents);
        }

        private WanTai.Controller.EVO.EVO_DoorLockStatus CheckDoorLockStatus()
        {
            WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
            WanTai.Controller.EVO.EVO_DoorLockStatus doorLockStatus = processor.CheckDoorLockStatus();
            return doorLockStatus;
        }

        private List<ReagentAndSupply> _ExperimentReagentAndSupply;
        private List<ReagentAndSupply> ExperimentReagentAndSupply
        {
            get
            {
                if (_ExperimentReagentAndSupply == null)
                    _ExperimentReagentAndSupply = new ReagentAndSuppliesController().GetAllByType(ReagentType.GeneralReagent);
                return _ExperimentReagentAndSupply;
            }
        }
        private List<ReagentAndSuppliesConfiguration> _PCRHeatLiquidPlate;
        private List<ReagentAndSuppliesConfiguration> PCRHeatLiquidPlate
        {
            get
            {
                if (_PCRHeatLiquidPlate == null)
                    _PCRHeatLiquidPlate = new ReagentAndSuppliesController().GetPCRHeatLiquidPlate();
                return _PCRHeatLiquidPlate;
            }
        }
        private void ReadReagentsAndSuppliesConsumption(Guid RotationID)
        {
            foreach (ReagentAndSupply _ReagentAndSupply in ExperimentReagentAndSupply)
            {
                string FilePath = WanTai.Common.Configuration.GetEvoOutputPath() + WanTai.Common.Configuration.GetSampleTrackingFileLocation();
                DirectoryInfo di = new DirectoryInfo(FilePath);
                FileInfo[] files = di.GetFiles(_ReagentAndSupply.BarCode + "*.csv", SearchOption.TopDirectoryOnly);
                if (files == null || files.Count() == 0)
                    continue;

                List<FileInfo> orderedFiles = files.OrderByDescending(f => f.LastWriteTime).ToList();
                FileInfo lastFileInfo = orderedFiles[0];
                if (ExperimentStartTime > lastFileInfo.LastWriteTime) continue;
                using (FileStream fileStream = new System.IO.FileStream(lastFileInfo.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    using (System.IO.StreamReader mysr = new System.IO.StreamReader(fileStream))
                    {
                        string strline;
                        string[] aryline;
                        strline = mysr.ReadLine();
                        //1(Position);1;1(Grid);Tube 13*100mm 16 Pos;Tube1;013/035678;11
                        float Volume = 0;
                        while ((strline = mysr.ReadLine()) != null)
                        {
                            aryline = strline.Split(new char[] { ',' });
                            if (aryline.Count() < 7) continue;
                            if (string.IsNullOrEmpty(aryline[6]) || aryline[6] == "0") continue;
                            float v = 0;
                            if (float.TryParse(aryline[6], out v))
                                Volume += Math.Abs(v);
                        }
                        if (Volume > 0)
                        {
                            ReagentsAndSuppliesConsumption Reagents = new ReagentsAndSuppliesConsumption()
                            {
                                ExperimentID = ExperimentID,
                                ItemID = WanTaiObjectService.NewSequentialGuid(),
                                ReagentAndSupplieID = _ReagentAndSupply.ItemID,
                                RotationID = RotationID,
                                UpdateTime = DateTime.Now,
                                Volume = -Math.Round(Volume, 2),
                                VolumeType = ConsumptionType.consume
                            };
                            new ReagentsAndSuppliesConsumptionController().UpdateConsumption(Reagents);
                        }
                    }
                }
            }
          
            foreach (ReagentAndSuppliesConfiguration reagentAndSuppliesConfiguration in PCRHeatLiquidPlate)
            {
                string FilePath = WanTai.Common.Configuration.GetEvoOutputPath() + WanTai.Common.Configuration.GetSampleTrackingFileLocation() + reagentAndSuppliesConfiguration.BarcodePrefix + ".csv";
                if (!File.Exists(FilePath)) continue;
                if (ExperimentStartTime > File.GetLastWriteTime(FilePath)) continue;
                using (WanTaiEntities _WanTaiEntities=new WanTaiEntities())
                {
                    using (FileStream fileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                    {
                        using (System.IO.StreamReader mysr = new System.IO.StreamReader(fileStream))
                        {
                            string strline;
                            string[] aryline;
                            strline = mysr.ReadLine();
                            //1(Position);1;1(Grid);Tube 13*100mm 16 Pos;Tube1;013/035678;11

                            while ((strline = mysr.ReadLine()) != null)
                            {
                                aryline = strline.Split(new char[] { ',' });
                                if (aryline.Count() < 7) continue;
                                if (string.IsNullOrEmpty(aryline[6]) || aryline[6] == "0" || string.IsNullOrEmpty(aryline[3])) continue;
                                float v = 0;
                                string[] BarCodes = aryline[3].Split(new char[] { '_' });
                                if (BarCodes.Length < 2) continue;
                                string BarCode = BarCodes[BarCodes.Length-1];
                                if (float.TryParse(aryline[6], out v))
                                    v += Math.Abs(v);
                                var ItemID = from r in _WanTaiEntities.ReagentAndSupplies
                                             join rs in _WanTaiEntities.ReagentsAndSuppliesConsumptions
                                             on r.ItemID equals rs.ReagentAndSupplieID
                                             where rs.VolumeType == 0 && r.ExperimentID == ExperimentID && rs.RotationID == RotationID && r.BarCode == BarCode
                                             select r.ItemID;
                                if (ItemID.Count() > 0)
                                {
                                    ReagentsAndSuppliesConsumption Reagents = new ReagentsAndSuppliesConsumption()
                                    {
                                        ExperimentID = ExperimentID,
                                        ItemID = WanTaiObjectService.NewSequentialGuid(),
                                        ReagentAndSupplieID = ItemID.FirstOrDefault(),
                                        RotationID = RotationID,
                                        UpdateTime = DateTime.Now,
                                        Volume = -Math.Round(v, 2),
                                        VolumeType = ConsumptionType.consume
                                    };
                                    ReagentsAndSuppliesConsumption Result = _WanTaiEntities.ReagentsAndSuppliesConsumptions.Where(Reagent => Reagent.VolumeType == 1 && Reagent.RotationID == RotationID && Reagent.ReagentAndSupplieID == ItemID.FirstOrDefault()).FirstOrDefault();
                                    if (Result == null)
                                        _WanTaiEntities.ReagentsAndSuppliesConsumptions.AddObject(Reagents);
                                    else
                                    {
                                        Result.Volume = Reagents.Volume;
                                        Result.UpdateTime = DateTime.Now;
                                    }
                                    _WanTaiEntities.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }
        private List<ReagentAndSuppliesConfiguration> _CurrentReagentAndSupply;
        private List<ReagentAndSuppliesConfiguration> CurrentReagentAndSupply
        {
            get
            {
                byte[] bytes = new byte[16];
                BitConverter.GetBytes(ReagentType.GeneralReagent).CopyTo(bytes, 0);

                if (_CurrentReagentAndSupply == null)
                    _CurrentReagentAndSupply = new ReagentSuppliesConfigurationController().GetByItemType(new Guid(bytes));
                return _CurrentReagentAndSupply;
            }
        }
        private void DetectReagent(List<ReagentAndSuppliesConfiguration> CurrentList)
        {
            //ReagentDetectService reagentDetectService = new ReagentDetectService();
            //ReagentDetectService.RunningLiquidThread = true;
            //Thread thread = new Thread(new ThreadStart(reagentDetectService.CalacCorrentVolume));                       
            //reagentDetectService.RiseWarning += new ReagentWarningHandler(ReagentWarning);
            //reagentDetectService.RiseNormal += new ReagentWarningHandler(ReagentNormal);
            //thread.Start(); 
            bool normalLevel = true;
            ReagentSuppliesConfigurationController configurationController = new ReagentSuppliesConfigurationController();
            configurationController.UpdateExperimentVolume(SessionInfo.ExperimentID, ref CurrentList, new short[] { 1, 2, 3 }, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName);
            configurationController.NeededVolumeofProcessingRotations(ref CurrentList);
            normalLevel = new ReagentDetectService().CheckCurrentVolume(CurrentList);
            if (normalLevel)
            {
                ReagentNormal(this, null);
            }
            else
            {
                ReagentWarning(this, null);
            }
        }

        private void ReagentWarning(object sender, ReagentWarningArgs args)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                delegate()
                {
                    btnAddReagent.Uid = "Error";
                   // btnAddReagent.Background = Brushes.Red;

                    //btnAddReagent_Click(null, null);
                }
            ));
        }

        private void ReagentNormal(object sender, ReagentWarningArgs args)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                delegate()
                {
                    btnAddReagent.Uid = "NoError";
                   // btnAddReagent.Background = Brushes.Transparent;
                    //btnAddReagent_Click(null, null);
                }
            ));
        }    

        private void btnAddReagent_Click(object sender, RoutedEventArgs e)
        {
            AddReagentsAndSupplies addReagentWindow = new AddReagentsAndSupplies(1);
            bool dlg_result = (bool)addReagentWindow.ShowDialog();
            if (dlg_result)
            {
                bool valid = new ReagentDetectService().CalacCorrentVolumeIsValid();
                if (valid)
                {
                    btnAddReagent.Uid = "NoError";
                  //  btnAddReagent.Background = Brushes.Transparent;
                }
            }
        }

        private void btnTrash_Click(object sender, RoutedEventArgs e)
        {
            if (FinishedRotation != null)
            {

                RubbishAlert rubbishAlert = new RubbishAlert(FinishedRotation.RotationID);
                rubbishAlert.ShowDialog();

                if (sender == null)
                {
                    PCRRubbishAlert frm = new PCRRubbishAlert(FinishedRotation.RotationID);
                    frm.ShowDialog();
                    btnTrash.Uid = "NoError";
                }
            }
        }
    }
}
