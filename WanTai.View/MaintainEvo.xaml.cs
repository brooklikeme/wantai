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

using WanTai.DataModel;
using WanTai.Controller.EVO;
using WanTai.Controller;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for MaintainEvo.xaml
    /// </summary>
    public partial class MaintainEvo : Window
    {       
        public MaintainEvo()
        {
            InitializeComponent();
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            APIHelper.SetHook();
            this.MaxHeight = this.Height;
            this.MinHeight = this.Height;
            this.MaxWidth = this.Width;
            this.MinWidth = this.Width;
        }
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hMenu, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        private void DisableMaxmizebox(bool isDisable)
        {
            int GWL_STYLE = -16;
            int WS_MAXIMIZEBOX = 0x00010000;
            int WS_MINIMIZEBOX = 0x00020000;
            int SWP_NOSIZE = 0x0001;
            int SWP_NOMOVE = 0x0002;
            int SWP_FRAMECHANGED = 0x0020;
            IntPtr handle = new WindowInteropHelper(this).Handle;
            int nStyle = GetWindowLong(handle, GWL_STYLE);
            if (isDisable)
            {
                nStyle &= ~(WS_MAXIMIZEBOX);
                nStyle &= ~(WS_MINIMIZEBOX);
            }
            else
            {
                nStyle |= WS_MAXIMIZEBOX;
                nStyle |= WS_MINIMIZEBOX;
            }
            SetWindowLong(handle, GWL_STYLE, nStyle);
            SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_FRAMECHANGED);
        }  
        bool RunFalg = true;
        private void execute_Click(object sender, RoutedEventArgs e)
        {
            this.cancel.IsEnabled = false;
            execute.IsEnabled = false;
          
            int RunIndex = 1;
            Thread thread = new Thread(new ThreadStart(delegate()
            {
                while (RunFalg)
                {
                    if (RunIndex == 0) continue;
                    this.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        ProgressBar progressBar =progressBar1;
                        if (RunIndex==1)
                            progressBar = progressBar1;
                        if (RunIndex == 2)
                            progressBar = progressBar2;
                        if (RunIndex == 3)
                            progressBar = progressBar3;

                        if (progressBar.Value == progressBar.Maximum)
                            progressBar.Maximum += 30;
                        progressBar.Value += 1;
                    });
                    Thread.Sleep(100 * 1);
                }
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    if (!ExitFalg)
                    {
                        execute.IsEnabled = true;
                        this.DialogResult = true;
                    }
                });
            }));
            thread.Start();
            Thread threadRunScript = new Thread(new ThreadStart(delegate()
            {
                string[] scripotNames= WanTai.Common.Configuration.GetMaintainMonthEvoScriptName().Split(',');
                foreach(string scriptName in scripotNames)
                {
                    if (String.IsNullOrEmpty(scriptName))
                    {
                        RunIndex++;
                        continue;
                    }
                    int _RunIndex = RunIndex;
                    RunIndex = 0; 
                    if (ExitFalg) return;
                    MessageBox.Show("请添加洗液" + _RunIndex.ToString() + ",点确认开始！", "系统提示");
                    RunIndex = _RunIndex;
                    try
                    {
                        if (ExitFalg) return;
                        IProcessor processor = ProcessorFactory.GetProcessor();
                        processor.StartScript(scriptName);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = ex.Message + System.Environment.NewLine + ex.StackTrace;
                        LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
                        MessageBox.Show("执行系统维护错误:" + ex.Message, "系统提示");
                        RunFalg = false;
                        return;
                    }
                    
                    RunIndex++;
                }             
                RunFalg = false;
                MessageBox.Show("执行系统维护成功", "系统提示");

            }));
            threadRunScript.Start();

        }              

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        bool ExitFalg = false;
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ExitFalg = true;
            APIHelper.UnHook();
            RunFalg = false;
        }        
    }
}
