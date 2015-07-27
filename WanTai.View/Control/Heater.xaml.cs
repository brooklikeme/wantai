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
using System.Data;

namespace WanTai.View.Control
{
    /// <summary>
    /// Interaction logic for HeaterHeaterShelf.xaml
    /// </summary>
    public partial class Heater : CarrierBase
    {
        public Heater()
        {
            InitializeComponent();
        }

        public Heater(Brush color, double width, double heigh)
        {
            InitializeComponent();
            this.Background = color;
            this.Width = width;
            this.Height = heigh;
            GridPlate.Background = color;
            GridPlate.Width = width * 0.8;
            GridPlate.Height = heigh * 0.8;            
        }       

        public override void UpdatePlate(List<PlateBase> plates)
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is ReagentTub)
                {
                    ReagentTub tub = (ReagentTub)UIE;
                    PlateBase tempPlate = plates.FirstOrDefault(P => P.Position == tub.Position && P.Grid==tub.Grid);
                    if (tempPlate != null)
                    {
                        tub.Color = tempPlate.Color;
                        tub.NeedVolume = tempPlate.NeedVolume;
                        tub.FirstAddVolume = tempPlate.FirstAddVolume;
                        tub.DisplayName = tempPlate.DisplayName;
                        tub.ItemType = tempPlate.ItemType;
                        tub.ChineseName = tempPlate.ChineseName;
                        tub.RelatedControls = tempPlate.RelatedControls;
                        tub.Correct = tempPlate.Correct;
                        tub.ConfigurationItemID = tempPlate.ConfigurationItemID;
                    }
                }
            }
            Plates = plates;
        }

        public override void Scan()
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is ReagentTub)
                {
                    ReagentTub plate = (ReagentTub)UIE;
                    plate.scan();
                }
            }
        }

        public override void ShiningStop()
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is ReagentTub)
                {
                    ReagentTub plate = (ReagentTub)UIE;
                    plate.ShiningStop();
                }
            }
        }

        private void btnLarge_Click(object sender, RoutedEventArgs e)
        {
            LargeHeater largHeater = new LargeHeater(this);
            largHeater.ShowDialog();
        }

        public override void ShiningStop(string PlateName)
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is ReagentTub)
                {
                    if (((ReagentTub)UIE).ChineseName == PlateName)
                        ((ReagentTub)UIE).ShiningStop();
                }
            }
        }
        public override void ShiningStop(Guid ConfigurationItemID)
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is ReagentTub)
                {
                    if (((ReagentTub)UIE).ConfigurationItemID == ConfigurationItemID)
                        ((ReagentTub)UIE).ShiningStop();
                }
            }
        }

        public override void ShiningWithGreen(string[] plates)
        {
            
        }
    }
}
