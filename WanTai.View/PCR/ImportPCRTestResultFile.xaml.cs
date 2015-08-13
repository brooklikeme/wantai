using System;
using System.Collections.Generic;
using System.Collections;
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
using System.IO;
using System.Data.OleDb;
using System.Data;

using WanTai.DataModel;
using WanTai.Controller.PCR;

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
        Guid currentExperimentId;

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
            if (type_comboBox.SelectedIndex == 0)
            {
                if (100 == 100)
                {
                    isRightFormat = processABIFile(fileName, rotationId, plate, out isRightPositionNumber);
                } else 
                {
                    isRightFormat = processABIFile100(fileName, rotationId, plate, out isRightPositionNumber); 
                }               
            }
            else if (type_comboBox.SelectedIndex == 1)
            {
                if (100 == 100)
                {
                    isRightFormat = processBIORADFile(fileName, rotationId, plate, out isRightPositionNumber);
                }
                else
                {
                    isRightFormat = processBIORADFile100(fileName, rotationId, plate, out isRightPositionNumber);
                }
            }
            else if (type_comboBox.SelectedIndex == 2)
            {
                if (100 == 100)
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

            bool result = controller.Create(pcrResultList);
            WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "导入PCR检测结果" + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), SessionInfo.ExperimentID);
            if (result)
            {
                MessageBox.Show("导入成功", "系统提示");
            }
            else
            {
                MessageBox.Show("导入失败", "系统提示");
            }

            this.Close();
        }        

        private bool processABIFile(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
            connectionStringBuilder.DataSource = fileName;
            connectionStringBuilder.Add("Mode", "Read");
            connectionStringBuilder.Add("Extended Properties", "Excel 8.0;IMEX=1;TypeGuessRows=0;ImportMixedTypes=Text");
            bool isRightFormat = false;
            bool hasWellColumn = false;
            bool hasDetectorColumn = false;
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
                    else if (strColumnName == "Reporter")
                    {
                        hasDetectorColumn = true;
                    }
                    if (strColumnName == "Cт")
                    {
                        hasCtColumn = true;
                    }
                }

                if (hasWellColumn && hasDetectorColumn && hasCtColumn)
                {
                    isRightFormat = true;
                }

                if (!isRightFormat)
                    return isRightFormat;

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Reporter],[Cт] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

                foreach (DataRow row in dt.Rows)
                {
                    PCRColumnData sData = new PCRColumnData() { Well = row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString() };
                    sData.XmlContent = "<Well>" + sData.Well + "</Well><Detector>" + sData.Detector + "</Detector><Ct>" + sData.Ct + "</Ct>";
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
                        MessageBoxResult selectResult = MessageBox.Show("PCR仪检测结果和混样数不一致，请检查操作是否正确？“是”将按现有规则导入", "系统提示", MessageBoxButton.YesNo);
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
                    }

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

            return isRightFormat;
        }

        private bool processABIFile100(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            OleDbConnectionStringBuilder connectionStringBuilder = new OleDbConnectionStringBuilder();
            connectionStringBuilder.Provider = "Microsoft.Jet.OLEDB.4.0";
            connectionStringBuilder.DataSource = fileName;
            connectionStringBuilder.Add("Mode", "Read");
            connectionStringBuilder.Add("Extended Properties", "Excel 8.0;IMEX=1;TypeGuessRows=0;ImportMixedTypes=Text");
            bool isRightFormat = false;
            bool hasWellColumn = false;
            bool hasDetectorColumn = false;
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
                }

                if (hasWellColumn && hasDetectorColumn && hasCtColumn && hasQuantityColumn && hasDrColumn)
                {
                    isRightFormat = true;
                }

                if (!isRightFormat)
                    return isRightFormat;

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Reporter],[Cт],[Quantity],[RSq (dR)] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

                foreach (DataRow row in dt.Rows)
                {
                    PCRColumnData sData = new PCRColumnData() { Well = row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString(), Quantity = row[3].ToString(), Dr = row[4].ToString() };
                    sData.XmlContent = "<Well>" + sData.Well + "</Well><Detector>" + sData.Detector + "</Detector><Ct>" + sData.Ct + "</Ct><Quantity>" + sData.Quantity + "</Quantity><Dr>" + sData.Dr + "</Dr>";
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
                        MessageBoxResult selectResult = MessageBox.Show("PCR仪检测结果和混样数不一致，请检查操作是否正确？“是”将按现有规则导入", "系统提示", MessageBoxButton.YesNo);
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
                    }

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

            return isRightFormat;
        }

        private bool processBIORADFile(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            string formatTitle = "Well,Fluor,Target,Content,Sample,Cq,Starting Quantity (SQ)";
            bool isRightFormat = false;
            System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();
            
            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string content = reader.ReadLine();
                        if (isRightFormat)
                        {
                            string[] values = content.Split(',');
                            if (values.Count() < 6)
                                break;
                            PCRColumnData bioData = new PCRColumnData();
                            bioData.Well = values[0];
                            bioData.Detector = values[1];
                            bioData.Ct = values[5];
                            bioData.XmlContent = "<Well>" + values[0] + "</Well><Fluor>" + values[1] + "</Fluor><Cq>" + values[5] + "</Cq>";

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
                    MessageBoxResult selectResult = MessageBox.Show("PCR仪检测结果和混样数不一致，请检查操作是否正确？“是”将按现有规则导入", "系统提示", MessageBoxButton.YesNo);
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
                } 

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

            return isRightFormat;
        }

        private bool processBIORADFile100(string fileName, Guid rotationId, Plate plate, out bool isRightPositionNumber)
        {
            isRightPositionNumber = true;
            string formatTitle = "Well,Fluor,Target,Content,Sample,Cq,Starting Quantity (SQ)";
            bool isRightFormat = false;
            System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string content = reader.ReadLine();
                        if (isRightFormat)
                        {
                            string[] values = content.Split(',');
                            if (values.Count() < 7)
                                break;
                            PCRColumnData bioData = new PCRColumnData();
                            bioData.Well = values[0];
                            bioData.Detector = values[1];
                            bioData.Ct = values[5];
                            bioData.Quantity = values[6];
                            bioData.XmlContent = "<Well>" + values[0] + "</Well><Fluor>" + values[1] + "</Fluor><Cq>" + values[5] + "</Cq><Quantity>" + values[6] + "</Quantity>";

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
                    MessageBoxResult selectResult = MessageBox.Show("PCR仪检测结果和混样数不一致，请检查操作是否正确？“是”将按现有规则导入", "系统提示", MessageBoxButton.YesNo);
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
                }

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

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Dye],[Ct (dR)] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();
            
                foreach (DataRow row in dt.Rows)
                {
                    PCRColumnData sData = new PCRColumnData() { Well=row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString() };
                    sData.XmlContent = "<Well>" + sData.Well + "</Well><Dye>" + sData.Detector + "</Dye><Ct>" + sData.Ct + "</Ct>";
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
                        MessageBoxResult selectResult = MessageBox.Show("PCR仪检测结果和混样数不一致，请检查操作是否正确？“是”将按现有规则导入", "系统提示", MessageBoxButton.YesNo);
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
                    }

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

                OleDbDataAdapter da = new OleDbDataAdapter("select [Well],[Dye],[Ct (dR)],[Quantity (copies)],[RSq (dR)] from [" + Sheet1 + "]", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                Dictionary<int, DataRow> dc = controller.GetPCRPositionsByPlateID(plate.PlateID, currentExperimentId);
                System.Collections.Generic.Dictionary<string, PCRColumnData> dataList = new System.Collections.Generic.Dictionary<string, PCRColumnData>();

                foreach (DataRow row in dt.Rows)
                {
                    PCRColumnData sData = new PCRColumnData() { Well = row[0].ToString(), Detector = row[1].ToString(), Ct = row[2].ToString(), Quantity = row[3].ToString(), Dr = row[4].ToString() };
                    sData.XmlContent = "<Well>" + sData.Well + "</Well><Dye>" + sData.Detector + "</Dye><Ct>" + sData.Ct + "</Ct><Quantity>" + sData.Quantity + "</Quantity><Dr>" + sData.Dr + "</Dr>";
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
                        MessageBoxResult selectResult = MessageBox.Show("PCR仪检测结果和混样数不一致，请检查操作是否正确？“是”将按现有规则导入", "系统提示", MessageBoxButton.YesNo);
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
                    }

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

            return isRightFormat;
        }

        private string checkPCRResult(PCRColumnData data, string testItemName, bool isSingle, Tubetype tubeType)
        {
            string rox = null;
            string hex = null;
            string fam = null;

            if ("ROX" == data.Detector)
            {
                rox = data.Ct;
            }
            else if ("FAM" == data.Detector)
            {
                fam = data.Ct;
            }
            else if ("HEX" == data.Detector)
            {
                hex = data.Ct;
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
                    else if ("HEX" == subData.Detector)
                    {
                        hex = subData.Ct;
                    }
                }
            }
 
            if (!string.IsNullOrEmpty(fam) && (fam == "Undetermined" || fam == "No Ct" || fam == "N/A"))
            {
                fam = "45";
            }
            if (!string.IsNullOrEmpty(hex) && (hex == "Undetermined" || hex == "No Ct" || hex == "N/A"))
            {
                hex = "45";
            }
            if (!string.IsNullOrEmpty(rox) && (rox == "Undetermined" || rox == "No Ct" || rox == "N/A"))
            {
                rox = "45";
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
                else
                {
                    return PCRTest.NoResult;
                }
            }

            float HBVValue = 0;
            float HCVValue = 0;
            float HIVValue = 0;
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
                    if (hexNumber > HBVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.NegativeResult;
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
                    if (famNumber > HCVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.NegativeResult;
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
                    if (famNumber > HIVValue && !string.IsNullOrEmpty(rox) && float.TryParse(rox, out outNumber) && float.Parse(rox) < 36)
                    {
                        return PCRTest.NegativeResult;
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
                    Quantity = data.Quantity;
                    DR = data.Dr;
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
                    && ("No Ct") == Quantity || "Undetermined" == Quantity || Quantity.Trim() == "")
                {
                    return PCRTest.NegativeResult;
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
                    return Quantity + " IU/ml";
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
                        && ("No Ct" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || Quantity.Trim() == "" || "Undetermined" == Quantity))
                    {
                        return PCRTest.NegativeResult;
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
                        && float.Parse(Quantity) < 10000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) >= 10000
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
                        && ("No Ct" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || Quantity.Trim() == "" || "Undetermined" == Quantity))
                    {
                        return PCRTest.NegativeResult;
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
                        && float.Parse(Quantity) < 100000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) >= 100000
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
                        && ("No Ct" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || Quantity.Trim() == "" || "Undetermined" == Quantity))
                    {
                        return PCRTest.NegativeResult;
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
                        && float.Parse(Quantity) < 10000000)
                    {
                        return Quantity;
                    }
                    else if (!string.IsNullOrEmpty(CT)
                        && float.TryParse(CT, out outNumber)
                        && float.Parse(CT) <= 40
                        && !string.IsNullOrEmpty(Quantity)
                        && float.TryParse(Quantity, out outNumber)
                        && float.Parse(Quantity) >= 10000000)
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
                        && ("No Ct" == CT || CT.Trim() == "" || "Undetermined" == CT)
                        && null != Quantity
                        && ("No Ct" == Quantity || Quantity.Trim() == "" || "Undetermined" == Quantity))
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
            if (type_comboBox.SelectedIndex == 0 || type_comboBox.SelectedIndex == 1)
            {
                fileDialog.Filter = "Text Files (*.csv)|*.csv";
            }
            else if(type_comboBox.SelectedIndex == 2)
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
            ComboBoxItem selectedItem = (ComboBoxItem)rotation_comboBox.SelectedItem;
            Guid rotationId = (Guid)selectedItem.Tag;
            barcode_comboBox.Items.Clear();
            List<Plate> plateList = controller.GetPCRPlateBarcode(rotationId, currentExperimentId);
            if (plateList != null && plateList.Count > 0)
            {
                foreach (Plate plate in plateList)
                {
                    barcode_comboBox.Items.Add(new ComboBoxItem() { Content = (string.IsNullOrEmpty(plate.BarCode) ? "("+PlateName.PCRPlate+")":plate.BarCode), DataContext=plate});
                }
            }
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
