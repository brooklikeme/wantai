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

namespace WanTai.View.Control
{
    /// <summary>
    /// Interaction logic for Coordinate.xaml
    /// </summary>
    public partial class Coordinate : UserControl
    {
        public Coordinate()
        {
            InitializeComponent();
            Line xLine = new Line();
            xLine.Stroke = Brushes.Black;
            xLine.StrokeThickness = 1;
            xLine.X1 = 0;
            xLine.Y1 = 5;
            xLine.X2 = 710;
            xLine.Y2 = 5;
            CoordinateGrid.Children.Add(xLine);

            int pointCount = 700 / 10;
            for (int i = 0; i <= pointCount; i++)
            {
                Line pointLine = new Line();
                pointLine.Stroke = Brushes.Black;
                pointLine.StrokeThickness = 1;
                pointLine.X1 = 10 + i * 10;
                pointLine.Y1 = 5;
                pointLine.X2 = 10 + i * 10;
                pointLine.Y2 = i % 5 == 0 ? 10 : 7;
                CoordinateGrid.Children.Add(pointLine);
                if (i % 5 == 0)
                {
                    TextBlock text = new TextBlock();
                    text.Text = i.ToString();
                    text.Margin = new Thickness(10 + i * 10 - 5, 10, 0, 0);
                    CoordinateGrid.Children.Add(text);
                }
            }
        }

        public Coordinate(double length, int unitLength)
        {
            InitializeComponent();
            Line xLine = new Line();
            xLine.Stroke = Brushes.Black;
            xLine.StrokeThickness = 1;
            xLine.X1 = 0;
            xLine.Y1 = 5;
            xLine.X2 = length;
            xLine.Y2 = 5;
            CoordinateGrid.Children.Add(xLine);

            int pointCount = (int)length / unitLength;
            for (int i = 0; i <= pointCount; i++)
            {
                Line pointLine = new Line();
                pointLine.Stroke = Brushes.Black;
                pointLine.StrokeThickness = 1;
                pointLine.X1 = unitLength + i * 10;
                pointLine.Y1 = 5;
                pointLine.X2 = unitLength + i * 10;
                pointLine.Y2 = i % 5 == 0 ? 10 : 7;
                CoordinateGrid.Children.Add(pointLine);
                if (i % 5 == 0)
                {
                    TextBlock text = new TextBlock();
                    text.Text = i.ToString();
                    text.Margin = new Thickness(unitLength + i * unitLength - 5, 10, 0, 0);
                    CoordinateGrid.Children.Add(text);
                }
            }
        }
    }
}
