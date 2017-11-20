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
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using WanTai.DataModel;
using System.Data.Entity;
using System.Windows.Media.Animation;
using System.Threading;
using System.ComponentModel;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            //   WanTai.Controller.SampleTrackingController.SampleTracking();
            this.DialogResult = false;
        }

        private void btn_Enter_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txt_LoginName.Text))
            {
                MessageBox.Show("请输入用户名！", "系统提示");
                txt_LoginName.Focus();
                return;
            }
            string ErrMsg = "操作成功";
            UserInfo LoginUser = new WanTai.Controller.LoginController().CheckLoginUser(txt_LoginName.Text, txt_LoginPassWord.Password, out ErrMsg);
            if (LoginUser == null)
            {
                MessageBox.Show(ErrMsg, "系统提示");
                return;
            }
            SessionInfo.LoginName = LoginUser.LoginName;
            WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "登陆成功", SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
            this.DialogResult = true;
        }
        public string[] RunArgs;
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
            Stream imageStream = Application.GetResourceStream(new Uri("/WanTag;component/Resources/Login_Bg.png", UriKind.Relative)).Stream;
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageStream);
            this.imagBackground.Image = bitmap;
            txt_LoginName.Focus();
            //第一种
            // DisableMaxmizebox(true);
            //第二种
            this.MaxHeight = this.Height;
            this.MinHeight = this.Height;
            this.MaxWidth = this.Width;
            this.MinWidth = this.Width;

            bool falg = true;
            //-u aa -p bb
            if (RunArgs != null && RunArgs.Length >= 4)
            {
                barRun.Visibility = System.Windows.Visibility.Visible;
                Thread threadBar = new Thread(new ThreadStart(delegate
                {
                    while (falg)
                    {
                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                        {
                            if (barRun.Value < 100)
                                barRun.Value++;
                        }));
                        Thread.Sleep(100 * 1);
                    }
                }));
                threadBar.Start();
               // StartTheStoryboardTimer();
                labLoginName.Visibility = System.Windows.Visibility.Hidden;
                txt_LoginName.Visibility = System.Windows.Visibility.Hidden;
                labLoginPassWord.Visibility = System.Windows.Visibility.Hidden;
                txt_LoginPassWord.Visibility = System.Windows.Visibility.Hidden;
                btn_Enter.Visibility = System.Windows.Visibility.Hidden;
                UserInfo LoginUser = null;
                Thread thread = new Thread(new ThreadStart(delegate
                {
                    string userName = RunArgs[1];
                    string passWord = RunArgs[3];
                    string ErrMsg = "操作成功";
                    LoginUser = new WanTai.Controller.LoginController().CheckLoginUser(userName, passWord, out ErrMsg);
                    if (LoginUser == null)
                    {
                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(
                        delegate()
                        {
                            barRun.Visibility = System.Windows.Visibility.Hidden;
                            labLoginName.Visibility = System.Windows.Visibility.Visible;
                            txt_LoginName.Visibility = System.Windows.Visibility.Visible;
                            labLoginPassWord.Visibility = System.Windows.Visibility.Visible;
                            txt_LoginPassWord.Visibility = System.Windows.Visibility.Visible;
                            btn_Enter.Visibility = System.Windows.Visibility.Visible;
                            txt_LoginName.Focus();
                        }));
                        falg = false;
                        return;
                    }
                    SessionInfo.LoginName = LoginUser.LoginName;
                    WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "登陆成功", SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate(){
                        falg = false;
                        this.DialogResult = true;
                     }));
                }));
                thread.Start();
               
            }
            else
            {
                barRun.Visibility = System.Windows.Visibility.Hidden;
                labLoginName.Visibility = System.Windows.Visibility.Visible;
                txt_LoginName.Visibility = System.Windows.Visibility.Visible;
                labLoginPassWord.Visibility = System.Windows.Visibility.Visible;
                txt_LoginPassWord.Visibility = System.Windows.Visibility.Visible;
                btn_Enter.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            imagBackground.Dispose();
        }
    }
}
