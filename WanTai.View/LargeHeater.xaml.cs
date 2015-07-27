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
using WanTai.View.Control;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for LargeHeater.xaml
    /// </summary>
    public partial class LargeHeater : Window
    {
        public LargeHeater()
        {
            InitializeComponent();
        }

        public LargeHeater(Heater heater)
        {
            InitializeComponent();
            Heater newHeater = new Heater();
            newHeater.Plates = heater.Plates;
            //newHeater.btnLarge.IsEnabled = false;
            this.MainGrid.Children.Add(newHeater);

        }
    }
}
