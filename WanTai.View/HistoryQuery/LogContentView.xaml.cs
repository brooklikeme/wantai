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

namespace WanTai.View.HistoryQuery
{
    /// <summary>
    /// Interaction logic for LogContentView.xaml
    /// </summary>
    public partial class LogContentView : Window
    {
        private string currentLogContent;

        public LogContentView()
        {
            InitializeComponent();            
        }

        public void SetLogContent(string _currentLogContent)
        {
            currentLogContent = _currentLogContent;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.TextBlock_LogContent.Text = currentLogContent;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }        
    }    
}
