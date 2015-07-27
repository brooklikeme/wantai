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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

using ScottLogic.Shapes;
using ScottLogic.Util;
using System.Windows.Media.Animation;
using ScottLogic.ScottLogic.PieChart;
namespace ScottLogic.PieChart
{
    /// <summary>
    /// Interaction logic for PieLoayout.xaml
    /// </summary>
    public partial class PieLoayout : UserControl
    {
        public PieLoayout()
        {
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PlottedPropertyProperty, typeof(PieLoayout));
            dpd.AddValueChanged(this, PlottedPropertyChanged);

            //dpd = DependencyPropertyDescriptor.FromProperty(PlottedBackgroundPropertyProperty, typeof(PieLoayout));
            //dpd.AddValueChanged(this, BackgroundPropertyChanged);
            InitializeComponent();
        }
        public String PlottedProperty
        {
            get { return GetPlottedProperty(this); }
            set { SetPlottedProperty(this, value); }
        }
       
        // PlottedProperty dependency property
        public static readonly DependencyProperty PlottedPropertyProperty =
                       DependencyProperty.RegisterAttached("PlottedProperty", typeof(String), typeof(PieLoayout),
                       new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Inherits));

        // PlottedProperty attached property accessors
        public static void SetPlottedProperty(UIElement element, String value)
        {
            element.SetValue(PlottedPropertyProperty, value);
        }
        public static String GetPlottedProperty(UIElement element)
        {
            return (String)element.GetValue(PlottedPropertyProperty);
        }
        public String PlottedBackgroundProperty
        {
            get { return GetPlottedBackgroundProperty(this); }
            set { SetPlottedBackgroundProperty(this, value); }
        }
        // PlottedProperty dependency property
        public static readonly DependencyProperty PlottedBackgroundPropertyProperty =
                       DependencyProperty.RegisterAttached("PlottedBackgroundProperty", typeof(String), typeof(PieLoayout),
                       new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Inherits));

        // PlottedProperty attached property accessors
        public static void SetPlottedBackgroundProperty(UIElement element, String value)
        {
            element.SetValue(PlottedBackgroundPropertyProperty, value);
        }
        public static String GetPlottedBackgroundProperty(UIElement element)
        {
            return (String)element.GetValue(PlottedBackgroundPropertyProperty);
        }
        /// <summary>
        /// A class which selects a color based on the item being rendered.
        /// </summary>
        public IColorSelector ColorSelector
        {
            get { return GetColorSelector(this); }
            set { SetColorSelector(this, value); }
        }

        // ColorSelector dependency property
        public static readonly DependencyProperty ColorSelectorProperty =
                       DependencyProperty.RegisterAttached("ColorSelectorProperty", typeof(IColorSelector), typeof(PieLoayout),
                       new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        // ColorSelector attached property accessors
        public static void SetColorSelector(UIElement element, IColorSelector value)
        {
            element.SetValue(ColorSelectorProperty, value);
        }
        public static IColorSelector GetColorSelector(UIElement element)
        {
            return (IColorSelector)element.GetValue(ColorSelectorProperty);
        }
     
        private void PlottedPropertyChanged(object sender, EventArgs e)
        {
            ConstructPiePieces();
        }
        private List<PiePiece> piePieces = new List<PiePiece>();
        private void ConstructPiePieces()
        {
            string[] str= PlottedProperty.Split(',');
            if (int.Parse(str[0])==0)
            {
                SP.Children.Clear();
                return;
            }
            if (!string.IsNullOrEmpty(str[2])) solidColorBrush.Color = (Color)ColorConverter.ConvertFromString(str[2]);
            SP.Children.Clear();
            if (str.Length == 4 || str.Length == 3)
            {
                if (str.Length == 4)
                    solidColorBrush.Color = (Color)ColorConverter.ConvertFromString(str[3].Split(';')[1]);

                StackPanel _StackPanel = new StackPanel();
                _StackPanel.Width = 24;
                _StackPanel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                _StackPanel.Height = 24;
                _StackPanel.Background = Resources["MyDrawingBrush"] as DrawingBrush;
                SP.Children.Add(_StackPanel);

                return;
            }
            double halfWidth = this.Width / 2;
            double innerRadius = 2;//halfWidth * HoleSize;
            // add the pie pieces
            SP.Children.Clear();
            piePieces.Clear();
            double accumulativeAngle = 0;
            
            StringBuilder stringBuilder = new StringBuilder();//是否扫描到，采血管、阴阳补，选择颜色
            int PlottedCount=0;
            for (int item = 3; item < str.Length; item++)
            {
                string strtmp = str[item].ToString().Split(';')[1];
                if (stringBuilder.ToString().IndexOf(strtmp) >= 0)
                    continue;

                stringBuilder.Append(strtmp + ",");
                PlottedCount++;
            }
            if (PlottedCount== 1)
            {
                solidColorBrush.Color = (Color)ColorConverter.ConvertFromString(stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length-1));

                StackPanel _StackPanel = new StackPanel();
                _StackPanel.Width = 24;
                _StackPanel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                _StackPanel.Height = 24;
                _StackPanel.Background = Resources["MyDrawingBrush"] as DrawingBrush;
                SP.Children.Add(_StackPanel);

                return;
            }
            for (int item = 0; item < PlottedCount; item++)
            {
                double wedgeAngle = 360 / (PlottedCount);
                PiePiece piece = new PiePiece()
                {
                    Radius = halfWidth,
                    InnerRadius = innerRadius,
                    CentreX = halfWidth,
                    CentreY = halfWidth,
                    PushOut = 0,
                    WedgeAngle = wedgeAngle,
                    PieceValue = wedgeAngle,
                    RotationAngle = accumulativeAngle,
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(stringBuilder.ToString().Split(',')[item]))
                    //Fill =ColorSelector != null ? ColorSelector.SelectBrush(item, item) : Brushes.Black,
                    // record the index of the item which this pie slice represents
                    //Tag = item,
                   // ToolTip = new ToolTip()
                };
                //piece.ToolTipOpening += new ToolTipEventHandler(PiePieceToolTipOpening);
                //piece.MouseUp += new MouseButtonEventHandler(PiePieceMouseUp);
                piePieces.Add(piece);
                SP.Children.Add(piece);
                accumulativeAngle += wedgeAngle;
            }
        }
        private Brush GetColorSelector(int index)
        {
            //Colors.Blue, Colors.Red, Colors.Yellow, Colors.Green, Colors.LightGray, Colors.Magenta, Colors.Brown 
            //<SolidColorBrush Color="#9F15C3"/>
            //<SolidColorBrush Color="#FF8E01"/>
            //<SolidColorBrush Color="#339933"/>
            //<SolidColorBrush Color="#00AAFF"/>
            //<SolidColorBrush Color="#818183"/>
            //<SolidColorBrush Color="#000033"/>
       
            if (index == 0)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9F15C3"));
            if (index == 1)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8E01"));
            if (index == 2)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#339933"));
            if (index == 3)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00AAFF"));
            if (index == 4)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#818183"));
            if (index == 5)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000033"));
            return Brushes.Black;
        }
    }
}
