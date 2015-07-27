using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using WanTai.Common;
using WanTai.DataModel;
namespace WanTai.Controller
{
    public class SampleTrackingController
    {
        public static void SampleTracking(Guid ExperimentsID, Guid OperationID, Guid RotationID, int OperationSequence,Guid NexRotationID,DateTime FileCreateTime,bool GroupOperation)
        {            
            using (WanTai.DataModel.WanTaiEntities _WanTaiEntities = new DataModel.WanTaiEntities())
            {
                string PCRBarCodesFile = WanTai.Common.Configuration.GetEvoOutputPath() + WanTai.Common.Configuration.GetPlatesBarCodesFile();
                if (File.Exists(PCRBarCodesFile))
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(PCRBarCodesFile);
                    DateTime fileModifiedTime = fileInfo.LastWriteTime;
                    if (DateTime.Compare(FileCreateTime, fileModifiedTime) < 0)
                    {                        
                            List<string> PCRPlateBarcodes = new List<string>();
                            Dictionary<string, string> MixPlateBarcodes = new Dictionary<string, string>();

                            using (FileStream fileStream = new System.IO.FileStream(PCRBarCodesFile, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                            {
                                using (System.IO.StreamReader mysr = new System.IO.StreamReader(fileStream))
                                {
                                    string strline;
                                    //1(Position);1;1(Grid);Tube 13*100mm 16 Pos;Tube1;013/035678;11
                                    while ((strline = mysr.ReadLine()) != null)
                                    {
                                        string BarCode = strline.Split(';')[strline.Split(';').Length - 1].Replace("\"", "");
                                        if (strline.IndexOf("PCR Plate;") > 0)
                                        {
                                            PCRPlateBarcodes.Add(BarCode);
                                        }
                                        else if (strline.IndexOf("DW 96 Plate") > 0)
                                        {
                                            if (strline.IndexOf("DW 96 Plate 1;") > 0)
                                            {
                                                MixPlateBarcodes.Add("DW 96 Plate 1", BarCode);
                                            }
                                            else if (strline.IndexOf("DW 96 Plate 2;") > 0)
                                            {
                                                MixPlateBarcodes.Add("DW 96 Plate 2", BarCode);
                                            }
                                        }
                                    }
                                }
                            }

                            if (PCRPlateBarcodes.Count() > 0)
                            {
                                List<Plate> Plates = _WanTaiEntities.Plates.Where(plate => (plate.RotationID == RotationID && plate.PlateType == (short)PlateType.PCR_Plate)).OrderBy(plate => plate.PlateID).ToList();
                                int index = 0;
                                foreach (Plate _plate in Plates)
                                {
                                    if (index < PCRPlateBarcodes.Count())
                                    {
                                        _plate.BarCode = PCRPlateBarcodes[index];
                                    }

                                    index++;
                                }
                            }

                            if (MixPlateBarcodes.Count() > 0)
                            {
                                foreach (string plateName in MixPlateBarcodes.Keys)
                                {
                                    if (GroupOperation && NexRotationID != new Guid())
                                    {
                                        Plate _plate = _WanTaiEntities.Plates.Where(plate => (plate.RotationID == NexRotationID && plate.PlateType == (short)PlateType.Mix_Plate) && plate.PlateName == plateName).FirstOrDefault();
                                        if (_plate != null)
                                            _plate.BarCode = MixPlateBarcodes[plateName];
                                    }
                                    else if(!GroupOperation)
                                    {
                                        Plate _plate = _WanTaiEntities.Plates.Where(plate => (plate.RotationID == RotationID && plate.PlateType == (short)PlateType.Mix_Plate) && plate.PlateName == plateName).FirstOrDefault();
                                        if (_plate != null)
                                            _plate.BarCode = MixPlateBarcodes[plateName];
                                    }
                                }
                            }

                            _WanTaiEntities.SaveChanges();
                        }                    
                }

                if (!Directory.Exists(WanTai.Common.Configuration.GetSampleTrackingOutPutPath())) return;
                string[] FileNames = Directory.GetFiles(WanTai.Common.Configuration.GetSampleTrackingOutPutPath(),"*.csv");
                if (FileNames.Length == 0) return;
                foreach (string FileName in FileNames)
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(FileName);
                    DateTime fileModifiedTime = fileInfo.LastWriteTime;
                    if (DateTime.Compare(FileCreateTime, fileModifiedTime) > 0) continue;
                    
                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(FileName);
                    Guid _RotationID = RotationID;
                    int _OperationSequence = OperationSequence;
                    if (NexRotationID != new Guid() && GroupOperation)
                    {
                        var Plates = _WanTaiEntities.Plates.Where(plate => plate.RotationID == NexRotationID && plate.PlateType == (short)PlateType.Mix_Plate && plate.BarCode == fileNameWithoutExtension);
                        if (Plates.FirstOrDefault() != null)
                        {
                            _RotationID = NexRotationID;
                            _OperationSequence = 1;
                        }
                    }

                    using (FileStream fileStream = new System.IO.FileStream(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                    {
                        using (System.IO.StreamReader mysr = new System.IO.StreamReader(fileStream))
                        {
                            using (CsvReader csv = new CsvReader(mysr, true))
                            {
                                // 格式化字段出错时，触发事件csv_ParseError  
                                csv.DefaultParseErrorAction = ParseErrorAction.RaiseEvent;
                                csv.ParseError += new EventHandler<ParseErrorEventArgs>(delegate(object sender, ParseErrorEventArgs e)
                                {
                                    // 如果错误是字段不存在，则跳转到下一行  
                                    if (e.Error is MissingFieldCsvException)
                                    {
                                        // Console.Write("--MISSING FIELD ERROR OCCURRED");
                                        e.Action = ParseErrorAction.AdvanceToNextLine;
                                    }
                                });


                                if (GroupOperation)
                                {
                                    ReagentAndSuppliesConfiguration ReagentAndSupplie = _WanTaiEntities.ReagentAndSuppliesConfigurations.Where(ReagentAndSupplies => ReagentAndSupplies.ItemType == 205).FirstOrDefault();
                                    if (ReagentAndSupplie != null)
                                    {
                                        if (fileInfo.Name.IndexOf(ReagentAndSupplie.BarcodePrefix) >= 0)
                                        {
                                            foreach (string[] result in csv)
                                            {
                                                if (string.IsNullOrEmpty(result[1]))
                                                    continue;
                                                float _temp = 0;
                                                if (string.IsNullOrEmpty(result[4]) || !float.TryParse(result[4], out _temp))
                                                    continue;
                                                if (string.IsNullOrEmpty(result[6]) || !float.TryParse(result[6], out _temp))
                                                    continue;
                                                _WanTaiEntities.AddToSampleTrackings(new SampleTracking()
                                                {
                                                    ItemID = WanTaiObjectService.NewSequentialGuid(),
                                                    RackID = result[0],
                                                    CavityID = result[1],
                                                    Position = result[2],
                                                    SampleID = result[3],
                                                    CONCENTRATION = float.Parse(result[4]),
                                                    VOLUME = string.IsNullOrEmpty(result[6]) ? 0 : float.Parse(result[6]),
                                                    SampleErrors = result[13],
                                                    ExperimentsID = ExperimentsID,
                                                    OperationID = OperationID,
                                                    OperationSequence = _OperationSequence,
                                                    RotationID = _RotationID,
                                                    FileName = fileInfo.Name,
                                                    CreateTime = DateTime.Now
                                                });
                                            }
                                            continue;
                                        }
                                    }
                                }

                                //var query = csv.Where(c => c[1] != c[3] && !string.IsNullOrEmpty(c[4]) && c[4] != "0");
                                var query = csv.Where(c => !(c[1] == c[3] && !string.IsNullOrEmpty(c[4]) && c[4] == "1"));
                                if (query == null) continue;
                                foreach (string[] result in query)
                                {
                                    if (string.IsNullOrEmpty(result[1]))
                                        continue;
                                    float _temp = 0;
                                    if (string.IsNullOrEmpty(result[4]) || !float.TryParse(result[4], out _temp))
                                        continue;
                                    if (string.IsNullOrEmpty(result[6]) || !float.TryParse(result[6], out _temp))
                                        continue;
                                    _WanTaiEntities.AddToSampleTrackings(new SampleTracking()
                                    {
                                        ItemID = WanTaiObjectService.NewSequentialGuid(),
                                        RackID = result[0],
                                        CavityID = result[1],
                                        Position = result[2],
                                        SampleID = result[3],
                                        CONCENTRATION = float.Parse(result[4]),
                                        VOLUME = string.IsNullOrEmpty(result[6]) ? 0 : float.Parse(result[6]),
                                        SampleErrors = result[13],
                                        ExperimentsID = ExperimentsID,
                                        OperationID = OperationID,
                                        OperationSequence = _OperationSequence,
                                        RotationID = _RotationID,
                                        FileName = fileInfo.Name,
                                        CreateTime = DateTime.Now
                                    });
                                }

                                /*****************************************************************************************************************************
                                StringBuilder sBuilder2 = new StringBuilder();
                                while (csv.ReadNextRecord())
                                {
                                    if (csv[1] != csv[3] && float.Parse(csv[4]) != 0)
                                        sBuilder2.Append(csv[7] + ";");
                                    //for (int i = 0; i < fieldCount; i++)
                                    //    Console.Write(string.Format("{0} = {1};", headers[i], csv[i]));
                                    //Console.WriteLine();
                                }
                                int fieldCount = csv.FieldCount;
                                string[] headers = csv.GetFieldHeaders();
                                var query = csv.Where(c => c[1] != c[3] && float.Parse(c[4])!=0);
                                StringBuilder sBuilder = new StringBuilder();
                                StringBuilder sBuilder1 = new StringBuilder();
                                foreach (string[] filed in query)
                                {
                                    sBuilder.Append(filed[0] + ";" + filed[1] + ";" + filed[3] + ";" + filed[4] + ";" + float.Parse(filed[4]).ToString());
                                    sBuilder1.Append(filed[7]+";");
                                }
                  
                                //var m = from n in csv where n[] < 5 orderby n select n; 
                              ************************************************************************************************************************************************/


                            }
                        }
                    }
                    
                }
                
                _WanTaiEntities.SaveChanges();
            }
        }
    }
}