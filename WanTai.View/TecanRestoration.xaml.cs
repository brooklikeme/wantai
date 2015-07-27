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
    public partial class TecanRestoration : Window
    {
        public TecanRestoration()
        {
            InitializeComponent();
        }
        public string scriptFileName { get; set; }
        bool RunFalg = true;
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            btnEnter.IsEnabled = false;
            Thread thread = new Thread(new ThreadStart(delegate() {
                while (RunFalg)
                {
                    this.Dispatcher.BeginInvoke((Action)delegate()
                    {
                        if (progressBar1.Value == progressBar1.Maximum)
                            progressBar1.Maximum += 30;
                         progressBar1.Value += 1;
                    });
                    Thread.Sleep(100 * 1);
                }
                 this.Dispatcher.BeginInvoke((Action)delegate()
                 {
                     if (!ExitFalg)
                     {
                         btnEnter.IsEnabled = true;
                         this.DialogResult = true;
                     }
                 });
            }));
            thread.Start();
            Thread threadRunScript = new Thread(new ThreadStart(delegate()
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
                RunFalg = false;
               
            }));
            threadRunScript.Start();
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
        bool ExitFalg = false;
        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ExitFalg = true;
            RunFalg = false;
        }
    }
}
