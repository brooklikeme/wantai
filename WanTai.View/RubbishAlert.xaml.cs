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
using System.Timers;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for RubbishAlert.xaml
    /// </summary>
    public partial class RubbishAlert : Window
    {
        DeskTopService desktopService = new DeskTopService();
        Timer timer = new Timer();
        public RubbishAlert()
        {
            InitializeComponent();
        }
        public RubbishAlert(Guid RotationID)
        {
            InitializeComponent();

            timer.Enabled = false;
            timer.Interval = 1000;
            timer.Elapsed += SwitchingAnimation;

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
            imgKingFisher.Width = lenghtUnit * 24;
            imgKingFisher.Height = lenghtUnit * 24;
            imgKingFisher.Margin = new Thickness(-lenghtUnit * 2 * 1.4, lenghtUnit * 12, 0, lenghtUnit * 2);

            if (SessionInfo.WorkDeskType == "200")
            {
                deskTop.Children.Add(desktopService.DrawCoordinateOld(width, limit, lenghtUnit + offset));
            }
            else
            {
                deskTop.Children.Add(desktopService.DrawCoordinate(width, limit, lenghtUnit + offset));
            }
            

            List<CarrierBase> carriers = desktopService.GetCarriers(900/84, 1.4);           
            List<ReagentAndSuppliesConfiguration> supplies = new ReagentSuppliesConfigurationController().
                GetRotationVolume(SessionInfo.ExperimentID, RotationID, new short[] { 0 }, ReagentAndSuppliesConfiguration.CurrentVolumeFieldName).FindAll(P => P.ItemType >= 100 && P.ItemType < 200);
            List<PlateBase> plates = new List<PlateBase>();
            if (supplies.Exists(P => P.ItemType == 101))
            {
                ReagentAndSuppliesConfiguration supply = supplies.First(P => P.ItemType == 101);
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
                plate.Correct = false;
                plates.Add(plate);
            }
            Brush PCRPlateColor = new SolidColorBrush(Colors.Red);
            foreach (ReagentAndSuppliesConfiguration supply in supplies)
            {
                Brush color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(supply.Color));
                if (supply.ItemType == DiTiType.DiTi1000)//DiTi1000
                {
                    //int position = 1;
                    //int grid = 1;
                    //string containerName = supply.ContainerName;
                    //int totalCount = int.Parse(decimal.Ceiling(decimal.Parse((supply.CurrentVolume / 96).ToString())).ToString());
                    //for (int i = 0; i < totalCount; i++)
                    //{
                    //    PlateBase plate = new WanTai.View.Control.Plate();
                    //    plate.DisplayName = supply.DisplayName + " " + (i + 1).ToString();
                    //    plate.ChineseName = supply.DisplayName;
                    //    plate.NeedVolume = 1;
                    //    plate.CurrentVolume = 1;
                    //    plate.Grid = grid;
                    //    plate.Position = position;
                    //    plate.ContainerName = "006";
                    //    plate.Color = color;
                    //    plate.ItemType = (short)supply.ItemType;
                    //    plate.ConfigurationItemID = supply.ItemID;
                    //    plate.Correct = false;
                    //    plates.Add(plate);

                    //    position++;
                    //    if (i == 5) break;
                    //}
                }
                else if (supply.ItemType == DiTiType.DiTi200)//DiTi200
                {
                    //int position = 1;
                    //int grid = 1;
                    //string containerName = supply.ContainerName;
                    //int totalCount = int.Parse(decimal.Ceiling(decimal.Parse((supply.CurrentVolume / 96).ToString())).ToString());
                    //for (int i = 0; i < totalCount; i++)
                    //{
                    //    PlateBase plate = new WanTai.View.Control.Plate();
                    //    plate.DisplayName = supply.DisplayName + " " + (i + 1).ToString();
                    //    plate.ChineseName = supply.DisplayName;
                    //    plate.NeedVolume = 1;
                    //    plate.CurrentVolume = 1;
                    //    plate.Grid = grid;
                    //    plate.Position = position;
                    //    plate.ContainerName = "007";
                    //    plate.Color = color;
                    //    plate.ItemType = (short)supply.ItemType;
                    //    plate.ConfigurationItemID = supply.ItemID;
                    //    plate.Correct = false;
                    //    plates.Add(plate);

                    //    position++;
                    //    if (i == 3) break;
                    //}
                }
                else if (supply.ItemType == 103 )//&& supply.NeedVolume>0)//PCR Plate
                {
                    PCRPlateColor = color;
             
                    int position = 2;
                    int grid = 1;
                    //string containerName = supply.ContainerName;
                    //for (int i = 0; i < supply.CurrentVolume; i++)
                    //{
                    PlateBase plate = new WanTai.View.Control.Plate();
                    plate.DisplayName = supply.DisplayName;
                    plate.ChineseName = supply.DisplayName;
                    plate.NeedVolume = 1;
                    plate.CurrentVolume = 1;
                    plate.Grid = grid;
                    plate.Position = position;
                    plate.ContainerName = "002";
                    plate.Color = color;
                    plate.ItemType = (short)supply.ItemType;
                    plate.ConfigurationItemID = supply.ItemID;
                    plate.Correct = false;
                    plates.Add(plate);

                    //position++;
                    //if (i == 2) break;
                    //}
                }
            }
            /*
          //DW96Plate3、DW96Plate4、DW96Plate5
            PlateBase PCRPlate = new WanTai.View.Control.Plate();
            PCRPlate.DisplayName = "DW96Plate3";
            PCRPlate.ChineseName = "DW96Plate3";
            PCRPlate.NeedVolume = 1;
            PCRPlate.CurrentVolume = 1;
            PCRPlate.Grid = 1;
            PCRPlate.Position = 1;
            PCRPlate.ContainerName = "002";
            PCRPlate.Color = PCRPlateColor;
            PCRPlate.ItemType = 100;
            PCRPlate.ConfigurationItemID = new Guid();
            PCRPlate.Correct = false;
            plates.Add(PCRPlate);
          
            PlateBase PCRPlate = new WanTai.View.Control.Plate();
            PCRPlate.DisplayName = "DW96Plate4";
            PCRPlate.ChineseName = "DW96Plate4";
            PCRPlate.NeedVolume = 1;
            PCRPlate.CurrentVolume = 1;
            PCRPlate.Grid = 1;
            PCRPlate.Position = 2;
            PCRPlate.ContainerName = "002";
            PCRPlate.Color = PCRPlateColor;
            PCRPlate.ItemType = 102;
            PCRPlate.ConfigurationItemID = new Guid();
            PCRPlate.Correct = false;
            plates.Add(PCRPlate);
      
            PCRPlate = new WanTai.View.Control.Plate();
            PCRPlate.DisplayName = "DW96Plate5";
            PCRPlate.ChineseName = "DW96Plate5";
            PCRPlate.NeedVolume = 1;
            PCRPlate.CurrentVolume = 1;
            PCRPlate.Grid = 1;
            PCRPlate.Position = 3;
            PCRPlate.ContainerName = "002";
            PCRPlate.Color = PCRPlateColor;
            PCRPlate.ItemType = 103;
            PCRPlate.ConfigurationItemID = new Guid(); 
            PCRPlate.Correct = false;
            plates.Add(PCRPlate);
            */

            foreach (CarrierBase c in carriers)
            {
                c.UpdatePlate(plates.FindAll(P => P.ContainerName == c.CarrierName));

                // 修改 枪头及深孔板 js
                if (SessionInfo.WorkDeskType == "100" && c.CarrierName == "006")
                    continue;

                deskTop.Children.Add(c);
                c.Scan();
            }
            this.RotationID = RotationID;
            timer.Start();

            string image = String.Format(@"/WanTag;component/Resources/Sample{0}.gif", SessionInfo.WorkDeskType);
            Sample.Source = new BitmapImage(new Uri(image, UriKind.Relative));
            // 隐藏 枪头 js
            if (SessionInfo.WorkDeskType == "100")
            {
                Sample.Margin = new Thickness(10, 0, 0, -15);
            }
            else if (SessionInfo.WorkDeskType == "150")
            {
                Sample.Margin = new Thickness(-15, 5, 0, -15);
                Sample.Width = 270;
            }
            else if (SessionInfo.WorkDeskType == "200")
            {
                Sample.Margin = new Thickness(-15, 5, 0, -15);
                Sample.Width = 420;
            }
        }
        private Guid RotationID;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            timer.Stop();
        }
        private bool flag = true;
        private void SwitchingAnimation(object sender, ElapsedEventArgs e)
        {
            if (flag)
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                        {
                            imgKingFisher.Source = new BitmapImage(new Uri(@"/WanTag;component/Resources/kingfisher_rubbish.gif", UriKind.Relative));
                        }));
                }
            }
            else
            {
                if (!this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(delegate()
                        {
                            imgKingFisher.Source = new BitmapImage(new Uri(@"/WanTag;component/Resources/kingfisher.gif", UriKind.Relative));
                        }));
                }
            }
            flag = !flag;
        }
    }
}
