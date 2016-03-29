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
using System.Data.Entity;
using WanTai.DataModel;
using System.IO;
using System.Windows.Markup;
using System.Data;
using System.Reflection;
using WanTai.Controller;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for TubesDetailView.xaml
    /// </summary>
    public partial class TubesDetailView : Window
    {
        private Guid batchID_;

        public TubesDetailView()
        {
            InitializeComponent();
        }
        
        public DataTable Tubes
        {
            set
            {
                dg_Bules.ItemsSource = value.DefaultView;
            }
        }
        public Guid BatchID
        {
            set
            {
                batchID_ = value;
                List<WanTai.DataModel.Configuration.LiquidType> LiquidTypeList = WanTai.Common.Configuration.GetLiquidTypes();
                dg_Bules.ItemsSource = new WanTai.Controller.TubesController().GetTubes(value, LiquidTypeList, SessionInfo.BatchType).DefaultView;
            }
        }

        public void ViewExperimentBatch(Guid batchID, string BatchType)
        {
            batchID_ = batchID;
            this.MixTime.Items.Add("1");
            if (BatchType == "A")
            {
                this.MixTime.Items.Add("2");
            }
            this.MixTime.SelectedIndex = 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var TestItems = new TestItemController().GetActiveTestItemConfigurations();
            StackPanel sp_pointout = new StackPanel();
            Label label = new Label();
            label.Content = this.Title;
            label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            label.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            sp_pointout.Children.Add(label);
            foreach (TestingItemConfiguration _TestingItem in TestItems)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Height = 20;
                textBlock.Margin = new Thickness(5, 0, 0, 0);
                textBlock.Width = 20;
                textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                textBlock.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_TestingItem.TestingItemColor));
                sp_pointout.Children.Add(textBlock);

                label = new Label();
                label.Content = _TestingItem.TestingItemName;
                label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                label.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                sp_pointout.Children.Add(label);
            }
       
        //    dg_Bules.Columns.Clear();
            StringBuilder SBuilder = new StringBuilder();
            for (int i = 1; i <0; i++)
            {   
                SBuilder.Append("<DataGridTemplateColumn Width=\"*\" ");
                //SBuilder.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
                //SBuilder.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
                //SBuilder.Append("xmlns:local='clr-namespace:WanTai.View;assembly=WanTai.View' ");
                //SBuilder.Append("xmlns:my1='clr-namespace:ScottLogic.PieChart;assembly=ScottLogic.PieChart' ");
                SBuilder.Append(">");
                //SBuilder.Append("<DataGridTemplateColumn.HeaderTemplate>");
                //SBuilder.Append("<DataTemplate>");
                //SBuilder.Append("<StackPanel Orientation=\"Horizontal\">");
                //SBuilder.Append("<TextBlock  HorizontalAlignment=\"Left\"  Text=\""+i.ToString()+"\" VerticalAlignment=\"Top\" />");
                //SBuilder.Append("<TextBlock  HorizontalAlignment=\"Left\"  Text=\"{Binding Path=TubePosBarCode" + i.ToString() + ",Converter={StaticResource TubeShowID}}\" Margin=\"10,0,0,0\" VerticalAlignment=\"Top\" />");
                //SBuilder.Append("</StackPanel>");
                //SBuilder.Append("</DataTemplate>");
                //SBuilder.Append("</DataGridTemplateColumn.HeaderTemplate>");

                SBuilder.Append("<DataGridTemplateColumn.CellTemplate>");
                SBuilder.Append("<DataTemplate>");
                SBuilder.Append("<StackPanel Orientation=\"Horizontal\">");
                SBuilder.Append("<StackPanel VerticalAlignment=\"Center\">");
                SBuilder.Append("<my1:PieLoayout  ToolTip=\"{Binding Path=TubeType" + i.ToString() + "}\" Visibility=\"{Binding Path=Visibility" + i.ToString() + "}\"  PlottedProperty=\"{Binding Path=TextItemCount" + i.ToString() + "}\"   x:Name=\"pie_" + i.ToString() + "\"   />");
                SBuilder.Append("</StackPanel>");
                SBuilder.Append("<StackPanel Margin=\"10,0,0,0\" Orientation=\"Vertical\" VerticalAlignment=\"Center\">");
                SBuilder.Append("<TextBlock  HorizontalAlignment=\"Left\"  Text=\"{Binding Path=BarCode" + i.ToString() + "}\" VerticalAlignment=\"Top\" />");
                SBuilder.Append("<ContentControl Content=\"{Binding Path=DetailView" + i.ToString() + ",  Converter={StaticResource hv}}\" />");
                //SBuilder.Append("<StackPanel  Orientation=\"Horizontal\">");
                //SBuilder.Append("<TextBlock  HorizontalAlignment=\"Left\"  Text=\"{Binding Path=TubeGroupName" + i.ToString() + "}\" VerticalAlignment=\"Top\" />");
                //SBuilder.Append("<TextBlock  HorizontalAlignment=\"Left\" Margin=\"5,0,0,0\"  Text=\"{Binding Path=PoolingRulesName" + i.ToString() + "}\" VerticalAlignment=\"Top\" />");
                //SBuilder.Append("   </StackPanel>");
                SBuilder.Append("</StackPanel>");
                SBuilder.Append("</StackPanel>");
                SBuilder.Append("</DataTemplate>");
                SBuilder.Append("</DataGridTemplateColumn.CellTemplate>");
                SBuilder.Append("</DataGridTemplateColumn>");

                //Stream stream = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(SBuilder.ToString()));
              //  DataGridTemplateColumn colIcon = XamlReader.Load(stream) as DataGridTemplateColumn;
              //  colIcon.Header = i.ToString();
               // dg_Bules.Columns.Add(colIcon);
            }       
        }

        private void dg_Bules_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            if (e.Row.GetIndex() == 0)
            {
                int i = 0;
                foreach (DataGridColumn Column in dg_Bules.Columns)
                {
                    string TubePosBarCode = (((System.Data.DataRowView)(e.Row.Item))).Row.Table.Rows[0]["TubePosBarCode" + (++i).ToString()].ToString();
                    Column.Header = (i).ToString();
                    if (!string.IsNullOrEmpty(TubePosBarCode))
                        Column.Header += "   ID:" + TubePosBarCode;

                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            dg_Bules.RowHeight = (dg_Bules.ActualHeight - dg_Bules.ColumnHeaderHeight) / 16 -1.2;
        }

        private void MixTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<WanTai.DataModel.Configuration.LiquidType> LiquidTypeList = WanTai.Common.Configuration.GetLiquidTypes();
            if (this.MixTime.Items.Count == 2)
            {
                if (this.MixTime.SelectedIndex == 0)
                {
                    dg_Bules.ItemsSource = new WanTai.Controller.TubesController().GetTubes(this.batchID_, LiquidTypeList, "A").DefaultView;
                }
                else
                {
                    dg_Bules.ItemsSource = new WanTai.Controller.TubesController().GetTubes(this.batchID_, LiquidTypeList, "B").DefaultView;

                }
            }
            else
            {
                dg_Bules.ItemsSource = new WanTai.Controller.TubesController().GetTubes(this.batchID_, LiquidTypeList, "null").DefaultView;

            }
        }
    }
}
