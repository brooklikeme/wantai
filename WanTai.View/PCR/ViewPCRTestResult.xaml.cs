using System;
using System.Collections.Generic;
using System.Collections;
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
using System.IO;
using System.Data.OleDb;
using System.Data;

using WanTai.DataModel;
using WanTai.Controller.PCR;

namespace WanTai.View.PCR
{
    /// <summary>
    /// Interaction logic for ViewPCRTestResult.xaml
    /// </summary>
    public partial class ViewPCRTestResult : Window
    {
        private DataRow currentRowData;
        EditPCRTestResultController controller = new EditPCRTestResultController();

        public ViewPCRTestResult()
        {
            InitializeComponent();            
        }

        public void SetCurrentData(DataRow rowData)
        {
            currentRowData = rowData;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Guid itemId = (Guid)currentRowData["PCRTestItemID"];
            label_TubeBarcode.Content = (string)currentRowData["TubeBarCode"];
            TextBlock_TubePosition.Text = (string)currentRowData["TubePosition"];
            label_TestingItemName.Content = (string)currentRowData["TestingItemName"];
            TextBlock_PCRTestContent.Text = controller.QueryPCRTestResult(itemId);
            textbox_PCRTestResult.Text = (string)currentRowData["PCRTestResult"];
        }    

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }        
    }    
}
