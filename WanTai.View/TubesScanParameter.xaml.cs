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
using WanTai.DataModel;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for TubesScanParameter.xaml
    /// </summary>
    public partial class TubesScanParameter : Window
    {
        public TubesScanParameter()
        {
            InitializeComponent();
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            int range = 6;
            if (SessionInfo.WorkDeskType == "100")
            {
                range = 6;
            }
            else if (SessionInfo.WorkDeskType == "150")
            {
                range = 18;
            }
            else if (SessionInfo.WorkDeskType == "200")
            {
                range = 36;
            }
            string message = String.Format("请输入1-{0}的数字！", range);

            int ColumnCount = 0;
            if (!int.TryParse(txtColumnCount.Text, out ColumnCount))
            {
                MessageBox.Show(message, "系统提示！");
                return;
            }
            if (ColumnCount < 1 || ColumnCount > range)
            {
                MessageBox.Show(message, "系统提示！");
                return;
            }
            new WanTai.Controller.TubesController().SaveScanParameter(ColumnCount);
            this.DialogResult = true;
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
    }
}
