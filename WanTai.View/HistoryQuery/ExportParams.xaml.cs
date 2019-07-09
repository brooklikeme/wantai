using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WanTai.DataModel;
using WanTai.Controller;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for NewExperiment.xaml
    /// </summary>
    public partial class ExportParams : Window
    {

        public ExportParams()
        {
            InitializeComponent();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            int startRow = 0;
            int endRow = 1;

            if (!string.IsNullOrEmpty(txtStartRow.Text) && !int.TryParse(txtStartRow.Text, out startRow)) 
            {
                errInfo.Text = "开始记录必须是空或者有效的数字！";
                return;
            }
            else if (!string.IsNullOrEmpty(txtEndRow.Text) && !int.TryParse(txtEndRow.Text, out endRow))
            {
                errInfo.Text = "结束记录必须是空或者有效的数字！";
                return;
            }
            else if (startRow > endRow)
            {
                errInfo.Text = "开始记录不能大于结束记录！";
                return;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
         
    }
}
