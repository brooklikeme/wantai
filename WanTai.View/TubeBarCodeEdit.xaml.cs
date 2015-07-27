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
using System.Data;
using WanTai.DataModel;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for TubeBarCodeEdit.xaml
    /// </summary>
    public partial class TubeBarCodeEdit : Window
    {
        public TubeBarCodeEdit()
        {
            InitializeComponent();
        }
        public DataGridCellInfo Cell { get; set; }
        public string BarCode { get; set; }
        private void dg_Tubes_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = CommFuntion.GetDataGridCellRowIndex(Cell).ToString();
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txt_BarCode.Text.Trim()))
            {
                MessageBox.Show("条形码不能为空！", "系统提示");
                txt_BarCode.Focus();
                return;
            }
            BarCode = txt_BarCode.Text.Trim();
            this.DialogResult = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MinHeight=this.MaxHeight = this.Height;

            this.MinWidth=this.MaxWidth = this.Width;
             
            DataTable table = ((System.Data.DataRowView)(Cell.Item)).DataView.Table;
            dg_Tubes.Columns[0].Header = CommFuntion.GetDataGridCellColumnIndex(Cell).ToString();
            int ColumnIndex=CommFuntion.GetDataGridCellColumnIndex(Cell);
            int RowIndex=CommFuntion.GetDataGridCellRowIndex(Cell)-1;
            dg_Tubes.Items.Add(new Tube() { BarCode = table.Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString(), TextItemCount = table.Rows[RowIndex]["TextItemCount" + ColumnIndex.ToString()].ToString() });
      
            txt_BarCode.Text = table.Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString();

        }
    }
}
