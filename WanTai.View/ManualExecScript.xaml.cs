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
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for TecanRestoration.xaml
    /// </summary>
    public partial class ManualExecScript : Window
    {
        public ManualExecScript()
        {
            InitializeComponent();
        }
        public string scriptFileName { get; set; }
        bool RunFalg = true;
        bool PauseFlag = false;
        private void btnScript_Click(object sender, RoutedEventArgs e)
        {
            btnScript1.IsEnabled = false;
            btnScript2.IsEnabled = false;
            btnScript3.IsEnabled = false;
            btnScript4.IsEnabled = false;
            btnScript5.IsEnabled = false;
            btnPause.IsEnabled = true;
            btnStop.IsEnabled = true;
            btnClose.IsEnabled = false;
            RunFalg = true;
            PauseFlag = false;
            scriptFileName = (sender as Button).DataContext.ToString();
            Thread thread = new Thread(new ThreadStart(delegate() {
                while (RunFalg)
                {
                    this.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        if (progressBar1.Value == progressBar1.Maximum)
                            progressBar1.Value = progressBar1.Maximum - 30;
                        if (!PauseFlag)
                        {
                            progressBar1.Value += 1;
                        }
                    });
                    Thread.Sleep(100 * 1);
                 }
                 this.Dispatcher.BeginInvoke((Action)delegate()
                 {
                     if (!RunFalg)
                     {
                         progressBar1.Value = 0;
                         btnScript1.IsEnabled = true;
                         btnScript2.IsEnabled = true;
                         btnScript3.IsEnabled = true;
                         btnScript4.IsEnabled = true;
                         btnScript5.IsEnabled = true;
                         btnPause.IsEnabled = false;
                         btnStop.IsEnabled = false;
                         btnClose.IsEnabled = true;
                     }
                 });
            }));
            thread.Start();
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state)
            {
                try
                {
                    WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                    //string scriptFileName = WanTai.Common.Configuration.GetTecanRestorationScriptName();
                    processor.StartScript(scriptFileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                if (!PauseFlag)
                    RunFalg = false;

            }));

            //Thread threadRunScript = new Thread(new ThreadStart(delegate()
            //{
            //   
            //}));
            //threadRunScript.Start();
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MaxHeight = this.Height;
            this.MinHeight = this.Height;
            this.MaxWidth = this.Width;
            this.MinWidth = this.Width;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            PauseFlag = false;
            RunFalg = false;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                btnStop.IsEnabled = false;
                btnPause.IsEnabled = false;
            });

            try
            {
                WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                processor.StopScript();
                processor.StopScript();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                btnStop.IsEnabled = true;
                btnPause.IsEnabled = true;
            });

        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke((Action)delegate()
            {
                btnStop.IsEnabled = false;
                btnPause.IsEnabled = false;
            });
            if ((String)btnPause.Content == "暂停")
            {
                try
                {
                    WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                    if (processor.PauseScript())
                    {
                        PauseFlag = true;
                        btnPause.Content = "继续";
                    }
                    else
                    {
                        MessageBox.Show("当前状态无法暂停！", "系统提示！");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    btnStop.IsEnabled = true;
                    btnPause.IsEnabled = true;
                });

            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state)
                {
                    try
                    {
                        PauseFlag = false;
                        WanTai.Controller.EVO.IProcessor processor = WanTai.Controller.EVO.ProcessorFactory.GetProcessor();
                        processor.ResumeScript();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    if (!PauseFlag)
                        RunFalg = false;

                }));
                btnPause.Content = "暂停";
                this.Dispatcher.BeginInvoke((Action)delegate()
                {
                    btnStop.IsEnabled = true;
                    btnPause.IsEnabled = true;
                });
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnScript2_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
