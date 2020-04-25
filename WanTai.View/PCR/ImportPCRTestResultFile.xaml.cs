using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WanTai.Controller;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Data.OleDb;
using System.Data;

using NPOI.HSSF.Model; // InternalWorkbook
using NPOI.HSSF.UserModel; // HSSFWorkbook, HSSFSheet

using WanTai.DataModel;
using WanTai.Controller.PCR;
// using Excel = Microsoft.Office.Interop.Excel; 

namespace WanTai.View.PCR
{
    /// <summary>
    /// Interaction logic for ImportPCRTestResultFile.xaml
    /// </summary>
    public partial class ImportPCRTestResultFile : Window
    {
        public static string[] PCRFileTypeName = { "ABI 7500", "BIO-RAD CFX96", "Stratagene Mx3000P" };
        ImportPCRTestResultFileController controller = new ImportPCRTestResultFileController();
        List<PCRTestResult> pcrResultList = new List<PCRTestResult>();
        string PCRFilePath = Common.Configuration.GetPCRFilePath();
        Guid currentExperimentId;
        Boolean hasEmptyResults = true;

        public ImportPCRTestResultFile()
        {           
            InitializeComponent();
            currentExperimentId = SessionInfo.ExperimentID;
        }

        public ImportPCRTestResultFile(Guid _importedexperimentID)
        {
            InitializeComponent();
            currentExperimentId = _importedexperimentID;
        }  

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string name in PCRFileTypeName)
            {
                this.type_comboBox.Items.Add(name);
            }

