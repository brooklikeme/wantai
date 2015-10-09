using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.View.Control;
using WanTai.DataModel;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using WanTai.Controller.EVO;
using WanTai.Controller;

namespace WanTai.View.Services
{
    public class DeskTopService
    {
        public Canvas DrawCoordinateOld(double length, int num, double unit)
        {
            Canvas CoordinateVanvas = new Canvas();
            Line xLine = new Line();
            xLine.Stroke = Brushes.Black;
            xLine.StrokeThickness = 1;
            xLine.X1 = 0;
            xLine.Y1 = 2;
            xLine.X2 = length;
            xLine.Y2 = 2;
            CoordinateVanvas.Children.Add(xLine);

            for (int i = 9; i <= num; i++)
            {
                Line pointLine = new Line();
                pointLine.Stroke = Brushes.Black;
                pointLine.StrokeThickness = 1;
                pointLine.X1 = length / (num - 9) * (i - 9);
                pointLine.Y1 = 5;
                pointLine.X2 = length / (num - 9) * (i - 9);
                pointLine.Y2 = i % 5 == 0 ? 10 : 7;
                CoordinateVanvas.Children.Add(pointLine);
                if (i % 5 == 0)
                {
                    TextBlock text = new TextBlock();
                    text.Text = i.ToString();
                    text.Margin = new Thickness(length / (num - 9) * (i - 9) - 5, 10, 0, 0);
                    CoordinateVanvas.Children.Add(text);
                }
            }
            CoordinateVanvas.Margin = new Thickness(0, unit * 30 - 30, 0, 0);
            return CoordinateVanvas;
        }

        public Canvas DrawCoordinate(double length, int num, double unit)
        {
            int len = 15;
            //int unit = 900 / 84;
            Canvas CoordinateVanvas = new Canvas();
            // 画刻度长度
            Line xLine = new Line();
            xLine.Stroke = Brushes.Black;
            xLine.StrokeThickness = 1;
            xLine.X1 = 11;
            xLine.Y1 = 5;
            xLine.X2 = length;
            xLine.Y2 = 5;
            CoordinateVanvas.Children.Add(xLine);

            // 画刻度
            for (int i = 1; i <= num; i++)
            {
                Line pointLine = new Line();
                pointLine.Stroke = Brushes.Black;
                pointLine.StrokeThickness = 1;
                pointLine.X1 = len * i;
                pointLine.Y1 = 5;
                pointLine.X2 = len * i;
                pointLine.Y2 = i % 5 == 0 ? 11 : 8;
                CoordinateVanvas.Children.Add(pointLine);

                // 刻度数字
                if (i % 5 == 0 || i == 1)
                {
                    TextBlock text = new TextBlock();
                    text.Text = i.ToString();
                    text.Margin = new Thickness(len * i - 5, 10, 0, 0);
                    CoordinateVanvas.Children.Add(text);
                }
            }
            CoordinateVanvas.Margin = new Thickness(length - (len * num), unit * 30 - 30, 0, 0);
            return CoordinateVanvas;
        }

