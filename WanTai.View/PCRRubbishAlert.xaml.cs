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
using WanTai.DataModel;
using WanTai.Controller;
using WanTai.View.Services;
using WanTai.Controller.Configuration;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for RubbishAlert.xaml
    /// </summary>
    public partial class PCRRubbishAlert : Window
    {
        DeskTopService desktopService = new DeskTopService();
        public PCRRubbishAlert()
        {
            InitializeComponent();
        }
        public PCRRubbishAlert(Guid RotationID)
        {
            InitializeComponent();
            double lenghtUnit = deskTop.Width / 84;
            imgKingFisher.Width = lenghtUnit * 24;
            imgKingFisher.Height = lenghtUnit * 24;
            imgKingFisher.Margin = new Thickness(-lenghtUnit * 2 * 1.4, lenghtUnit * 1, 0, lenghtUnit * 2);
            deskTop.Children.Add(desktopService.DrawCoordinate(deskTop.Width, 69, lenghtUnit));

            List<CarrierBase> carriers = desktopService.GetCarriers(lenghtUnit, 1.4);           
            List<ReagentAndSuppliesConfiguration> supplies = new ReagentSuppliesConfigurationController().
                GetRotationVolume(SessionInfo.ExperimentID, RotationID, new short[] { 0 }, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName).FindAll(P => P.ItemType >= 100 && P.ItemType < 200);
            List<PlateBase> plates = new List<PlateBase>();
            //if (supplies.Exists(P => P.ItemType == 101))//dw 96 plate
            //{
            //    ReagentAndSuppliesConfiguration supply = supplies.First(P => P.ItemType == 101);
            //    PlateBase plate = new Control.Plate();
            //    plate.DisplayName = supply.DisplayName;
            //    plate.ChineseName = supply.DisplayName;
            //    plate.NeedVolume = 1;
            //    plate.CurrentVolume = 1;
            //    plate.Grid = 1;
            //    plate.Position = 1;
            //    plate.ContainerName = "001";
            //    plate.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(supply.Color));
            //    plate.ItemType = (short)supply.ItemType;
            //    plate.BarcodePrefix = supply.BarcodePrefix;
            //    plate.ConfigurationItemID = supply.ItemID;
            //    plate.Correct = false;
            //   // plates.Add(plate);
            //}
            //if (supplies.Exists(P => P.ItemType == 102))//96 Tip Comb
            //{
            //    ReagentAndSuppliesConfiguration supply = supplies.First(P => P.ItemType == 102);
            //    PlateBase plate = new Control.Plate();
            //    plate.DisplayName = supply.DisplayName;
            //    plate.ChineseName = supply.DisplayName;
            //    plate.NeedVolume = 1;
            //    plate.CurrentVolume = 1;
            //    plate.Grid = 1;
            //    plate.Position = 1;
            //    plate.ContainerName = "001";
            //    plate.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(supply.Color));
            //    plate.ItemType = (short)supply.ItemType;
            //    plate.BarcodePrefix = supply.BarcodePrefix;
            //    plate.ConfigurationItemID = supply.ItemID;
            //    plate.Correct = false;
            //    plates.Add(plate);
            //}
           
            Brush PCRPlateColor = new SolidColorBrush(Colors.Red);
            foreach (ReagentAndSuppliesConfiguration supply in supplies)
            {
                if (supply.ItemType == 101)//dw 96 plate
                {
                    for (int i = 1; i < 4; i++)
                    {
                        PlateBase plate = new Control.Plate();
                        plate.DisplayName = supply.DisplayName+ " " + (6 -i).ToString();;
                        plate.ChineseName = supply.DisplayName;
                        plate.NeedVolume = 1;
                        plate.CurrentVolume = 1;
                        plate.Grid = 1;
                        plate.Position = i;
                        plate.ContainerName = "002";
                        plate.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(supply.Color));
                        plate.ItemType = (short)supply.ItemType;
                        plate.BarcodePrefix = supply.BarcodePrefix;
                        plate.ConfigurationItemID = supply.ItemID;
                        plate.Correct = true;
                        plates.Add(plate);
                    }
                }
                else if (supply.ItemType == 102 && supply.CurrentVolume > 0)//96 Tip Comb
                {
                    PlateBase plate = new Control.Plate();
                    plate.DisplayName = supply.DisplayName;
                    plate.ChineseName = supply.DisplayName;
                    plate.NeedVolume = 1;
                    plate.CurrentVolume = 1;
                    plate.Grid = 1;
                    plate.Position = 1;
                    plate.ContainerName = "001";
                    plate.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(supply.Color));
                    plate.ItemType = (short)supply.ItemType;
                    plate.BarcodePrefix = supply.BarcodePrefix;
                    plate.ConfigurationItemID = supply.ItemID;
                    plate.Correct = true;
                    plates.Add(plate);
                }                
                else if (supply.ItemType == 103 && supply.CurrentVolume>0)//PCR Plate
                {
                    int position = 5;
                    int grid = 1;                    
                    PlateBase plate = new WanTai.View.Control.Plate();
                    plate.DisplayName = supply.DisplayName;
                    plate.ChineseName = supply.DisplayName;
                    plate.NeedVolume = 1;
                    plate.CurrentVolume = 1;
                    plate.Grid = grid;
                    plate.Position = position;
                    plate.ContainerName = "007";
                    plate.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(supply.Color));
                    plate.ItemType = (short)supply.ItemType;
                    plate.ConfigurationItemID = supply.ItemID;
                    plate.Correct = true;
                    plates.Add(plate);                    
                }
            }  
    
            foreach (CarrierBase c in carriers)
            {
                List<PlateBase> greenPlate = plates.FindAll(P => P.ContainerName == c.CarrierName);
                c.UpdatePlate(greenPlate);
                deskTop.Children.Add(c);
                string[] plateNames = new string[greenPlate.Count];
                int index = 0;
                foreach (PlateBase plate in greenPlate)
                {
                    plateNames[index] = plate.DisplayName;
                    index++;
                }
                c.ShiningWithGreen(plateNames);
                //c.Scan();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
