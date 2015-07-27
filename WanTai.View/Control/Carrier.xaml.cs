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
using WanTai.DataModel;

namespace WanTai.View.Control
{
    /// <summary>
    /// Interaction logic for Carrier.xaml
    /// </summary>
    public partial class Carrier : CarrierBase
    {
        public Carrier()
        {
            InitializeComponent();
        }

        public Carrier(Brush color, double width, double heigh)
        {
            InitializeComponent();
            GridPlate.Background = color;
            this.Width = width;
            this.Height = heigh;
        }
       
        public override void UpdatePlate(List<PlateBase> plates)
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is Plate)
                {
                    Plate plate = (Plate)UIE;
                    PlateBase tempPlate = plates.FirstOrDefault(P => P.Position == plate.Position);
                    if (tempPlate!=null)
                    {
                        plate.Color = tempPlate.Color;                        
                        plate.NeedVolume = tempPlate.NeedVolume;
                        plate.DisplayName = tempPlate.DisplayName;
                        plate.ItemType = tempPlate.ItemType;
                        plate.ChineseName = tempPlate.ChineseName;
                        plate.RelatedControls = tempPlate.RelatedControls;
                        plate.Correct = tempPlate.Correct;
                        plate.ConfigurationItemID = tempPlate.ConfigurationItemID;
                       
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
                if (UIE is Plate)
                {
                    Plate plate = (Plate)UIE;
                    plate.scan();
                }
            }
        }

        public override void ShiningStop()
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is Plate)
                {
                    Plate plate = (Plate)UIE;
                    plate.ShiningStop();
                }
            }
        }

        public override void ShiningStop(string PlateName)
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is Plate)
                {
                    if (((Plate)UIE).ChineseName==PlateName)
                        ((Plate)UIE).ShiningStop();
                }
            }
        }
        public override void ShiningStop(Guid ConfigurationItemID)
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is Plate)
                {
                    if (((Plate)UIE).ConfigurationItemID == ConfigurationItemID)
                        ((Plate)UIE).ShiningStop();
                }
            }
        }

        public override void ShiningWithGreen(string[] plates)
        {
            UIElementCollection children = GridPlate.Children;
            foreach (UIElement UIE in children)
            {
                if (UIE is Plate)
                {
                    foreach (string plateName in plates)
                    {
                        if (((Plate)UIE).DisplayName == plateName)
                          ((Plate)UIE).ShiningWithGreen();
                    }
                }
            }
        }
    }
}
