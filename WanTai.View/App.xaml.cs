using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using WanTai.DataModel;
using System.IO;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using WanTai.Controller.EVO;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        [STAThread]
        public static void Main(string[] args)
        {
            bool isAppRunning = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "WanTag.exe", out isAppRunning);
            if (!isAppRunning)
            {
                MessageBox.Show("程序已运行!", "系统提示!");
                return; 
            }
            RunArgs = args;
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            App app = new App();
            //经反复测试得出的结论：这里必须用OnExplicitShutdown强调显式结束应用程序，先启动登录==〉后启动主程序的运行机制才能得以实现
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            EnsureApplicationResources();
            app.Run();
        }
        private static string[] RunArgs;
        private static void EnsureApplicationResources()
        {
            // merge in your application resources
            Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(
                    new Uri("WanTag;component/Style/BaseStyle.xaml",
                    UriKind.Relative)) as ResourceDictionary);

             Application.Current.Resources.MergedDictionaries.Add(
                Application.LoadComponent(
                    new Uri("WanTag;component/Style/MultiComboBoxStyle.xaml",
                    UriKind.Relative)) as ResourceDictionary);

             //Application.Current.Resources.MergedDictionaries.Add(
             //   Application.LoadComponent(
             //       new Uri("WanTai.View;component/Style/ButtonStyle.xaml",
             //       UriKind.Relative)) as ResourceDictionary);
        }
        private void Main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Main_Closed(object sender, System.EventArgs e)
        {
            //if (!ProcessorFactory.HasClosed)
            //{
            //    IProcessor processor = ProcessorFactory.GetProcessor();
            //    processor.Close();
            //}           
            Application.Current.Shutdown();   //显式结束应用程序
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Login loginWindow = new Login();
            loginWindow.RunArgs = RunArgs;
            loginWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loginWindow.ShowDialog();
            if ((bool)loginWindow.DialogResult)
            {
                try
                { 
                    this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                    MainWindow main = new MainWindow();
                    main.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    main.Closed += new EventHandler(Main_Closed);
                    main.Closing += new System.ComponentModel.CancelEventHandler(Main_Closing); 
                    main.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(" 提示:\n\n" + ex.Message + ex.StackTrace, "系统提示！");
                    WanTai.Common.CommonFunction.WriteLog(ex.ToString());
                }
            }
            else
            {
                Application.Current.Shutdown();   //显式结束应用程序
            }
            
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show("系统出现错误将退出!\n\n"
                + " 提示:\n\n" + ex.Message,
                "系统提示！");
            WanTai.Common.CommonFunction.WriteLog(ex.Message);
            WanTai.Common.CommonFunction.WriteLog(ex.StackTrace);
        }
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //Exception ex = e.Exception;
            //MessageBox.Show("系统出现错误将退出!\n\n"
            //    + " 提示:\n\n" + ex.Message + ex.StackTrace,
            //    "系统提示！");
            //WanTai.Common.CommonFunction.WriteLog(ex.Message);
            //WanTai.Common.CommonFunction.WriteLog(ex.StackTrace);
        }        
    }
}