            List<RotationInfo> rotationList = controller.GetFinishedRotation(currentExperimentId);
            foreach (RotationInfo info in rotationList)
            {
                rotation_comboBox.Items.Add(new ComboBoxItem() { Content = info.RotationName, Tag = info.RotationID });                
            }
            // init selection
            if (!set_next_empty_item())
            {
                MessageBox.Show("所有数据均已导入", "系统提示");
                hasEmptyResults = false;
            }
        }        

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
            {
                return;
            }

            ComboBoxItem selectedItem = (ComboBoxItem)rotation_comboBox.SelectedItem;
            Guid rotationId = (Guid)selectedItem.Tag;

            ComboBoxItem barcodeSelectedItem = (ComboBoxItem)barcode_comboBox.SelectedItem;
            Plate plate = (Plate)barcodeSelectedItem.DataContext;
            Guid plateId = plate.PlateID;
            bool hasRecord = controller.IsPlateHasImportedResult(rotationId, plateId, currentExperimentId);
            if (hasRecord)
            {
                MessageBox.Show("此条码的PCR板已经导入记录，不能重复导入，请删除旧的再次导入。", "系统提示");
                return;
            }

            string fileName = file_textBox.Text;
            bool isRightFormat = false;
            bool isRightPositionNumber = true;
            string PCRStartTime = "";
            string PCREndTime = "";
            string PCRDevice = type_comboBox.SelectedItem.ToString();

            if (type_comboBox.SelectedIndex == 0)
            {                
                if (SessionInfo.WorkDeskType != "100")
                {
                    isRightFormat = preProcessABIFile(fileName, out PCRStartTime, out PCREndTime) && processABIFile(fileName, rotationId, plate, out isRightPositionNumber);
                } else 
                {
                    isRightFormat = preProcessABIFile(fileName, out PCRStartTime, out PCREndTime) && processABIFile100(fileName, rotationId, plate, out isRightPositionNumber); 
                }               
            }
            else if (type_comboBox.SelectedIndex == 1)
            {
                if (SessionInfo.WorkDeskType != "100")
                {
                    isRightFormat = processBIORADFile(fileName, rotationId, plate, out isRightPositionNumber, out PCRStartTime, out PCREndTime);
                }
                else
                {
                    isRightFormat = processBIORADFile100(fileName, rotationId, plate, out isRightPositionNumber, out PCRStartTime, out PCREndTime);
                }
            }
            else if (type_comboBox.SelectedIndex == 2)
            {
                if (SessionInfo.WorkDeskType != "100")
                {
                    isRightFormat = processStratageneFile(fileName, rotationId, plate, out isRightPositionNumber);
                }
                else
                {
                    isRightFormat = processStratageneFile100(fileName, rotationId, plate, out isRightPositionNumber);
                }
                
            }

            if (!isRightFormat)
            {
                MessageBox.Show("文件格式不对，请选择正确的文件", "系统提示");
                return;
            }

            if (!isRightPositionNumber)
            {
                return;
            }

            bool result = controller.Create(pcrResultList) && controller.SetPCRPlateExtContent(plateId, currentExperimentId,  PCRStartTime, PCREndTime, PCRDevice);
            WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "导入PCR检测结果" + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
            if (result)
            {
                // clear file name
                file_textBox.Text = "";

                if (set_next_empty_item()) {
                    pcrResultList = new List<PCRTestResult>();
                    MessageBox.Show("导入成功, 请导入下一批数据", "系统提示");
                } else {
                    MessageBox.Show("导入成功, 所有数据均已导入", "系统提示");
                }                
            }
            else
            {
                MessageBox.Show("导入失败", "系统提示");
            }
        }

        private bool preProcessABIFile(string fileName, out string PCRStartTime, out string PCREndTime)
        {
            HSSFWorkbook workbook;
            HSSFSheet sheet = null;
            PCRStartTime = "";
            PCREndTime = "";

            // 复制新文件

            // get sheets list from xls
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                workbook = new HSSFWorkbook(fs);

                if (workbook.Count == 1)
                    sheet = (HSSFSheet)workbook.GetSheetAt(0);
                else{
                    for (int i = 0; i < workbook.Count; i++)
                    {
                        if (workbook.GetSheetAt(i).SheetName == "Results")
                        {
                            sheet = (HSSFSheet)workbook.GetSheetAt(i);
                            break;
                        }
                    }
                }

                if (null == sheet)
                    return false;

                // 
                int index = 0;
                int tableRowIndex = -1;
                while (tableRowIndex == -1 && index < 20)
                {
                    if (null != sheet.GetRow(index))
                    {
                        // add necessary columns
                        for (int j = sheet.GetRow(index).FirstCellNum; j < sheet.GetRow(index).LastCellNum; j++)
                        {
                            string cellValue = sheet.GetRow(index).GetCell(j) != null ? sheet.GetRow(index).GetCell(j).StringCellValue : "";
                            if ("Experiment Run End Time" == cellValue)
                            {
                                PCREndTime = (sheet.GetRow(index).LastCellNum > j + 1 && null != sheet.GetRow(index).GetCell(j + 1)) ? sheet.GetRow(index).GetCell(j + 1).StringCellValue : "";
                            }
                            if ("Experiment Run Start Time" == cellValue)
                            {
                                PCRStartTime = (sheet.GetRow(index).LastCellNum > j + 1 && null != sheet.GetRow(index).GetCell(j + 1)) ? sheet.GetRow(index).GetCell(j + 1).StringCellValue : "";
                            }
                            if ("Well" == cellValue)
                            {
                                tableRowIndex = index;
                                break;
                            }
                        }
                    }
                    index++;
                }
                if (tableRowIndex == -1)
                    return false;
                else
                {
                    // delete unused rows
                    int rowNum = sheet.LastRowNum;
                    sheet.ShiftRows(tableRowIndex, rowNum, 0 - tableRowIndex);
                    // delete unused sheets
                    for (int i = workbook.Count - 1; i >= 0; i --)
                    {
                        if (workbook.GetSheetAt(i).SheetName != sheet.SheetName)
                        {
                            workbook.RemoveSheetAt(i);
                        }
                    }
                    using (var preFile = new FileStream(fileName + ".pre", FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(preFile);
                    }
                }
            }
            return true;
        }

        private bool processABIFile(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
            connectionStringBuilder.DataSource = fileName + ".pre";
            connectionStringBuilder.Add("Mode", "Read");
            connectionStringBuilder.Add("Extended Properties", "Excel 8.0;IMEX=1;TypeGuessRows=0;ImportMixedTypes=Text");
            bool isRightFormat = false;
            bool hasWellColumn = false;
            bool hasDetectorColumn = false;
            bool hasCtColumn = false;
            bool hasNameColumn = false;
            using (OleDbConnection con = new OleDbConnection(connectionStringBuilder.ToString()))
            {
                con.Open();
                DataTable dtSchema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string Sheet1 = dtSchema.Rows[0].Field<string>("TABLE_NAME");

                DataTable sheetColumns = con.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, Sheet1, null });
                foreach (DataRow row in sheetColumns.Rows)
                {
                    string strColumnName = row["COLUMN_NAME"].ToString();
                    if (strColumnName == "Well")
                    {
                        hasWellColumn = true;
                    }
                    else if (strColumnName == "Reporter")
                    {
                        hasDetectorColumn = true;
                    }
                    if (strColumnName == "Cт")
                    {
                        hasCtColumn = true;
                    }
                    if (strColumnName == "Sample Name")
                    {
                        hasNameColumn = true;
                    }
                }

                if (hasWellColumn && hasDetectorColumn && hasCtColumn && hasNameColumn)
                {
                    isRightFormat = true;
                }

                if (!isRightFormat)
                    return isRightFormat;

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Reporter],[Cт],[Sample Name] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

                foreach (DataRow row in dt.Rows)
                {
                    if (null != row[0].ToString() && "" != row[0].ToString()
                        && null != row[1].ToString() && "" != row[1].ToString()
                        && null != row[2].ToString() && "" != row[2].ToString())
                    {
                        PCRColumnData sData = new PCRColumnData() { Well = row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString(), SampleName = row[3].ToString() };
                        sData.XmlContent = "<Row><Well>" + sData.Well + "</Well><Detector>" + sData.Detector + "</Detector><Ct>" + sData.Ct + "</Ct><SampleName>" + sData.SampleName + "</SampleName></Row>";
                        if (!dataList.ContainsKey(sData.Well))
                        {
                            dataList.Add(sData.Well, sData);
                        }
                        else
                        {
                            PCRColumnData _sData = (PCRColumnData)dataList[sData.Well];
                            if (_sData.DataList == null)
                            {
                                _sData.DataList = new List<PCRColumnData>();
                                _sData.DataList.Add(sData);
                            }
                            else
                            {
                                _sData.DataList.Add(sData);
                            }
                        }
                    }
                }

                if (dc.Count != dataList.Count)
                {
                    isRightPositionNumber = false;
                    if (!isRightPositionNumber)
                    {
                        MessageBoxResult selectResult = MessageBox.Show("文件记录数[" + dataList.Count.ToString() + "]和检测数[" + dc.Count.ToString() 
                            + "]不一致，是否继续导入？“是”继续导入", "系统提示", MessageBoxButton.YesNo);
                        if (selectResult == MessageBoxResult.No)
                        {
                            return isRightFormat;
                        }
                        else
                        {
                            isRightPositionNumber = true;
                        }
                    }
                }

                foreach (PCRColumnData sData in dataList.Values)
                {
                    PCRTestResult info = new PCRTestResult();
                    info.ItemID = WanTaiObjectService.NewSequentialGuid();
                    info.RotationID = rotationId;
                    info.Position = controller.ChangeCharacterToPositionNumber(sData.Well);
                    info.CreateTime = DateTime.Now;

                    //calculate 
                    if (dc.ContainsKey(info.Position))
                    {
                        DataRow dcrow = dc[info.Position];
                        bool isSingle = (int)dcrow["TubeNumber"] == 1 ? true : false;
                        Tubetype tubeType = (Tubetype)((short)dcrow["TubeType"]);
                        info.Result = checkPCRResult(sData, dcrow["TestName"].ToString(), isSingle, tubeType);

                        info.ExperimentID = currentExperimentId;
                        if (plate.BarCode != null)
                        {
                            info.BarCode = plate.BarCode;
                        }

                        info.PlateID = plate.PlateID;
                        string xmlContent = sData.XmlContent;
                        if (sData.DataList != null && sData.DataList.Count > 0)
                        {
                            foreach (PCRColumnData data in sData.DataList)
                            {
                                xmlContent = xmlContent + data.XmlContent;
                            }
                        }
                        info.PCRContent = "<PCRContent>" + xmlContent + "</PCRContent>";
                        pcrResultList.Add(info);
                    }
                }
            }

            return isRightFormat;
        }

        private bool processABIFile100(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
            connectionStringBuilder.DataSource = fileName + ".pre";
            connectionStringBuilder.Add("Mode", "Read");
            connectionStringBuilder.Add("Extended Properties", "Excel 8.0;IMEX=1;TypeGuessRows=0;ImportMixedTypes=Text");
            bool isRightFormat = false;
            bool hasWellColumn = false;
            bool hasDetectorColumn = false;
            bool hasCtColumn = false;
            bool hasQuantityColumn = false;
            bool hasDrColumn = false;
            bool hasNameColumn = false;
            using (OleDbConnection con = new OleDbConnection(connectionStringBuilder.ToString()))
            {
                con.Open();
                DataTable dtSchema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string Sheet1 = dtSchema.Rows[0].Field<string>("TABLE_NAME");

                DataTable sheetColumns = con.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, Sheet1, null });
                foreach (DataRow row in sheetColumns.Rows)
                {
                    string strColumnName = row["COLUMN_NAME"].ToString();
                    if (strColumnName == "Well")
                    {
                        hasWellColumn = true;
                    }
                    else if (strColumnName == "Reporter")
                    {
                        hasDetectorColumn = true;
                    }
                    if (strColumnName == "Cт")
                    {
                        hasCtColumn = true;
                    }
                    if (strColumnName == "Quantity")
                    {
                        hasQuantityColumn = true;
                    }
                    if (strColumnName == "RSq (dR)")
                    {
                        hasDrColumn = true;
                    }
                    if (strColumnName == "Sample Name")
                    {
                        hasNameColumn = true;
                    }
                }

                if (hasWellColumn && hasDetectorColumn && hasCtColumn && hasQuantityColumn && hasNameColumn)
                {
                    isRightFormat = true;
                }

                if (!isRightFormat)
                    return isRightFormat;

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Reporter],[Cт],[Quantity],[Sample Name] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

                foreach (DataRow row in dt.Rows)
                {
                    if (null != row[0].ToString() && "" != row[0].ToString()
                        && null != row[1].ToString() && "" != row[1].ToString()
                        && null != row[2].ToString() && "" != row[2].ToString())
                    {
                        PCRColumnData sData = new PCRColumnData() { Well = row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString(), Quantity = row[3].ToString(), SampleName = row[4].ToString() };
                        sData.XmlContent = "<Row><Well>" + sData.Well + "</Well><Detector>" + sData.Detector + "</Detector><Ct>" + sData.Ct + "</Ct><Quantity>" + sData.Quantity + "</Quantity><SampleName>" + sData.SampleName + "</SampleName></Row>";
                        if (!dataList.ContainsKey(sData.Well))
                        {
                            dataList.Add(sData.Well, sData);
                        }
                        else
                        {
                            PCRColumnData _sData = (PCRColumnData)dataList[sData.Well];
                            if (_sData.DataList == null)
                            {
                                _sData.DataList = new List<PCRColumnData>();
                                _sData.DataList.Add(sData);
                            }
                            else
                            {
                                _sData.DataList.Add(sData);
                            }
                        }
                    }
                }

                if (dc.Count != dataList.Count)
                {
                    isRightPositionNumber = false;
                    if (!isRightPositionNumber)
                    {
                        MessageBoxResult selectResult = MessageBox.Show("文件记录数[" + dataList.Count.ToString() + "]和检测数[" + dc.Count.ToString()
                            + "]不一致，是否继续导入？“是”继续导入", "系统提示", MessageBoxButton.YesNo); 
                        if (selectResult == MessageBoxResult.No)
                        {
                            return isRightFormat;
                        }
                        else
                        {
                            isRightPositionNumber = true;
                        }
                    }
                }

                foreach (PCRColumnData sData in dataList.Values)
                {
                    PCRTestResult info = new PCRTestResult();
                    info.ItemID = WanTaiObjectService.NewSequentialGuid();
                    info.RotationID = rotationId;
                    info.Position = controller.ChangeCharacterToPositionNumber(sData.Well);
                    info.CreateTime = DateTime.Now;

                    //calculate 
                    if (dc.ContainsKey(info.Position))
                    {
                        DataRow dcrow = dc[info.Position];
                        bool isSingle = (int)dcrow["TubeNumber"] == 1 ? true : false;
                        Tubetype tubeType = (Tubetype)((short)dcrow["TubeType"]);
                        info.Result = checkPCRResult100(sData, dcrow["TestName"].ToString(), isSingle, tubeType);

                        info.ExperimentID = currentExperimentId;
                        if (plate.BarCode != null)
                        {
                            info.BarCode = plate.BarCode;
                        }

                        info.PlateID = plate.PlateID;
                        string xmlContent = sData.XmlContent;
                        if (sData.DataList != null && sData.DataList.Count > 0)
                        {
                            foreach (PCRColumnData data in sData.DataList)
                            {
                                xmlContent = xmlContent + data.XmlContent;
                            }
                        }
                        info.PCRContent = "<PCRContent>" + xmlContent + "</PCRContent>";
                        pcrResultList.Add(info);
                    }
                }
            }

            return isRightFormat;
        }

        private bool processBIORADFile(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber, out string PCRStartTime, out string PCREndTime)
        {
            isRightPositionNumber = true;
            string formatTitle = "Well,Fluor,Target,Content,Sample,Cq,Starting Quantity (SQ)";
            bool isRightFormat = false;
            System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();
            PCRStartTime = "";
            PCREndTime = "";

            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string content = reader.ReadLine();
                        string[] values = content.Split(',');
                        if (values.Count() > 1)
                        {
                            if (values[0] == "Run Started")
                            {
                                PCRStartTime = values[1];
                            }
                            else if (values[0] == "Run Ended")
                            {
                                PCREndTime = values[1];
                            }
                        }
                        if (isRightFormat)
                        {
                            if (values.Count() < 6)
                                break;
                            PCRColumnData bioData = new PCRColumnData();
                            bioData.Well = values[0];
                            bioData.Detector = values[1];
                            bioData.Ct = values[5];
                            bioData.SampleName = values[4];
                            bioData.XmlContent = "<Row><Well>" + values[0] + "</Well><Detector>" + values[1] + "</Detector><Ct>" + values[5] + "</Ct><SampleName>" + values[4] + "</SampleName></Row>";

                            if (!dataList.ContainsKey(bioData.Well))
                            {
                                dataList.Add(bioData.Well, bioData);
                            }
                            else
                            {
                                PCRColumnData _bioData = (PCRColumnData)dataList[bioData.Well];
                                if (_bioData.DataList == null)
                                {
                                    _bioData.DataList = new List<PCRColumnData>();
                                    _bioData.DataList.Add(bioData);
                                }
                                else
                                {
                                    _bioData.DataList.Add(bioData);
                                }                                
                            }
                        }

                        if (!isRightFormat && content.StartsWith(formatTitle))
                        {
                            isRightFormat = true;
                        }
                    }
                }
            }

            if (!isRightFormat)
                return isRightFormat;            

            Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
            if (dc.Count != dataList.Count)
            {
                isRightPositionNumber = false;
                if (!isRightPositionNumber)
                {
                    MessageBoxResult selectResult = MessageBox.Show("文件记录数[" + dataList.Count.ToString() + "]和检测数[" + dc.Count.ToString()
                        + "]不一致，是否继续导入？“是”继续导入", "系统提示", MessageBoxButton.YesNo); 
                    if (selectResult == MessageBoxResult.No)
                    {
                        return isRightFormat;
                    }
                    else
                    {
                        isRightPositionNumber = true;
                    }
                }
            }

            foreach (PCRColumnData bioData in dataList.Values)
            {
                PCRTestResult info = new PCRTestResult();
                info.ItemID = WanTaiObjectService.NewSequentialGuid();
                info.RotationID = rotationId;
                info.Position = controller.ChangeCharacterToPositionNumber(bioData.Well);
                info.CreateTime = DateTime.Now;
                //calculate 
                if (dc.ContainsKey(info.Position))
                {
                    DataRow row = dc[info.Position];
                    bool isSingle = (int)row["TubeNumber"] == 1 ? true : false;
                    Tubetype tubeType = (Tubetype)((short)row["TubeType"]);
                    info.Result = checkPCRResult(bioData, row["TestName"].ToString(), isSingle, tubeType);                   
                    info.ExperimentID = currentExperimentId;
                    if (plate.BarCode != null)
                    {
                        info.BarCode = plate.BarCode;
                    }

                    info.PlateID = plate.PlateID;
                    string xmlContent = bioData.XmlContent;
                    if (bioData.DataList != null && bioData.DataList.Count > 0)
                    {
                        foreach (PCRColumnData data in bioData.DataList)
                        {
                            xmlContent = xmlContent + data.XmlContent;
                        }
                    }
                    info.PCRContent = "<PCRContent>" + xmlContent + "</PCRContent>";
                    pcrResultList.Add(info);
                } 

            }

            return isRightFormat;
        }

        private bool processBIORADFile100(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber, out string PCRStartTime, out string PCREndTime)
        {
            isRightPositionNumber = true;
            string formatTitle = "Well,Fluor,Target,Content,Sample,Cq,Starting Quantity (SQ)";
            bool isRightFormat = false;
            System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

            PCRStartTime = "";
            PCREndTime = "";
            
            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string content = reader.ReadLine();
                        string[] values = content.Split(',');
                        if (values.Count() > 1)
                        {
                            if (values[0] == "Run Started")
                            {
                                PCRStartTime = values[1];
                            }
                            else if (values[0] == "Run Ended")
                            {
                                PCREndTime = values[1];
                            }
                        }
                        if (isRightFormat)
                        {
                            if (values.Count() < 7)
                                break;
                            PCRColumnData bioData = new PCRColumnData();
                            bioData.Well = values[0];
                            bioData.Detector = values[1];
                            bioData.Ct = values[5];
                            bioData.SampleName = values[4];
                            bioData.Quantity = values[6];
                            bioData.XmlContent = "<Row><Well>" + values[0] + "</Well><Detector>" + values[1] + "</Detector><Ct>" + values[5] + "</Ct><SampleName>" + values[4] + "</SampleName></Row>";

                            if (!dataList.ContainsKey(bioData.Well))
                            {
                                dataList.Add(bioData.Well, bioData);
                            }
                            else
                            {
                                PCRColumnData _bioData = (PCRColumnData)dataList[bioData.Well];
                                if (_bioData.DataList == null)
                                {
                                    _bioData.DataList = new List<PCRColumnData>();
                                    _bioData.DataList.Add(bioData);
                                }
                                else
                                {
                                    _bioData.DataList.Add(bioData);
                                }
                            }
                        }

                        if (!isRightFormat && content.StartsWith(formatTitle))
                        {
                            isRightFormat = true;
                        }
                    }
                }
            }

            if (!isRightFormat)
                return isRightFormat;

            Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
            if (dc.Count != dataList.Count)
            {
                isRightPositionNumber = false;
                if (!isRightPositionNumber)
                {
                    MessageBoxResult selectResult = MessageBox.Show("文件记录数[" + dataList.Count.ToString() + "]和检测数[" + dc.Count.ToString()
                        + "]不一致，是否继续导入？“是”继续导入", "系统提示", MessageBoxButton.YesNo); 
                    if (selectResult == MessageBoxResult.No)
                    {
                        return isRightFormat;
                    }
                    else
                    {
                        isRightPositionNumber = true;
                    }
                }
            }

            foreach (PCRColumnData bioData in dataList.Values)
            {
                PCRTestResult info = new PCRTestResult();
                info.ItemID = WanTaiObjectService.NewSequentialGuid();
                info.RotationID = rotationId;
                info.Position = controller.ChangeCharacterToPositionNumber(bioData.Well);
                info.CreateTime = DateTime.Now;
                //calculate 
                if (dc.ContainsKey(info.Position))
                {
                    DataRow row = dc[info.Position];
                    bool isSingle = (int)row["TubeNumber"] == 1 ? true : false;
                    Tubetype tubeType = (Tubetype)((short)row["TubeType"]);
                    info.Result = checkPCRResult100(bioData, row["TestName"].ToString(), isSingle, tubeType);

                    info.ExperimentID = currentExperimentId;
                    if (plate.BarCode != null)
                    {
                        info.BarCode = plate.BarCode;
                    }

                    info.PlateID = plate.PlateID;
                    string xmlContent = bioData.XmlContent;
                    if (bioData.DataList != null && bioData.DataList.Count > 0)
                    {
                        foreach (PCRColumnData data in bioData.DataList)
                        {
                            xmlContent = xmlContent + data.XmlContent;
                        }
                    }
                    info.PCRContent = "<PCRContent>" + xmlContent + "</PCRContent>";
                    pcrResultList.Add(info);
                }

            }

            return isRightFormat;
        }

        private bool processStratageneFile(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
            connectionStringBuilder.DataSource = fileName; 
            connectionStringBuilder.Add("Mode", "Read");
            connectionStringBuilder.Add("Extended Properties", "Excel 8.0;IMEX=1;TypeGuessRows=0;ImportMixedTypes=Text");
            bool isRightFormat = false;
            bool hasWellColumn = false;
            bool hasDyeColumn = false;
            bool hasCtColumn = false;
            using (OleDbConnection con = new OleDbConnection(connectionStringBuilder.ToString()))
            {
                con.Open();
                DataTable dtSchema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" }); 
                string Sheet1 = dtSchema.Rows[0].Field<string>("TABLE_NAME");

                DataTable sheetColumns = con.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, Sheet1, null });
                foreach (DataRow row in sheetColumns.Rows)
                {
                    string strColumnName = row["COLUMN_NAME"].ToString();
                    if (strColumnName == "Well")
                    {
                        hasWellColumn = true;
                    }
                    else if (strColumnName == "Dye")
                    {
                        hasDyeColumn = true;
                    }
                    if (strColumnName == "Ct (dR)")
                    {
                        hasCtColumn = true;
                    }
                }

                if (hasWellColumn && hasDyeColumn && hasCtColumn)
                {
                    isRightFormat = true;
                }

                if (!isRightFormat)
                    return isRightFormat;

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Dye],[Ct (dR)],[Well Name] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();
            
                foreach (DataRow row in dt.Rows)
                {
                    PCRColumnData sData = new PCRColumnData() { Well=row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString(), SampleName = row[3].ToString() };
                    sData.XmlContent = "<Row><Well>" + sData.Well + "</Well><Detector>" + sData.Detector + "</Detector><Ct>" + sData.Ct + "</Ct><SampleName>" + sData.SampleName + "</SampleName></Row>";
                    if (!dataList.ContainsKey(sData.Well))
                    {
                        dataList.Add(sData.Well, sData);
                    }
                    else
                    {
                        PCRColumnData _sData = (PCRColumnData)dataList[sData.Well];
                        if (_sData.DataList == null)
                        {
                            _sData.DataList = new List<PCRColumnData>();
                            _sData.DataList.Add(sData);
                        }
                        else
                        {
                            _sData.DataList.Add(sData);
                        }
                    }                    
                }

                if (dc.Count != dataList.Count)
                {
                    isRightPositionNumber = false;
                    if (!isRightPositionNumber)
                    {
                        MessageBoxResult selectResult = MessageBox.Show("文件记录数[" + dataList.Count.ToString() + "]和检测数[" + dc.Count.ToString()
                            + "]不一致，是否继续导入？“是”继续导入", "系统提示", MessageBoxButton.YesNo); 
                        if (selectResult == MessageBoxResult.No)
                        {
                            return isRightFormat;
                        }
                        else
                        {
                            isRightPositionNumber = true;
                        }
                    }
                }

                foreach (PCRColumnData sData in dataList.Values)
                {
                    PCRTestResult info = new PCRTestResult();
                    info.ItemID = WanTaiObjectService.NewSequentialGuid();
                    info.RotationID = rotationId;
                    info.Position = controller.ChangeCharacterToPositionNumber(sData.Well);
                    info.CreateTime = DateTime.Now;

                    //calculate 
                    if (dc.ContainsKey(info.Position))
                    {
                        DataRow dcrow = dc[info.Position];
                        bool isSingle = (int)dcrow["TubeNumber"] == 1 ? true : false;
                        Tubetype tubeType = (Tubetype)((short)dcrow["TubeType"]);
                        info.Result = checkPCRResult(sData, dcrow["TestName"].ToString(), isSingle, tubeType);

                        info.ExperimentID = currentExperimentId;
                        if (plate.BarCode != null)
                        {
                            info.BarCode = plate.BarCode;
                        }

                        info.PlateID = plate.PlateID;
                        string xmlContent = sData.XmlContent;
                        if (sData.DataList != null && sData.DataList.Count > 0)
                        {
                            foreach (PCRColumnData data in sData.DataList)
                            {
                                xmlContent = xmlContent + data.XmlContent;
                            }
                        }
                        info.PCRContent = "<PCRContent>" + xmlContent + "</PCRContent>";
                        pcrResultList.Add(info);
                    }

                }
            }            

            return isRightFormat;
        }

        private bool processStratageneFile100(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
            connectionStringBuilder.DataSource = fileName;
            connectionStringBuilder.Add("Mode", "Read");
            connectionStringBuilder.Add("Extended Properties", "Excel 8.0;IMEX=1;TypeGuessRows=0;ImportMixedTypes=Text");
            bool isRightFormat = false;
            bool hasWellColumn = false;
            bool hasDyeColumn = false;
            bool hasCtColumn = false;
            bool hasQuantityColumn = false;
            bool hasDrColumn = false;
            using (OleDbConnection con = new OleDbConnection(connectionStringBuilder.ToString()))
            {
                con.Open();
                DataTable dtSchema = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                string Sheet1 = dtSchema.Rows[0].Field<string>("TABLE_NAME");

                DataTable sheetColumns = con.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, Sheet1, null });
                foreach (DataRow row in sheetColumns.Rows)
                {
                    string strColumnName = row["COLUMN_NAME"].ToString();
                    if (strColumnName == "Well")
                    {
                        hasWellColumn = true;
                    }
                    else if (strColumnName == "Dye")
                    {
                        hasDyeColumn = true;
                    }
                    if (strColumnName == "Ct (dR)")
                    {
                        hasCtColumn = true;
                    }
                    if (strColumnName == "Quantity (copies)")
                    {
                        hasQuantityColumn = true;
                    }
                    if (strColumnName == "RSq (dR)")
                    {
                        hasDrColumn = true;
                    }
                }

                if (hasWellColumn && hasDyeColumn && hasCtColumn && hasQuantityColumn && hasDrColumn)
                {
                    isRightFormat = true;
                }

                if (!isRightFormat)
                    return isRightFormat;

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Dye],[Ct (dR)],[Well Name] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

                foreach (DataRow row in dt.Rows)
                {
                    PCRColumnData sData = new PCRColumnData() { Well = row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString(), SampleName = row[3].ToString() };
                    sData.XmlContent = "<Row><Well>" + sData.Well + "</Well><Detector>" + sData.Detector + "</Detector><Ct>" + sData.Ct + "</Ct><SampleName>" + sData.SampleName + "</SampleName></Row>";
                    if (!dataList.ContainsKey(sData.Well))
                    {
                        dataList.Add(sData.Well, sData);
                    }
                    else
                    {
                        PCRColumnData _sData = (PCRColumnData)dataList[sData.Well];
                        if (_sData.DataList == null)
                        {
                            _sData.DataList = new List<PCRColumnData>();
                            _sData.DataList.Add(sData);
                        }
                        else
                        {
                            _sData.DataList.Add(sData);
                        }
                    }
                }

                if (dc.Count != dataList.Count)
                {
                    isRightPositionNumber = false;
                    if (!isRightPositionNumber)
                    {
                        MessageBoxResult selectResult = MessageBox.Show("文件记录数[" + dataList.Count.ToString() + "]和检测数[" + dc.Count.ToString()
                            + "]不一致，是否继续导入？“是”继续导入", "系统提示", MessageBoxButton.YesNo); 
                        if (selectResult == MessageBoxResult.No)
                        {
                            return isRightFormat;
                        }
                        else
                        {
                            isRightPositionNumber = true;
                        }
                    }
                }

                foreach (PCRColumnData sData in dataList.Values)
                {
                    PCRTestResult info = new PCRTestResult();
                    info.ItemID = WanTaiObjectService.NewSequentialGuid();
                    info.RotationID = rotationId;
                    info.Position = controller.ChangeCharacterToPositionNumber(sData.Well);
                    info.CreateTime = DateTime.Now;

                    //calculate 
                    if (dc.ContainsKey(info.Position))
                    {
                        DataRow dcrow = dc[info.Position];
                        bool isSingle = (int)dcrow["TubeNumber"] == 1 ? true : false;
                        Tubetype tubeType = (Tubetype)((short)dcrow["TubeType"]);
                        info.Result = checkPCRResult100(sData, dcrow["TestName"].ToString(), isSingle, tubeType);

                        info.ExperimentID = currentExperimentId;
                        if (plate.BarCode != null)
                        {
                            info.BarCode = plate.BarCode;
                        }

                        info.PlateID = plate.PlateID;
                        string xmlContent = sData.XmlContent;
                        if (sData.DataList != null && sData.DataList.Count > 0)
                        {
                            foreach (PCRColumnData data in sData.DataList)
                            {
                                xmlContent = xmlContent + data.XmlContent;
                            }
                        }
                        info.PCRContent = "<PCRContent>" + xmlContent + "</PCRContent>";
                        pcrResultList.Add(info);
                    }


                }
            }

            return isRightFormat;
        }

        private string checkPCRResult(PCRColumnData data, string testItemName, bool isSingle, Tubetype tubeType)
        {
            string rox = null;
            string hex = null;
            string fam = null;
            string cy5 = null;
            string HBVResult = null;
            string HCVResult = null;
            string HIVResult = null;


            if ("ROX" == data.Detector)
            {
                rox = data.Ct;
            }
            else if ("FAM" == data.Detector)
            {
                fam = data.Ct;
            }
            else if ("HEX" == data.Detector || "VIC" == data.Detector)
            {
                hex = data.Ct;
            }
            else if ("CY5" == data.Detector.ToUpper())
            {
                cy5 = data.Ct;
            }

            if (data.DataList != null)
            {
                foreach (PCRColumnData subData in data.DataList)
                {
                    if ("ROX" == subData.Detector)
                    {
                        rox = subData.Ct;
                    }
                    else if ("FAM" == subData.Detector)
                    {
                        fam = subData.Ct;
                    }
                    else if ("HEX" == subData.Detector || "VIC" == subData.Detector)
                    {
                        hex = subData.Ct;
                    }
                    else if ("CY5" == subData.Detector.ToUpper())
                    {
                        cy5 = subData.Ct;
                    }
                }
            }
 
            if (!string.IsNullOrEmpty(fam) && (fam == "Undetermined" || fam == "No Ct" || fam == "N/A" || fam == "NaN"))
            {
                fam = "45";
            }
            if (!string.IsNullOrEmpty(hex) && (hex == "Undetermined" || hex == "No Ct" || hex == "N/A" || hex == "NaN"))
            {
                hex = "45";
            }
            if (!string.IsNullOrEmpty(rox) && (rox == "Undetermined" || rox == "No Ct" || rox == "N/A" || rox == "NaN"))
            {
                rox = "45";
            }
            if (!string.IsNullOrEmpty(cy5) && (cy5 == "Undetermined" || cy5 == "No Ct" || cy5 == "N/A" || cy5 == "NaN"))
            {
                cy5 = "45";
            }

            float outNumber = 0;

            //阴性对照
            if (tubeType == Tubetype.NegativeControl)
            {
                if (testItemName == "HBV")
                {
                    if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                    {
                        float hexNumber = float.Parse(hex);
                        if (hexNumber > 37 && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                        {
                            return PCRTest.NegativeResult;
                        }
                    }

                    return PCRTest.InvalidResult;
                }
                else if (testItemName == "HCV")
                {
                    if (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber))
                    {
                        float famNumber = float.Parse(fam);
                        if (famNumber > 42 && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                        {
                            return PCRTest.NegativeResult;
                        }                        
                    }

                    return PCRTest.InvalidResult;
                }
                else if (testItemName == "HIV")
                {
                    if (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber))
                    {
                        float famNumber = float.Parse(fam);
                        if (famNumber > 42 && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                        {
                            return PCRTest.NegativeResult;
                        }
                    }

                    return PCRTest.InvalidResult;
                }
                else if (testItemName == "BCI")
                {
                    bool hbvValid = false;
                    bool hcvValid = false;
                    bool hivValid = false;
                    if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                    {
                        float hexNumber = float.Parse(hex);
                        if (hexNumber <= 35) {
                            // check HBV
                            hbvValid = (rox == "45");
                            // check HCV
                            hcvValid = (fam == "45");
                            // check HIV
                            hivValid = (cy5 == "45");
                        }
                    }
                    HBVResult = hbvValid ? PCRTest.NegativeResult : PCRTest.InvalidResult;
                    HCVResult = hcvValid ? PCRTest.NegativeResult : PCRTest.InvalidResult;
                    HIVResult = hivValid ? PCRTest.NegativeResult : PCRTest.InvalidResult;

                    return "BCI|" + HBVResult + "|" + HCVResult + "|" + HIVResult;
                }
                else
                {
                    return PCRTest.NoResult;
                }
            }
            else if (tubeType == Tubetype.PositiveControl)
            {
                if (testItemName == "HBV")
                {
                    if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                    {
                        float hexNumber = float.Parse(hex);
                        if (hexNumber <= 33 && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                        {
                            return PCRTest.PositiveResult;
                        }
                    }

                    return PCRTest.InvalidResult;
                }
                else if (testItemName == "HCV")
                {
                    if (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber))
                    {
                        float famNumber = float.Parse(fam);
                        if (famNumber <=37 && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                        {
                            return PCRTest.PositiveResult;
                        }
                    }

                    return PCRTest.InvalidResult;
                }
                else if (testItemName == "HIV")
                {
                    if (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber))
                    {
                        float famNumber = float.Parse(fam);
                        if (famNumber <=37 && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                        {
                            return PCRTest.PositiveResult;
                        }
                    }

                    return PCRTest.InvalidResult;
                }
                else if (testItemName == "BCI")
                {
                    bool hbvValid = false;
                    bool hcvValid = false;
                    bool hivValid = false;
                    //if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                    //{
                    //    float hexNumber = float.Parse(hex);
                    //    if (hexNumber <= 35)
                    //    {
                    // check HBV
                    hbvValid = float.TryParse(rox, out outNumber) && float.Parse(rox) <= 33;
                    // check HCV
                    hcvValid = float.TryParse(fam, out outNumber) && float.Parse(fam) <= 33;
                    // check HIV
                    hivValid = float.TryParse(cy5, out outNumber) && float.Parse(cy5) <= 33;
                    //    }
                    //}
                    HBVResult = hbvValid ? PCRTest.PositiveResult : PCRTest.InvalidResult;
                    HCVResult = hcvValid ? PCRTest.PositiveResult : PCRTest.InvalidResult;
                    HIVResult = hivValid ? PCRTest.PositiveResult : PCRTest.InvalidResult;

                    return "BCI|" + HBVResult + "|" + HCVResult + "|" + HIVResult;
                }
                else
                {
                    return PCRTest.NoResult;
                }
            }

            float HBVValue = 0;
            float HCVValue = 0;
            float HIVValue = 0;
            float BCIValue = 35;
            if (isSingle)
            {
                HBVValue = 35;
                HCVValue = 42;
                HIVValue = 40;
            }
            else
            {
                HBVValue = 37;
                HCVValue = 42;
                HIVValue = 42;
            }

            if (testItemName == "HBV")
            {
                if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                {
                    float hexNumber = float.Parse(hex);

                    if (hex == "45" && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.NegativeResult;
                    }
                    else if (hexNumber > HBVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.LowResult;
                    }
                    else if (hexNumber <= HBVValue)
                    {
                        return PCRTest.PositiveResult;
                    }
                    else if (hexNumber > HBVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) >= 36)
                    {
                        return PCRTest.InvalidResult;
                    }
                    else
                    {
                        return PCRTest.NoResult;
                    }
                }
                else
                {
                    return PCRTest.NoResult;
                }
            }
            else if (testItemName == "HCV")
            {
                if (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber))
                {
                    float famNumber = float.Parse(fam);
                    if (fam == "45" && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.NegativeResult;
                    }
                    else if (famNumber > HCVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.LowResult;
                    }
                    else if (famNumber <= HCVValue)
                    {
                        return PCRTest.PositiveResult;
                    }
                    else if (famNumber > HCVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) >= 36)
                    {
                        return PCRTest.InvalidResult;
                    }
                    else
                    {
                        return PCRTest.NoResult;
                    }
                }
                else
                {
                    return PCRTest.NoResult;
                }
            }
            else if (testItemName == "HIV")
            {
                if (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber))
                {
                    float famNumber = float.Parse(fam);
                    if (fam == "45" && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.NegativeResult;
                    }
                    else if (famNumber > HIVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.LowResult;
                    }
                    else if (famNumber <= HIVValue)
                    {
                        return PCRTest.PositiveResult;
                    }
                    else if (famNumber > HIVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) >= 36)
                    {
                        return PCRTest.InvalidResult;
                    }
                    else
                    {
                        return PCRTest.NoResult;
                    }
                }
                else
                {
                    return PCRTest.NoResult;
                }
            }
            else if (testItemName == "BCI")
            {
                // check HBV result
                if (!string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) <= BCIValue)
                {
                    HBVResult = PCRTest.PositiveResult;
                }
                else if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                {
                    float hexNumber = float.Parse(hex);
                    if (hexNumber <= BCIValue && rox == "45")
                    {
                        HBVResult = PCRTest.NegativeResult;
                    }
                    else if (hexNumber > BCIValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) > BCIValue)
                    {
                        HBVResult = PCRTest.InvalidResult;
                    }
                    else if (hexNumber <= BCIValue && (!string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) > BCIValue))
                    {
                        HBVResult = PCRTest.BCILowResult;
                    }
                    else
                    {
                        HBVResult = PCRTest.NoResult;
                    }
                }
                else
                {
                    HBVResult = PCRTest.NoResult;
                }
                // check HCV result
                if (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber) && float.Parse(fam) <= BCIValue)
                {
                    HCVResult = PCRTest.PositiveResult;
                }
                else if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                {
                    float hexNumber = float.Parse(hex);
                    if (hexNumber <= BCIValue && fam == "45")
                    {
                        HCVResult = PCRTest.NegativeResult;
                    }
                    else if (hexNumber > BCIValue && !string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber) && float.Parse(fam) > BCIValue)
                    {
                        HCVResult = PCRTest.InvalidResult;
                    }
                    else if (hexNumber <= BCIValue && (!string.IsNullOrEmpty(fam) && float.TryParse(fam, out outNumber) && float.Parse(fam) > BCIValue))
                    {
                        HCVResult = PCRTest.BCILowResult;
                    }
                    else
                    {
                        HCVResult = PCRTest.NoResult;
                    }
                }
                else
                {
                    HCVResult = PCRTest.NoResult;
                }
                // check HIV result
                if (!string.IsNullOrEmpty(cy5) && float.TryParse(cy5, out outNumber) && float.Parse(cy5) <= BCIValue)
                {
                    HIVResult = PCRTest.PositiveResult;
                }
                else if (!string.IsNullOrEmpty(hex) && float.TryParse(hex, out outNumber))
                {
                    float hexNumber = float.Parse(hex);
                    if (hexNumber <= BCIValue && cy5 == "45")
                    {
                        HIVResult = PCRTest.NegativeResult;
                    }
                    else if (hexNumber > BCIValue && !string.IsNullOrEmpty(cy5) && float.TryParse(cy5, out outNumber) && float.Parse(cy5) > BCIValue)
                    {
                        HIVResult = PCRTest.InvalidResult;
                    }
                    else if (hexNumber <= BCIValue && (!string.IsNullOrEmpty(cy5) && float.TryParse(cy5, out outNumber) && float.Parse(cy5) > BCIValue))
                    {
                        HIVResult = PCRTest.BCILowResult;
                    }
                    else
                    {
                        HIVResult = PCRTest.NoResult;
                    }
                }
                else
                {
                    HIVResult = PCRTest.NoResult;
                }
                return "BCI|" + HBVResult + "|" + HCVResult + "|" + HIVResult;
            }
            else
            {
                return PCRTest.NoResult;
            }            
        }

        private string checkPCRResult100(PCRColumnData data, string testItemName, bool isSingle, Tubetype tubeType)
        {
            string CT = null, IC = null, Quantity = null, DR = null;

            // get ct and ic
            if (testItemName == "HBV")
            {
                if ("VIC" == data.Detector || "HEX" == data.Detector)
                {
                    CT = data.Ct;
                    DR = data.Dr;
                    if (null != data.Quantity)
                        Quantity = data.Quantity;
                }
                else if ("ROX" == data.Detector)
                {
                    IC = data.Ct;
                }
                if (data.DataList != null)
                {
                    foreach (PCRColumnData subData in data.DataList)
                    {
                        if ("VIC" == subData.Detector || "HEX" == subData.Detector)
                        {
                            CT = subData.Ct;
                            if (null != subData.Quantity)
                                Quantity = subData.Quantity;
                        }
                        else if ("ROX" == subData.Detector)
                        {
                            IC = subData.Ct;
                        }
                    }
                }
            }
            else if (testItemName == "HCV")
            {
                if ("ROX" == data.Detector)
                {
                    CT = data.Ct;
                    if (null != data.Quantity)
                        Quantity = data.Quantity;
                }
                else if ("FAM" == data.Detector)
                {
                    IC = data.Ct;
                }
                if (data.DataList != null)
                {
                    foreach (PCRColumnData subData in data.DataList)
                    {
                        if ("ROX" == subData.Detector)
                        {
                            CT = subData.Ct;
                            if (null != subData.Quantity)
                                Quantity = subData.Quantity;
                        }
                        else if ("FAM" == subData.Detector)
                        {
                            IC = subData.Ct;
                        }
                    }
                }
            }
            else if (testItemName == "HIV")
            {
                if ("FAM" == data.Detector)
                {
                    CT = data.Ct;
                    if (null != data.Quantity)
                        Quantity = data.Quantity;
                }
                else if ("VIC" == data.Detector || "HEX" == data.Detector)
                {
                    IC = data.Ct;
                }
                if (data.DataList != null)
                {
                    foreach (PCRColumnData subData in data.DataList)
                    {
                        if ("FAM" == subData.Detector)
                        {
                            CT = subData.Ct;
                            if (null != subData.Quantity)
                                Quantity = subData.Quantity;
                        }
                        else if ("VIC" == subData.Detector || "HEX" == subData.Detector)
                        {
                            IC = subData.Ct;
                        }
                    }
                }
            }
 
            float outNumber = 0;

            //阴性对照
            if (tubeType == Tubetype.NegativeControl)
            {
                if (!string.IsNullOrEmpty(IC) 
                    && float.TryParse(IC, out outNumber) 
                    && float.Parse(IC) <= 35
                    && null != Quantity
                    && ("No Ct" == Quantity || "Undetermined" == Quantity || "N/A" == Quantity || "NaN" == Quantity || Quantity.Trim() == ""))
                {
                    return Quantity;
                }
                else
                {
                    return PCRTest.InvalidResult;
                }
            }
            else if (tubeType == Tubetype.PositiveControl){
                if (!string.IsNullOrEmpty(IC) 
                    && float.TryParse(IC, out outNumber) 
                    && float.Parse(IC) <= 35
                    && !string.IsNullOrEmpty(Quantity)
                    && float.TryParse(Quantity, out outNumber)
                    && float.Parse(Quantity) >= 355
                    && float.Parse(Quantity) <= 2820)
                {
                    return Quantity;
                }
                else
                {
                    return PCRTest.InvalidResult;
                }
            } 
            else
            {
                if (testItemName == "HBV")
                {
                    if (!string.IsNullOrEmpty(IC) 
                        && float.TryParse(IC, out outNumber) 
                        && float.Parse(IC) <= 35
                        && null != CT
                        && ("No Ct" == CT || "N/A" == CT || "NaN" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || Quantity.Trim() == "" || Quantity == "N/A" || Quantity == "NaN" || "Undetermined" == Quantity))
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(IC) 
                        && float.TryParse(IC, out outNumber) 
                        && float.Parse(IC) <= 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) < 10)
                    {
                        return "＜10";
                    }
                    else if (!string.IsNullOrEmpty(IC) 
                        && float.TryParse(IC, out outNumber) 
                        && float.Parse(IC) <= 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) >= 10
                        && float.Parse(Quantity) <= 10000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) > 10000
                        && float.Parse(Quantity) <= 1000000000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) > 1000000000)
                    {
                        return  "> 10E9";
                    }
                    else if (!string.IsNullOrEmpty(IC) 
                        && float.TryParse(IC, out outNumber) 
                        && float.Parse(IC) > 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) < 10000)
                    {
                        return PCRTest.InvalidResult;
                    }
                    else
                    {
                        return PCRTest.NoResult;
                    }
                }
                else if (testItemName == "HCV")
                {
                    if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) <= 35
                        && null != CT
                        && ("No Ct" == CT || "N/A" == CT || "NaN" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || Quantity.Trim() == "" || Quantity.Trim() == "N/A" || Quantity.Trim() == "NaN"|| "Undetermined" == Quantity))
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) <= 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) < 50)
                    {
                        return "＜50";
                    }
                    else if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) <= 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) >= 50
                        && float.Parse(Quantity) <= 100000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) > 100000
                        && float.Parse(Quantity) <= 100000000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) > 100000000)
                    {
                        return "> 10E8";
                    }
                    else if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) > 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) < 10000)
                    {
                        return PCRTest.InvalidResult;
                    }
                    else
                    {
                        return PCRTest.NoResult;
                    }
                }
                else if (testItemName == "HIV")
                {
                    if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) <= 35
                        && null != CT
                        && ("No Ct" == CT || "N/A" == CT || "NaN" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || "N/A" == Quantity || "NaN" == Quantity || Quantity.Trim() == "" || "Undetermined" == Quantity))
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) <= 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) < 50)
                    {
                        return "＜50";
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) >= 50
                        && float.Parse(Quantity) <= 10000000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) > 10000000)
                    {
                        return "> 10E7";
                    }
                    else if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) > 35
                        && !string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) < 50)
                    {
                        return PCRTest.InvalidResult;
                    }
                    else if (!string.IsNullOrEmpty(IC)
                        && float.TryParse(IC, out outNumber)
                        && float.Parse(IC) > 35
                        && null != CT
                        && ("No Ct" == CT || "N/A" == CT || "NaN" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || "N/A" == Quantity || "NaN" == Quantity|| Quantity.Trim() == "" || "Undetermined" == Quantity))
                    {
                        return PCRTest.InvalidResult;
                    }
                    else
                    {
                        return PCRTest.NoResult;
                    }
                }
                else
                {
                    return PCRTest.NoResult;
                }
            }
        }

        private bool validate()
        {
            if (rotation_comboBox.SelectedIndex < 0)
            {
                MessageBox.Show("请选择轮次", "系统提示");
                rotation_comboBox.Focus();
                return false;
            }

            if (barcode_comboBox.SelectedIndex<0)
            {
                MessageBox.Show("请选择PCR板条码", "系统提示");
                barcode_comboBox.Focus();
                return false;
            }

            if (type_comboBox.SelectedIndex < 0)
            {
                MessageBox.Show("请选择PCR仪类型", "系统提示");
                type_comboBox.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(file_textBox.Text))
            {
                MessageBox.Show("请选择文件上传", "系统提示");
                file_textBox.Focus();
                return false;
            }

            if ((type_comboBox.SelectedIndex == 0 || type_comboBox.SelectedIndex == 2) && 
                !(System.IO.Path.GetExtension(this.file_textBox.Text).Equals(".xls", StringComparison.CurrentCultureIgnoreCase)
                || System.IO.Path.GetExtension(this.file_textBox.Text).Equals(".xlsx", StringComparison.CurrentCultureIgnoreCase)))
            {
                MessageBox.Show("请选择正确的文件上传", "系统提示");
                file_textBox.Focus();
                return false;
            }

            if ((type_comboBox.SelectedIndex == 1) &&
                !System.IO.Path.GetExtension(this.file_textBox.Text).Equals(".csv", StringComparison.CurrentCultureIgnoreCase))
            {
                MessageBox.Show("请选择正确的文件上传", "系统提示");
                file_textBox.Focus();
                return false;
            }

            return true;
        }

        private void selectFile_button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            if (type_comboBox.SelectedIndex == 1)
            {
                fileDialog.Filter = "Text Files (*.csv)|*.csv";
            }
            else if(type_comboBox.SelectedIndex == 0 || type_comboBox.SelectedIndex == 2)
            {
                fileDialog.Filter = "All Microsoft Office Excel Files (*.xls)|*.xls";
            }

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.file_textBox.Text = fileDialog.FileName;                             
            }
        }       

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void rotation_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rotation_comboBox.SelectedItem == null) return;
            ComboBoxItem selectedItem = (ComboBoxItem)rotation_comboBox.SelectedItem;
            Guid rotationId = (Guid)selectedItem.Tag;
            barcode_comboBox.Items.Clear();
            List<Plate> plateList = controller.GetPCRPlateBarcode(rotationId, currentExperimentId);
            ExperimentsInfo experimentInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(currentExperimentId);
            PlateController plateController = new PlateController();
            if (plateList != null && plateList.Count > 0)
            {
                int plateIndex = 0;
                foreach (Plate plate in plateList)
                {
                    plateIndex++;
                    Guid plateId = plate.PlateID;
                    bool hasRecord = controller.IsPlateHasImportedResult(rotationId, plateId, currentExperimentId);
                    if (string.IsNullOrEmpty(plate.BarCode)) {

                        plate.BarCode = experimentInfo.StartTime.ToString("yyyyMMdd") + "-" + plateIndex;
                        plateController.UpdateBarcode(plateId, plate.BarCode);
                    }
                    barcode_comboBox.Items.Add(new ComboBoxItem() { Content = (string.IsNullOrEmpty(plate.BarCode) ? ("("+PlateName.PCRPlate + "-" + plateIndex.ToString() + ")" + (hasRecord ? "(已导入)" : "")) : (plate.BarCode + (hasRecord ? "(已导入)" : ""))), DataContext=plate});
                }
                foreach (ComboBoxItem barcode_item in barcode_comboBox.Items)
                {
                    if (!barcode_item.Content.ToString().Contains("(已导入)"))
                    {
                        barcode_item.IsSelected = true;
                        return;
                    }
                }
            }
        }

        private bool set_next_empty_item()
        {
            foreach (ComboBoxItem rotation_item in rotation_comboBox.Items)
            {
                Guid rotationId = (Guid)rotation_item.Tag;
                barcode_comboBox.Items.Clear();
                List<Plate> plateList = controller.GetPCRPlateBarcode(rotationId, currentExperimentId);
                if (plateList != null && plateList.Count > 0)
                {
                    foreach (Plate plate in plateList)
                    {
                        Guid plateId = plate.PlateID;
                        bool hasRecord = controller.IsPlateHasImportedResult(rotationId, plateId, currentExperimentId);
                        if (!hasRecord)
                        {
                            rotation_item.IsSelected = false;
                            rotation_item.IsSelected = true;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void AutoImportPCRResults()
        {
            if (!hasEmptyResults) return;
            int importedNum = 0;
            ExperimentsInfo experimentInfo = new WanTai.Controller.HistoryQuery.ExperimentsController().GetExperimentById(currentExperimentId);
            List<RotationInfo> rotationList = controller.GetFinishedRotation(currentExperimentId);
            // Get today's earlier experiments      
            int rotationIndex = new WanTai.Controller.HistoryQuery.ExperimentsController().GetEarlierRotationCount(experimentInfo.StartTime);
            int plateIndex = 0;
            foreach (RotationInfo info in rotationList)
            {
                rotationIndex ++;
                List<Plate> plateList = controller.GetPCRPlateBarcode(info.RotationID, currentExperimentId);
                if (plateList != null && plateList.Count > 0)
                {
                    plateIndex = 0;
                    foreach (Plate plate in plateList)
                    {
                        plateIndex++;
                        Guid plateId = plate.PlateID;
                        bool hasRecord = controller.IsPlateHasImportedResult(info.RotationID, plateId, currentExperimentId);
                        if (!hasRecord)
                        {
                            // get item names
                            List<string> testItemNames = controller.GetTestItemNamesByPlateID(plateId, currentExperimentId);
                            Dictionary<string, int> matchedFileNameDict = new Dictionary<string,int>();
                            foreach (string testItemName in testItemNames)
                            {
                                string needFileName = experimentInfo.StartTime.ToString("yyyy-M-d-") + rotationIndex + "-*" + testItemName + "*.*";
                                string[] matchFileNames = System.IO.Directory.GetFiles(PCRFilePath, needFileName);
                                foreach (string filename in matchFileNames) {
                                    if (!matchedFileNameDict.ContainsKey(filename)) {
                                        matchedFileNameDict.Add(filename, 1);
                                    }
                                }
                            }
                            foreach (string filename in matchedFileNameDict.Keys) {
                                string matchFileName = null;
                                string PCRDevice = "";
                                string extension = System.IO.Path.GetExtension(filename);
                                if (extension.Equals(".xls", StringComparison.CurrentCultureIgnoreCase) || extension.Equals(".xlsx", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    matchFileName = filename;
                                    PCRDevice = "ABI 7500";
                                }
                                else if (extension.Equals(".csv", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    matchFileName = filename;
                                    PCRDevice = "BIO-RAD CFX96";
                                }
                                bool isRightFormat = false;
                                bool isRightPositionNumber = true;
                                string PCRStartTime = "";
                                string PCREndTime = "";

                                if (matchFileName != null)
                                {
                                    if (PCRDevice == "ABI 7500")
                                    {
                                        if (SessionInfo.WorkDeskType != "100")
                                        {
                                            isRightFormat = preProcessABIFile(matchFileName, out PCRStartTime, out PCREndTime) && processABIFile(matchFileName, info.RotationID, plate, out isRightPositionNumber);
                                        }
                                        else
                                        {
                                            isRightFormat = preProcessABIFile(matchFileName, out PCRStartTime, out PCREndTime) && processABIFile100(matchFileName, info.RotationID, plate, out isRightPositionNumber);
                                        }
                                    }
                                    else if (PCRDevice == "BIO-RAD CFX96")
                                    {
                                        if (SessionInfo.WorkDeskType != "100")
                                        {
                                            isRightFormat = processBIORADFile(matchFileName, info.RotationID, plate, out isRightPositionNumber, out PCRStartTime, out PCREndTime);
                                        }
                                        else
                                        {
                                            isRightFormat = processBIORADFile100(matchFileName, info.RotationID, plate, out isRightPositionNumber, out PCRStartTime, out PCREndTime);
                                        }
                                    }
                                    else
                                    {
                                        if (SessionInfo.WorkDeskType != "100")
                                        {
                                            isRightFormat = processStratageneFile(matchFileName, info.RotationID, plate, out isRightPositionNumber);
                                        }
                                        else
                                        {
                                            isRightFormat = processStratageneFile100(matchFileName, info.RotationID, plate, out isRightPositionNumber);
                                        }

                                    }

                                    if (!isRightFormat)
                                    {
                                        MessageBox.Show("[" + matchFileName + "]文件格式不对，请选择正确的文件", "系统提示");
                                        return;
                                    }

                                    if (!isRightPositionNumber)
                                    {
                                        return;
                                    }

                                    bool result = controller.Create(pcrResultList) && controller.SetPCRPlateExtContent(plateId, currentExperimentId, PCRStartTime, PCREndTime, PCRDevice);
                                    WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "导入PCR检测结果" + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
                                    if (result)
                                    {
                                        importedNum++;
                                        pcrResultList = new List<PCRTestResult>();

                                    }
                                    else
                                    {
                                        MessageBox.Show("[" + matchFileName + "]文件导入失败", "系统提示");
                                    }
                                }
                            }
                         }
                    }
                }
            }

            MessageBox.Show("成功导入 " + importedNum + " 个文件！", "系统提示");

            this.Close();
        }
    }

    class PCRColumnData
    {
        public string Well;
        public string Detector;
        public string XmlContent;
        public string Quantity;
        public string Dr;
        public string Ct;
        public string SampleName;
        public List<PCRColumnData> DataList;
    }


    /*
    class ABIPCRColumnData : PCRColumnData
    { 
        public string Detector;
        public string Ct;
    }

    class BIORADPCRColumnData : PCRColumnData
    {
        public string Fluor;
        public string Cq;        
    }

    class StratagenePCRColumnData : PCRColumnData
    {
        public string Dye;
        public string Ct;
    }*/
}
