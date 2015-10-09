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

            int width = 0, limit = 0; double offset = 0;
            if (SessionInfo.WorkDeskType == "100")
            {
                limit = 32;
                width = 500;
                offset = 4.1;
            }
            else if (SessionInfo.WorkDeskType == "150")
            {
                limit = 47;
                width = 700;
                offset = 1.7;
            }
            else if (SessionInfo.WorkDeskType == "200")
            {
                this.Width = 1200;
                this.Height = 500;
                limit = 69;
                width = 900;
                offset = -0.6;
            }

            deskTop.Width = width;
            double lenghtUnit = deskTop.Width / 84;
            imgKingFisher.Width = (900 / 84) * 24;
            imgKingFisher.Height = (900 / 84) * 24;
            imgKingFisher.Margin = new Thickness(-(500 / 84) * 2 * 1.4, (500 / 84) * 12, 0, (500 / 84) * 2);

            if (SessionInfo.WorkDeskType == "200")
            {
                deskTop.Children.Add(desktopService.DrawCoordinateOld(width, limit, lenghtUnit + offset));
            }
            else
            {
                deskTop.Children.Add(desktopService.DrawCoordinate(width, limit, lenghtUnit + offset));
            }

            List<CarrierBase> carriers = desktopService.GetCarriers(900 / 84, 1.4);           
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
                    if (SessionInfo.WorkDeskType == "100")
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            PlateBase plate = new Control.Plate();
                            plate.DisplayName = supply.DisplayName + " " + (5 - i).ToString(); ;
                            plate.ChineseName = supply.DisplayName;
                            plate.NeedVolume = 1;
                            plate.CurrentVolume = 1;
                            plate.Grid = 1;
                            plate.Position = (SessionInfo.WorkDeskType == "100" && i == 3) ? 2 : i + 1;
                            plate.ContainerName = (SessionInfo.WorkDeskType == "100" && i == 3) ? "001" : "002";
                            plate.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(supply.Color));
                            plate.ItemType = (short)supply.ItemType;
                            plate.BarcodePrefix = supply.BarcodePrefix;
                            plate.ConfigurationItemID = supply.ItemID;
                            plate.Correct = true;
                            plates.Add(plate);
                        }
                    }
                    else
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            PlateBase plate = new Control.Plate();
                            plate.DisplayName = supply.DisplayName + " " + (6 - i).ToString(); ;
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
                    int position = SessionInfo.WorkDeskType == "100" ? 1 : 5;
                    int grid = 1;                    
                    PlateBase plate = new WanTai.View.Control.Plate();
                    plate.DisplayName = supply.DisplayName;
                    plate.ChineseName = supply.DisplayName;
                    plate.NeedVolume = 1;
                    plate.CurrentVolume = 1;
                    plate.Grid = grid;
                    plate.Position = position;
                    plate.ContainerName = SessionInfo.WorkDeskType == "100" ? "002" : "007";
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
                // 修改 枪头及深孔板 js
                if (SessionInfo.WorkDeskType == "100" && c.CarrierName == "006")
                    continue;
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

            string image = String.Format(@"/WanTag;component/Resources/Sample{0}.gif", SessionInfo.WorkDeskType);
            Sample.Source = new BitmapImage(new Uri(image, UriKind.Relative));
            // 隐藏 枪头 js
            if (SessionInfo.WorkDeskType == "100")
            {
                Sample.Margin = new Thickness(10, 0, 0, -15);
                Sample.Width = 130;
            }
            else if (SessionInfo.WorkDeskType == "150")
            {
                Sample.Margin = new Thickness(-15, 5, 0, -15);
                Sample.Width = 320;
            }
            else if (SessionInfo.WorkDeskType == "200")
            {
                Sample.Margin = new Thickness(-15, 5, 0, -15);
                Sample.Width = 440;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
