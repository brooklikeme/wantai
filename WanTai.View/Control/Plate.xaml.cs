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
using System.Timers;
using System.Data;


namespace WanTai.View.Control
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Plate : PlateBase
    {

        #region Property

        public override Brush Color
        {
            get { return GridPlate.Background; }
            set { GridPlate.Background = value; }
        }
       


        public override string DisplayName
        {
            get
            {
                return txtPlateName.Text;
            }
            set
            {
                txtPlateName.Text = value;
                this.ToolTip = value;
                //if (ItemType == 104 || ItemType == 105)
                //{
                //    HammerheadImage.Source = new BitmapImage(new Uri("/WanTai.View;component/Resources/hammerhead.gif", UriKind.Relative));
                //    txtPlateName.Text += value.ToString();
                //}
                //if (ItemType == 104)
                //    HammerheadImage.Height = 35;
                //if (ItemType == 105)
                //    HammerheadImage.Height = 25;
            }
        }
        private short _ItemType;
        public override  short ItemType 
        {
            get
            {
                return _ItemType;
            }
            set
            {
                _ItemType = value;
                //if (value != 104 && value != 105) 
                //   HammerheadImage.Visibility = System.Windows.Visibility.Hidden;
                if ((value == 104 || value == 105) && !string.IsNullOrEmpty(DisplayName))
                {
                    HammerheadImage.Source = new BitmapImage(new Uri("/WanTag;component/Resources/hammerhead.png", UriKind.Relative));
                    txtPlateName.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    txtPlateName.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                }
                else
                {
                    txtPlateName.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    txtPlateName.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                }
                //Image img = new Image();
                //img.Source = new BitmapImage(new Uri("/WanTai.View;component/Resources/hammerhead.gif", UriKind.Relative));

                if (value == 104 && !string.IsNullOrEmpty(DisplayName))
                    HammerheadImage.Height = 30;
                else if (value == 105 && !string.IsNullOrEmpty(DisplayName))
                    HammerheadImage.Height = 25;
                else HammerheadImage.Height = 0;
            }
        }
        #endregion

        #region Constructure

        public Plate()
        {
            InitializeComponent();
        }

        #endregion

        public override void Check(int code)
        {
            if (code == 1)
            {
                this.BorderBrush = Brushes.Red;
            }
            if (code == 2)
            {
                this.BorderBrush = Brushes.Red;
            }
            if (code == 3)
            {

            }
        }
    }
}