        public List<CarrierBase> GetCarriers(double lengthUnit, double cooPoint)
        {
            List<CarrierBase> carrierBases = new List<CarrierBase>();
            List<DataModel.Carrier> carriers = new CarrierController().GetCarrier();
            foreach (DataModel.Carrier c in carriers)
            {
                CarrierBase uc;
                Brush brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(c.Color));
                if (c.Type == 0)
                {
                    // 修改 深孔板边距 js   @Carrier
                    uc = new View.Control.Carrier(
                       brush, c.Width * lengthUnit, c.Heigh * lengthUnit, (float)c.Width);
                }
                else if (c.Type == 1)
                {
                    uc = new Shelf(
                       brush, c.Width * lengthUnit, c.Heigh * lengthUnit);
                }
                else //(c.Type==2)
                {
                    uc = new Heater(
                    brush, c.Width * lengthUnit, c.Heigh * lengthUnit);
                }
                // 计算 位置 js 
                uc.Margin = new Thickness((c.Grid - 9) * lengthUnit * cooPoint, c.Position * lengthUnit, 0, 0);
                uc.CarrierName = c.CarrierName;
                carrierBases.Add(uc);
            }
            return carrierBases;
        }
        /// <summary>
        /// Create plates and put plate on carrier
        /// </summary>
        /// <param name="reagentAndSupplies"></param>
        /// <param name="carriers"></param>
        /// <param name="flag">0 Need,1 Current</param>
        /// <returns></returns>
        public List<PlateBase> SetReagentPosition(List<ReagentAndSuppliesConfiguration> reagentAndSupplies, List<DataModel.Carrier> carriers, short flag)
        {
            List<PlateBase> plates = new List<PlateBase>();

            double volume = 0;
            double actualSavedVolumn = 0;
            double currentActualVolumn = 0;
            foreach (ReagentAndSuppliesConfiguration reagent in reagentAndSupplies)
            {
                if (flag == 0)
                {
                    volume = reagent.NeedVolume;
                    if (reagent.ActualSavedVolume > 0)
                        actualSavedVolumn = reagent.ActualSavedVolume;
                }
                else
                {
                    volume = reagent.CurrentVolume;
                    if (reagent.CurrentActualVolume > 0)
                        currentActualVolumn = reagent.CurrentActualVolume;
                }
                if (volume <= 0) continue;
                Brush color = new SolidColorBrush((Color)ColorConverter.ConvertFromString(reagent.Color));
                if (reagent.ItemType > 0 && reagent.ItemType < 100 && reagent.ItemType % 5 == 0)
                {
                    PlateBase plate = new WanTai.View.Control.ReagentTub();
                    plate.DisplayName = reagent.DisplayName;
                    plate.ChineseName = reagent.DisplayName;
                    plate.EnglishName = reagent.EnglishName;
                    plate.NeedVolume = reagent.NeedVolume;
                    plate.CurrentVolume = reagent.CurrentVolume;
                    plate.Grid = reagent.Grid;
                    plate.Position = reagent.Position;
                    plate.ContainerName = reagent.ContainerName;
                    plate.CarrierGrid = (int)carriers.First(P => P.CarrierName == reagent.ContainerName).Grid;
                    plate.Color = color;
                    plate.ItemType = (short)reagent.ItemType;
                    plate.BarcodePrefix = reagent.BarcodePrefix;
                    plate.Barcode = reagent.BarcodePrefix;
                    plate.ConfigurationItemID = reagent.ItemID;
                    plate.Correct = reagent.Correct;
                    plate.ConfigurationItemID = reagent.ItemID;
                    plates.Add(plate);
                }
                else if (reagent.ItemType == 101)//DW 96 Plate
                {
                    int position = reagent.Position;
                    int grid = reagent.Grid;
                    string containerName = reagent.ContainerName;
                    for (int i = 0; i < volume; i++)
                    {
                        PlateBase plate = new WanTai.View.Control.Plate();
                        plate.DisplayName = reagent.DisplayName + " " + (i + 1).ToString();
                        plate.ChineseName = reagent.DisplayName;
                        plate.EnglishName = reagent.EnglishName;
                        plate.NeedVolume = i == 0 ? volume : 0;
                        plate.CurrentVolume = reagent.CurrentVolume;
                        plate.Grid = grid;
                        plate.Position = position;
                        plate.ContainerName = containerName;
                        plate.CarrierGrid = (int)carriers.First(P => P.CarrierName == containerName).Grid;
                        plate.Color = color;
                        plate.ItemType = (short)reagent.ItemType;
                        plate.BarcodePrefix = reagent.BarcodePrefix;
                        plate.Barcode = reagent.BarcodePrefix;
                        plate.ConfigurationItemID = reagent.ItemID;
                        plate.Correct = reagent.Correct;
                        plates.Add(plate);

                        position--;
                        if (i == 1)
                        {
                            containerName = "002";
                            position = 3;
                        }
                        if (i == 4) break;
                    }
                    //if (volume == 4)
                    //{
                    //    PlateBase plate = new WanTai.View.Control.Plate();
                    //    plate.DisplayName = "PCR 板";
                    //    plate.ChineseName = reagent.DisplayName;
                    //    plate.EnglishName = reagent.EnglishName;
                    //    plate.NeedVolume = 0;
                    //    plate.CurrentVolume = reagent.CurrentVolume;
                    //    plate.Grid = grid;
                    //    plate.Position = position;
                    //    plate.ContainerName = containerName;
                    //    plate.CarrierGrid = (int)carriers.First(P => P.CarrierName == containerName).Grid;
                    //    plate.Color = color;
                    //    plate.ItemType = (short)reagent.ItemType;
                    //    plate.BarcodePrefix = reagent.BarcodePrefix;
                    //    plate.Barcode = reagent.BarcodePrefix;
                    //    plate.ConfigurationItemID = reagent.ItemID;
                    //    plate.Correct = reagent.Correct;
                    //    plates.Add(plate);
                    //}
                }
                else if (reagent.ItemType == 103)//PCR Plate
                {
                    int position = reagent.Position;
                    int grid = reagent.Grid;
                    string containerName = reagent.ContainerName;
                    int offset = 0;

                    if (SessionInfo.WorkDeskType == "100"){
                        // offset = 1;
                    }

                    for (int i = 0; i < volume; i++)
                    {
                        PlateBase plate = new WanTai.View.Control.Plate();
                        plate.DisplayName = reagent.DisplayName + " " + (i + 1 + offset).ToString();
                        plate.ChineseName = reagent.DisplayName;
                        plate.EnglishName = reagent.EnglishName;
                        plate.NeedVolume = i == 0 ? volume : 0;
                        plate.CurrentVolume = reagent.CurrentVolume;
                        plate.Grid = grid;
                        plate.Position = position;
                        plate.ContainerName = containerName;
                        plate.CarrierGrid = (int)carriers.First(P => P.CarrierName == containerName).Grid;
                        plate.Color = color;
                        plate.ItemType = (short)reagent.ItemType;
                        plate.BarcodePrefix = reagent.BarcodePrefix;
                        plate.Barcode = reagent.BarcodePrefix;
                        plate.ConfigurationItemID = reagent.ItemID;
                        plate.Correct = reagent.Correct;
                        plates.Add(plate);

                        position++;
                        if (i == 0) break;

                    }
                }
                else if (reagent.ItemType == 104)//1000 Diti
                {
                    int position = reagent.Position;
                    int grid = reagent.Grid;
                    string containerName = reagent.ContainerName;
                    for (int i = 0; i < volume; i++)
                    {
                        PlateBase plate = new WanTai.View.Control.Plate();
                        plate.DisplayName = reagent.DisplayName + " " + (i + 1).ToString();
                        plate.ChineseName = reagent.DisplayName;
                        plate.EnglishName = reagent.EnglishName;
                        plate.NeedVolume = i == 0 ? volume : 0;
                        if (actualSavedVolumn > 0)
                        {
                            plate.ActualSavedVolume = i == 0 ? actualSavedVolumn : 0;
                        }

                        if (currentActualVolumn > 0)
                        {
                            plate.CurrentActualVolume = i == 0 ? currentActualVolumn : 0;
                        }

                        plate.Grid = grid;
                        plate.Position = position;
                        plate.ContainerName = containerName;
                        plate.CarrierGrid = (int)carriers.First(P => P.CarrierName == containerName).Grid;
                        plate.Color = color;
                        plate.ItemType = (short)reagent.ItemType;
                        plate.BarcodePrefix = reagent.BarcodePrefix;
                        plate.Barcode = reagent.BarcodePrefix;
                        plate.ConfigurationItemID = reagent.ItemID;
                        plate.Correct = reagent.Correct;
                        plates.Add(plate);

                        position++;
                        if (i == 1)
                        {
                            containerName = "006";
                            position = 2;
                        }
                        if (i == 5) break;
                    }
                }
                else if (reagent.ItemType == 105)//200 Diti
                {
                    int position = reagent.Position;
                    int grid = reagent.Grid;
                    string containerName = reagent.ContainerName;
                    for (int i = 0; i < volume; i++)
                    {
                        PlateBase plate = new WanTai.View.Control.Plate();
                        plate.DisplayName = reagent.DisplayName + " " + (i + 1).ToString();
                        plate.ChineseName = reagent.DisplayName;
                        plate.EnglishName = reagent.EnglishName;
                        plate.NeedVolume = i == 0 ? volume : 0;
                        if (actualSavedVolumn > 0)
                        {
                            plate.ActualSavedVolume = i == 0 ? actualSavedVolumn : 0;
                        }

                        if (currentActualVolumn > 0)
                        {
                            plate.CurrentActualVolume = i == 0 ? currentActualVolumn : 0;
                        }

                        plate.Grid = grid;
                        plate.Position = position;
                        plate.ContainerName = containerName;
                        plate.CarrierGrid = (int)carriers.First(P => P.CarrierName == containerName).Grid;
                        plate.Color = color;
                        plate.ItemType = (short)reagent.ItemType;
                        plate.BarcodePrefix = reagent.BarcodePrefix;
                        plate.Barcode = reagent.BarcodePrefix;
                        plate.ConfigurationItemID = reagent.ItemID;
                        plate.Correct = reagent.Correct;
                        plates.Add(plate);

                        position++;
                        if (i == 0)
                        {
                            containerName = "007";
                            position = 2;
                        }
                        if (i == 3) break;
                    }
                }
                else if (reagent.ItemType < 200)
                {
                    PlateBase plate = new WanTai.View.Control.Plate();
                    plate.DisplayName = reagent.DisplayName;
                    plate.ChineseName = reagent.DisplayName;
                    plate.EnglishName = reagent.EnglishName;
                    plate.NeedVolume = reagent.NeedVolume;
                    plate.CurrentVolume = reagent.CurrentVolume;
                    plate.Grid = reagent.Grid;
                    plate.Position = reagent.Position;
                    plate.ContainerName = reagent.ContainerName;
                    plate.CarrierGrid = (int)carriers.First(P => P.CarrierName == reagent.ContainerName).Grid;
                    plate.Color = color;
                    plate.ItemType = (short)reagent.ItemType;
                    plate.BarcodePrefix = reagent.BarcodePrefix;
                    plate.Barcode = reagent.BarcodePrefix;
                    plate.ConfigurationItemID = reagent.ItemID;
                    plate.Correct = reagent.Correct;
                    plates.Add(plate);
                }
            }
            return plates;
        }

        public void CallScanScript()
        {
            string scanLabwaresScriptName = WanTai.Common.Configuration.GetLabwaresScanScriptName();

            try
            {
                IProcessor processor = ProcessorFactory.GetProcessor();
                processor.StartScript(scanLabwaresScriptName);
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}
