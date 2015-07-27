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
using System.IO;
namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for LoadFrom.xaml
    /// </summary>
    public partial class LoadFrom : Window
    {
        public LoadFrom()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Stream imageStream = Application.GetResourceStream(new Uri("/WanTag;component/Resources/loading.gif", UriKind.Relative)).Stream;
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(imageStream);
            this.imageExpender1.Image = bitmap;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            imageExpender1.Dispose();
        }
    }
}
