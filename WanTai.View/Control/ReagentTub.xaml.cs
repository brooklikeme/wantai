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

namespace WanTai.View.Control
{
    /// <summary>
    /// Interaction logic for HeaterPlate.xaml
    /// </summary>
    public partial class ReagentTub : PlateBase
    {
        #region Property
      
        public override Brush Color
        {
            get { return EllipseTub.Fill; }
            set { EllipseTub.Fill = value; }
        }
        public override string DisplayName
        {
            get
            {
                return this.ToolTip.ToString();
            }
            set
            {
                txtPlateName.Text = value;
                this.ToolTip = value;
            }
        }
        private short _ItemType;
        public override short ItemType
        {
            get
            {
                return _ItemType;
            }
            set
            {
                _ItemType = value;
            }
        }
        #endregion

        #region Constructure

        public ReagentTub()
        {
            InitializeComponent();
        }

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
        #endregion

    }
}
