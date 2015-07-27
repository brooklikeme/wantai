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
using WanTai.Controller;

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for RotationOperatesViewDetail.xaml
    /// </summary>
    public partial class RotationOperatesViewDetail : Window
    {        
        DataTable dataTable = new DataTable();
        private Guid rotationId;
        private string rotationName;

        public RotationOperatesViewDetail()
        {
            InitializeComponent();
        }

        public Guid RotationId
        {
            set { rotationId = value; }
        }

        public string RotationName
        {
            set { rotationName = value; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataTable.Columns.Add("Number", typeof(int));
            dataTable.Columns.Add("OperationID", typeof(Guid));
            dataTable.Columns.Add("OperationName", typeof(string));
            dataTable.Columns.Add("StartTime", typeof(string));
            dataTable.Columns.Add("EndTime", typeof(string));
            dataTable.Columns.Add("State", typeof(string));
            dataTable.Columns.Add("ErrorLog", typeof(string));
            dataTable.Columns.Add("Color", typeof(string));
            dataGrid_view.ItemsSource = dataTable.DefaultView;

            this.RotationName_label.Content = rotationName;
            initDataGrid();
        }

        private void initDataGrid()
        {
            ConfigRotationController rotationController = new ConfigRotationController();
            rotationController.GetRotationOperates(rotationId, dataTable, WindowCustomizer.alternativeColor, WindowCustomizer.defaultColor);            
        }

        private void close_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }  
    }
}
