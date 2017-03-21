using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using WanTai.DataModel;
using System.Data;
using System.IO;
namespace WanTai.Controller
{
    public class TubesGroupController
    {
        public TubesBatch SaveTubesGroup(Guid ExperimentID, TubesBatch DelTubesBatch, int RotationIndex, IList<TubeGroup> TubeGroupList, DataTable Tubes, out int ErrType, out string ErrMsg)
        {
            //try                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
            {
                ErrMsg = "操作成功！";
                ErrType = 0;
                string workDeskType = WanTai.Common.Configuration.GetWorkDeskType();

                // ExperimentID = new Guid("1C188E0B-F8A7-11E0-A935-0019D147C478");
                using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
                {
                    #region 删除上次操作的数据
                    if (DelTubesBatch.TubesBatchID != new Guid())
                    {
                        TubesBatch tubeBatch = _WanTaiEntities.TubesBatches.Where(_TubeBatch => _TubeBatch.TubesBatchID == DelTubesBatch.TubesBatchID).FirstOrDefault();
                        if (tubeBatch != null)
                        {
                            List<TubeGroup> DelTubeGroups = _WanTaiEntities.TubeGroups.Where(DelTubeGroup => DelTubeGroup.TubesBatchID == (Guid)DelTubesBatch.TubesBatchID).ToList<TubeGroup>();                           
                            foreach (TubeGroup DelTubeGroup in DelTubeGroups)
                            {
                                List<Tube> DelTubes = _WanTaiEntities.Tubes.Where(DelTube => DelTube.TubeGroupID == DelTubeGroup.TubeGroupID).ToList<Tube>();
                                foreach (Tube DelTube in DelTubes)
                                {
                                    DelTube.DWPlatePositions.Clear();
                                    _WanTaiEntities.Tubes.DeleteObject(DelTube);
                                    // _WanTaiEntities.SaveChanges();
                                }
                                DelTubeGroup.TestingItemConfigurations.Clear();
                                // _WanTaiEntities.SaveChanges();
                                _WanTaiEntities.TubeGroups.DeleteObject(DelTubeGroup);
                                // _WanTaiEntities.SaveChanges();
                                //  List<TestingItemConfiguration> DelTestingItemConfigurations=_WanTaiEntities.TestingItemConfigurations.Where(DelTestingItemConfiguration=>DelTestingItemConfiguration.)
                            }
                            List<Plate> DelPlates = _WanTaiEntities.Plates.Where(plate => plate.TubesBatchID == tubeBatch.TubesBatchID).ToList();
                            foreach(Plate plate in DelPlates)
                            {
                                List<PCRPlatePosition> DelPCRPlatePosition = _WanTaiEntities.PCRPlatePositions.Where(Position => Position.PlateID==plate.PlateID).ToList();
                                foreach (PCRPlatePosition position in DelPCRPlatePosition)
                                {
                                    position.DWPlatePositions.Clear();
                                    _WanTaiEntities.PCRPlatePositions.DeleteObject(position);
                                }
                                List<DWPlatePosition> DelDWPlatePosition = _WanTaiEntities.DWPlatePositions.Where(Position => Position.PlateID == plate.PlateID).ToList();
                                foreach (DWPlatePosition position in DelDWPlatePosition)
                                {
                                    position.Tubes.Clear();
                                    position.PCRPlatePositions.Clear();
                                    _WanTaiEntities.DWPlatePositions.DeleteObject(position);
                                }
                                _WanTaiEntities.Plates.DeleteObject(plate);
                            }

                            _WanTaiEntities.TubesBatches.DeleteObject(tubeBatch);
                           //_WanTaiEntities.SaveChanges();
                        }
                    }
                    #endregion
                    // _WanTaiEntities.SaveChanges();
                    // update testing Item for del tubesbatch
                    if (!String.IsNullOrEmpty(SessionInfo.BatchType) && int.Parse(SessionInfo.BatchType) > 1)
                    {
                        foreach(KeyValuePair<Guid, int> _TestingItem in SessionInfo.BatchTestingItem) {
                            if (_TestingItem.Value > 0)
                            {
                                if (DelTubesBatch.TestingItem.ContainsKey(_TestingItem.Key))
                                {
                                    DelTubesBatch.TestingItem[_TestingItem.Key] += _TestingItem.Value;
                                }
                                else
                                {
                                    DelTubesBatch.TestingItem.Add(_TestingItem.Key, _TestingItem.Value);
                                }
                            }
                        }
                    }

                    #region 保存TubesBatch
                    TubesBatch _TubesBatch = new TubesBatch();
                    _TubesBatch.ExperimentID = ExperimentID;
                    _TubesBatch.CreateTime = DateTime.Now;
                    _TubesBatch.TestingItem = DelTubesBatch.TestingItem;
                    _TubesBatch.TubesBatchID = WanTaiObjectService.NewSequentialGuid();
                    _TubesBatch.TubesBatchName = "批次" + RotationIndex.ToString();
                    _WanTaiEntities.AddToTubesBatches(_TubesBatch);
                    #endregion
                    // _WanTaiEntities.SaveChanges();
                    RotationInfo ExperimentRotation = _WanTaiEntities.RotationInfoes.Where(Rotation => Rotation.ExperimentID == ExperimentID && Rotation.TubesBatchID == null).FirstOrDefault();
                    if (ExperimentRotation != null && ExperimentRotation.RotationSequence == 1 
                        && String.IsNullOrEmpty(SessionInfo.BatchType) && int.Parse(SessionInfo.BatchType) == SessionInfo.BatchTimes)
                    {
                        ExperimentRotation = null;
                    }
                    
                    string CSVPath = WanTai.Common.Configuration.GetWorkListFilePath();
                    string DWFileName = CSVPath + WanTai.Common.Configuration.GetAddSamplesWorkListFileName();
                    
                    //string PCRFileName = CSVPath + "PCR" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                    ///todo: each test item should has a seperate file name, the file is in TestingItemConfiguration.
                    int PoolCountInTotal = 0;
                    int PoolingWorkListRowCount = 0;
                    #region 开始保存文件
                    using (FileStream DWFile = new FileStream(DWFileName, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter DWStreamWriter = new StreamWriter(DWFile, Encoding.Default))
                        {
                            // 统一变量声明
                            Plate DWPlate_1 = null;
                            Plate DWPlate_2 = null;
                            TubeGroup npTubeGroup = null;
                            List<TubeGroup> npTubeGroupList = new List<TubeGroup>();
                            int Total100NPItem = 0;
                            int TotalTestingItem = 0;

                            #region 保存 Plates信息  96孔板 1\2 PCR配液板
                            DWStreamWriter.WriteLine("Source Labware Label,Source Position,Volume,Destination Labware Label,Destination Position");
                            //PCRStreamWriter.WriteLine("Source Labware Label,Source Position,Volume,Destination Labware Label,Destination Position");
                            ///DW板和PCR板
                            // 150 和 200用两块DW板，100的只用一块
                            // BatchType为B时不需要新增96孔板，沿用之前的
                            DWPlate_1 = new Plate();
                            DWPlate_1.ExperimentID = ExperimentID;
                            DWPlate_1.PlateID = WanTaiObjectService.NewSequentialGuid();
                            DWPlate_1.PlateName = PlateName.DWPlate1;
                            DWPlate_1.TubesBatchID = _TubesBatch.TubesBatchID;
                            DWPlate_1.PlateType = (short)PlateType.Mix_Plate;
                            _WanTaiEntities.AddToPlates(DWPlate_1);

                            if (workDeskType != "100")
                            {
                                DWPlate_2 = new Plate();
                                DWPlate_2.ExperimentID = ExperimentID;
                                DWPlate_2.PlateID = WanTaiObjectService.NewSequentialGuid();
                                DWPlate_2.PlateName = PlateName.DWPlate2;
                                DWPlate_2.PlateType = (short)PlateType.Mix_Plate;
                                DWPlate_2.TubesBatchID = _TubesBatch.TubesBatchID;
                                _WanTaiEntities.AddToPlates(DWPlate_2);
                            }

                            #endregion 
                            
                            #region 阴阳对应物 TubeGroup信息(单检)
                            if (workDeskType != "100")
                            {
                                // 创建新的阴阳对照物分组
                                npTubeGroup = new TubeGroup();
                                npTubeGroup.CreateTime = DateTime.Now;
                                npTubeGroup.ExperimentID = _TubesBatch.ExperimentID;
                                npTubeGroup.TubeGroupID = WanTaiObjectService.NewSequentialGuid();

                                npTubeGroup.PoolingRulesID = _WanTaiEntities.PoolingRulesConfigurations.Where(PoolingRules => (PoolingRules.TubeNumber == 1)).FirstOrDefault().PoolingRulesID;

                                npTubeGroup.isComplement = false;
                                if (SessionInfo.BatchTimes > 1)
                                    npTubeGroup.BatchType = "1";

                                foreach (KeyValuePair<Guid, int> _TestingItem in DelTubesBatch.TestingItem)
                                {
                                    if (_TestingItem.Value == 0) continue;

                                    TotalTestingItem += _TestingItem.Value + 2;
                                    npTubeGroup.TestingItemConfigurations.Add(_WanTaiEntities.TestingItemConfigurations.Where(TestingItem => TestingItem.TestingItemID == _TestingItem.Key).FirstOrDefault());
                                }
                                //if (DelTubesBatch.HBVNumber > 0)
                                //    NewTubeGroupNull.TestingItemConfigurations.Add(_WanTaiEntities.TestingItemConfigurations.Where(TestingItem => TestingItem.TestingItemName == "HBV").FirstOrDefault());
                                //if (DelTubesBatch.HCVNumber > 0)
                                //    NewTubeGroupNull.TestingItemConfigurations.Add(_WanTaiEntities.TestingItemConfigurations.Where(TestingItem => TestingItem.TestingItemName == "HCV").FirstOrDefault());
                                //if (DelTubesBatch.HIVNumber > 0)
                                //    NewTubeGroupNull.TestingItemConfigurations.Add(_WanTaiEntities.TestingItemConfigurations.Where(TestingItem => TestingItem.TestingItemName == "HIV").FirstOrDefault());
                                npTubeGroup.TubesBatchID = _TubesBatch.TubesBatchID;
                                _WanTaiEntities.AddToTubeGroups(npTubeGroup);
                            }
                            else
                            {
                                // 100的台面每个检测项都需要建立阴阳对照物分组（阴阳参照物+?个定量参考品）
                                int npGroupStartIndex = 1;
                                int npGroupEndIndex = SessionInfo.LiquidCfgCount;
                                foreach (Guid testingItemID in SessionInfo.TestingItemIDs)
                                {
                                    if (DelTubesBatch.TestingItem.ContainsKey(testingItemID) && (DelTubesBatch.TestingItem[testingItemID] > 0))
                                    {
                                        npTubeGroup = new TubeGroup();
                                        npTubeGroup.CreateTime = DateTime.Now;
                                        npTubeGroup.ExperimentID = _TubesBatch.ExperimentID;
                                        npTubeGroup.PoolingRulesID = _WanTaiEntities.PoolingRulesConfigurations.Where(PoolingRules => (PoolingRules.TubeNumber == 1)).FirstOrDefault().PoolingRulesID;
                                        npTubeGroup.isComplement = false;
                                        npTubeGroup.TestingItemConfigurations.Add(_WanTaiEntities.TestingItemConfigurations.Where(TestingItem => TestingItem.TestingItemID == testingItemID).FirstOrDefault());
                                        npTubeGroup.TubeGroupID = WanTaiObjectService.NewSequentialGuid();
                                        npTubeGroup.TubesBatchID = _TubesBatch.TubesBatchID;
                                        npTubeGroup.TubesGroupName = npGroupStartIndex + "," + npGroupEndIndex;
                                        npTubeGroupList.Add(npTubeGroup);
                                        TotalTestingItem += SessionInfo.LiquidCfgCount;
                                        Total100NPItem += SessionInfo.LiquidCfgCount;
                                        TotalTestingItem += DelTubesBatch.TestingItem[testingItemID];
                                        _WanTaiEntities.AddToTubeGroups(npTubeGroup);
                                    }
                                    npGroupStartIndex += SessionInfo.LiquidCfgCount;
                                    npGroupEndIndex += SessionInfo.LiquidCfgCount;
                                }
                            }

                            #endregion
                           
                            string[] PCRCSV = new string[TotalTestingItem]; //PCR文件

                            //  string[] PCRCSV = new string[DelTubesBatch.HBVNumber + (DelTubesBatch.HBVNumber > 0 ? 2 : 0) + DelTubesBatch.HCVNumber + (DelTubesBatch.HCVNumber > 0 ? 2 : 0) + DelTubesBatch.HIVNumber + (DelTubesBatch.HIVNumber > 0 ? 2 : 0)];
                            //  Tubes.Select();
                            //阴、阳对照物   0血管、1补液、2阳性、3阴性  PositiveControl = 2, NegativeControl = 3
                            //SystemFluidConfiguration NegativeControl = _WanTaiEntities.SystemFluidConfigurations.Where(systemFluid => systemFluid.ItemType == 3).FirstOrDefault();
                            //SystemFluidConfiguration PositiveControl = _WanTaiEntities.SystemFluidConfigurations.Where(systemFluid => systemFluid.ItemType == 2).FirstOrDefault();
                            DWPlatePosition DWPlate_NegativeControl_1 = null;
                            DWPlatePosition DWPlate_NegativeControl_2 = null;
                            DWPlatePosition DWPlate_PositiveControl_1 = null;
                            DWPlatePosition DWPlate_PositiveControl_2 = null;
                            Dictionary<Guid, List<DWPlatePosition>> DWPlatePosition100 = new Dictionary<Guid, List<DWPlatePosition>>();

                            string[] NegativePositive = null;

                            if (workDeskType != "100")
                            {
                                NegativePositive = new string[4];
                            }
                            else
                            {
                                // 100的有多组阴阳对照物，每组6个管子，但只加样到1块DW板中
                                NegativePositive = new string[Total100NPItem];
                            }

                            // combine batch a tubes
                            List<DataTable> TubesArray = new List<DataTable>();
                            TubesArray.Add(Tubes);
                            if (!String.IsNullOrEmpty(SessionInfo.BatchType) && int.Parse(SessionInfo.BatchType) > 1)
                            {
                                for (int i = 0; i < int.Parse(SessionInfo.BatchType) - 1; i ++ )
                                    TubesArray.Add(SessionInfo.BatchTubeList[i]);
                            }

                            foreach (DataTable _tubes in TubesArray)
                            {
                                if (_tubes == null) continue;
                                foreach (string str in _tubes.TableName.Split(']'))
                                {
                                    if (string.IsNullOrEmpty(str)) continue;
                                    bool ignore100NpTube = true;
                                    int DW100PlatePostion = 0;
                                    Guid tubeTestingItemID = new Guid();
                                    string TubesPosition = str.Remove(0, 1);
                                    int ColumnIndex = int.Parse(TubesPosition.Split(',')[0]);
                                    int RowIndex = int.Parse(TubesPosition.Split(',')[1]) - 1;
                                    Tube tube = new Tube();
                                    tube.BarCode = _tubes.Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString();
                                    tube.ExperimentID = ExperimentID;
                                    tube.Position = RowIndex + 1;
                                    tube.Grid = ColumnIndex;
                                    int posIndex = (ColumnIndex - 1) * 16 + RowIndex + 1;
                                    if (workDeskType != "100")
                                    {
                                        tube.TubeGroupID = npTubeGroup.TubeGroupID;
                                    }
                                    else
                                    {
                                        int _tubeGroupIndex = 0;
                                        foreach (TubeGroup _tubeGroup in npTubeGroupList)
                                        {
                                            int npGroupStartIndex = int.Parse(_tubeGroup.TubesGroupName.Split(',')[0]);
                                            int npGroupEndIndex = int.Parse(_tubeGroup.TubesGroupName.Split(',')[1]);
                                            if (posIndex >= npGroupStartIndex && posIndex <= npGroupEndIndex)
                                            {
                                                tube.TubeGroupID = _tubeGroup.TubeGroupID;
                                                tubeTestingItemID = _tubeGroup.TestingItemConfigurations.FirstOrDefault().TestingItemID;
                                                ignore100NpTube = false;
                                                DW100PlatePostion = _tubeGroupIndex * SessionInfo.LiquidCfgCount + (posIndex - npGroupStartIndex) + 1;
                                                break;
                                            }
                                            _tubeGroupIndex++;
                                        }
                                    }
                                    tube.TubeID = WanTaiObjectService.NewSequentialGuid();
                                    tube.TubePosBarCode = _tubes.Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString();
                                    if (_tubes.Rows[RowIndex]["TubeType" + ColumnIndex.ToString()].ToString() == "Complement")
                                        tube.TubeType = (int)Tubetype.Complement;
                                    if (_tubes.Rows[RowIndex]["TubeType" + ColumnIndex.ToString()].ToString() == "NegativeControl")
                                        tube.TubeType = (int)Tubetype.NegativeControl;
                                    if (_tubes.Rows[RowIndex]["TubeType" + ColumnIndex.ToString()].ToString() == "PositiveControl")
                                        tube.TubeType = (int)Tubetype.PositiveControl;
                                    if (_tubes.Rows[RowIndex]["TubeType" + ColumnIndex.ToString()].ToString() == "Tube")
                                        tube.TubeType = (int)Tubetype.Tube;
                                    // 100的加500到一个板子，其它的分别加480到两个板子
                                    tube.Volume = workDeskType == "100" ? 500 : 480;

                                    if (tube.TubeType == (int)Tubetype.PositiveControl || tube.TubeType == (int)Tubetype.NegativeControl
                                        || (workDeskType == "100" && tube.TubeType == (int)Tubetype.Complement))
                                    {
                                        if (workDeskType != "100")
                                        {
                                            _WanTaiEntities.AddToTubes(tube);
                                            DWPlatePosition DWPositionA = new DWPlatePosition();
                                            DWPositionA.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            DWPositionA.TubeGroupID = tube.TubeGroupID;
                                            DWPositionA.Position = tube.TubeType == (int)Tubetype.PositiveControl ? 2 : 1;
                                            DWPositionA.PlateID = DWPlate_1.PlateID;
                                            DWPositionA.Tubes.Add(tube);
                                            // DWPlate_NegativeControl_1.Tubes.Add(_WanTaiEntities.Tubes.Where(t => t.TubeID == tube.TubeID).FirstOrDefault());
                                            _WanTaiEntities.AddToDWPlatePositions(DWPositionA);

                                            DWPlatePosition DWPositionB = new DWPlatePosition();
                                            DWPositionB.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            DWPositionB.TubeGroupID = tube.TubeGroupID;
                                            DWPositionB.Position = tube.TubeType == (int)Tubetype.PositiveControl ? 2 : 1;
                                            DWPositionB.PlateID = DWPlate_2.PlateID;
                                            DWPositionB.Tubes.Add(tube);
                                            // DWPlate_NegativeControl_1.Tubes.Add(_WanTaiEntities.Tubes.Where(t => t.TubeID == tube.TubeID).FirstOrDefault());
                                            _WanTaiEntities.AddToDWPlatePositions(DWPositionB);
                                            if (tube.TubeType == (int)Tubetype.PositiveControl)
                                            {
                                                DWPlate_PositiveControl_1 = DWPositionA;
                                                DWPlate_PositiveControl_2 = DWPositionB;
                                                NegativePositive[1] = "Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + ",480," + PlateName.DWPlate1 + ",2";
                                                NegativePositive[3] = "Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + ",480," + PlateName.DWPlate2 + ",2";
                                                //DWStreamWriter.WriteLine(tube.BarCode + "," + (RowIndex + 1).ToString() + ",480,"+PlateName.DWPlate1+",2");
                                                //DWStreamWriter.WriteLine(tube.BarCode + "," + (RowIndex + 1).ToString() + ",480,"+PlateName.DWPlate2+",2");
                                            }

                                            if (tube.TubeType == (int)Tubetype.NegativeControl)
                                            {
                                                DWPlate_NegativeControl_1 = DWPositionA;
                                                DWPlate_NegativeControl_2 = DWPositionB;
                                                NegativePositive[0] = "Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + ",480," + PlateName.DWPlate1 + ",1";
                                                NegativePositive[2] = "Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + ",480," + PlateName.DWPlate2 + ",1";
                                                //DWStreamWriter.WriteLine(tube.BarCode + "," + (RowIndex + 1).ToString() + ",480,"+PlateName.DWPlate1+",1");
                                                //DWStreamWriter.WriteLine(tube.BarCode + "," + (RowIndex + 1).ToString() + ",480,"+PlateName.DWPlate2+",1");

                                            }
                                            PoolingWorkListRowCount += 2;
                                        }
                                        else
                                        {
                                            if (!ignore100NpTube)
                                            {
                                                _WanTaiEntities.AddToTubes(tube);
                                                DWPlatePosition DWPosition = new DWPlatePosition();
                                                DWPosition.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                                DWPosition.TubeGroupID = tube.TubeGroupID;
                                                DWPosition.Position = DW100PlatePostion; ;
                                                DWPosition.PlateID = DWPlate_1.PlateID;
                                                DWPosition.Tubes.Add(tube);
                                                _WanTaiEntities.AddToDWPlatePositions(DWPosition);


                                                if (null != tubeTestingItemID)
                                                {
                                                    if (!DWPlatePosition100.ContainsKey(tubeTestingItemID))
                                                    {
                                                        List<DWPlatePosition> platePositionList = new List<DWPlatePosition>();
                                                        platePositionList.Add(DWPosition);
                                                        DWPlatePosition100.Add(tubeTestingItemID, platePositionList);
                                                    }
                                                    else
                                                    {
                                                        DWPlatePosition100[tubeTestingItemID].Add(DWPosition);
                                                    }
                                                }
                                                NegativePositive[DWPosition.Position - 1] = "Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + ",500," + PlateName.DWPlate1 + "," + DWPosition.Position;

                                                PoolingWorkListRowCount += 1;
                                            }
                                        }
                                        // HolePosition++;

                                    }// NewTubeGroup.Tubes.Add(tube);
                                }
                            }                            

                            //
                            if (String.IsNullOrEmpty(SessionInfo.BatchType) || int.Parse(SessionInfo.BatchType) == 1)
                            {
                                foreach (string str in NegativePositive){
                                    if (!string.IsNullOrEmpty(str))
                                        DWStreamWriter.WriteLine(str);
                                }

                            }

                            #endregion 

                            //_WanTaiEntities.SaveChanges();
                            #region 保存阴阳对照物PCR Worklist(2011-11-22)
                            // 150 A轮不保存PCR worklist，B轮统一保存 ????
                            // 100的每个检测项目包括?项
                            int npNumber = workDeskType != "100" ? 2 : SessionInfo.LiquidCfgCount;
                            Dictionary<string, int> PoolCountOfTestItem = new Dictionary<string, int>();
                            List<TestingItemConfiguration> TestingItemList = _WanTaiEntities.TestingItemConfigurations.OrderBy(TestintItem => TestintItem.DisplaySequence).ToList();
                            int PCRPosition = 1;
                            int LastTestingItemCount = 0;
                            Plate PCRPlate = null;
                            int PCRPosition_tmp = 1;
                            FormulaParameters formulaParameters = new FormulaParameters();

                            foreach (TestingItemConfiguration TestingItem in TestingItemList)
                            {
                                if (!DelTubesBatch.TestingItem.ContainsKey(TestingItem.TestingItemID) || DelTubesBatch.TestingItem[TestingItem.TestingItemID] == 0)
                                {
                                    PoolCountOfTestItem.Add(TestingItem.TestingItemName, 0);
                                    continue;
                                }
                                if (PCRPlate != null)
                                    TestingItem.PlateID = PCRPlate.PlateID;
                                if ((LastTestingItemCount + DelTubesBatch.TestingItem[TestingItem.TestingItemID] + npNumber) > 96 || PCRPlate == null)
                                {
                                    PCRPlate = new Plate();
                                    PCRPlate.ExperimentID = ExperimentID;
                                    PCRPlate.PlateID = WanTaiObjectService.NewSequentialGuid();
                                    PCRPlate.TubesBatchID = _TubesBatch.TubesBatchID;
                                    PCRPlate.PlateName = PlateName.PCRPlate;
                                    PCRPlate.PlateType = (short)PlateType.PCR_Plate;
                                    _WanTaiEntities.AddToPlates(PCRPlate);
                                    TestingItem.PlateID = PCRPlate.PlateID;
                                    LastTestingItemCount = 0;
                                    PCRPosition = 1;
                                    formulaParameters.PCRPlatesCount += 1;
                                }

                                if (workDeskType != "100")
                                {
                                    LastTestingItemCount += DelTubesBatch.TestingItem[TestingItem.TestingItemID] + npNumber;
                                    PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                                    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                                    _PCRPlatePosition.TestName = TestingItem.TestingItemName;
                                    _PCRPlatePosition.Position = PCRPosition;
                                    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_1);
                                    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_2);
                                    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                                    _PCRPlatePosition = new PCRPlatePosition();
                                    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                                    _PCRPlatePosition.TestName = TestingItem.TestingItemName;
                                    _PCRPlatePosition.Position = PCRPosition + 1;
                                    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_PositiveControl_1);
                                    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_PositiveControl_2);
                                    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);


                                    PCRCSV[PCRPosition_tmp - 1] = PlateName.DWPlate5 + ",1,20," + PlateName.PCRPlate + "," + PCRPosition.ToString();
                                    PCRCSV[PCRPosition_tmp] = PlateName.DWPlate5 + ",2,20," + PlateName.PCRPlate + "," + (PCRPosition + 1).ToString();
                                }
                                else
                                {
                                    if (DWPlatePosition100.ContainsKey(TestingItem.TestingItemID))
                                    {
                                        int posIndex = PCRPosition;
                                        int pcrIndex = PCRPosition_tmp - 1;
                                        foreach (DWPlatePosition pos in DWPlatePosition100[TestingItem.TestingItemID])
                                        {
                                            PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                                            _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                                            _PCRPlatePosition.TestName = TestingItem.TestingItemName;
                                            _PCRPlatePosition.Position = posIndex;
                                            _PCRPlatePosition.DWPlatePositions.Add(pos);
                                            _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);
                                            PCRCSV[pcrIndex] = PlateName.DWPlate5 + "," + pos.Position.ToString() + ",20," + PlateName.PCRPlate + "," + posIndex.ToString();
                                            pcrIndex++;
                                            posIndex ++;
                                        }
                                    }
                                }

                                TestingItem.TestingItemPCR = PCRPosition_tmp + npNumber;
                                TestingItem.TestingItemPosition = PCRPosition + npNumber;
                                PCRPosition += DelTubesBatch.TestingItem[TestingItem.TestingItemID] + npNumber;
                                PCRPosition_tmp += DelTubesBatch.TestingItem[TestingItem.TestingItemID] + npNumber;

                                formulaParameters.PCRWorkListRowCount += DelTubesBatch.TestingItem[TestingItem.TestingItemID] + npNumber;
                                PoolCountOfTestItem.Add(TestingItem.TestingItemName, DelTubesBatch.TestingItem[TestingItem.TestingItemID]);
                            }
                            //DataModel.FormulaParameters.PCRWorkListRowCount = PCRPosition - 1;
                            formulaParameters.PoolCountOfTestItem = PoolCountOfTestItem;
                            foreach (string strKey in formulaParameters.PoolCountOfTestItem.Keys)
                            {
                                if (formulaParameters.PoolCountOfTestItem[strKey] > 0)
                                    formulaParameters.TestItemCountInTotal++;
                            }

                            #endregion

                            #region 废除的代码
                            //foreach (KeyValuePair<Guid, int> _TestingItem in DelTubesBatch.TestingItem)
                            //{
                            //    TotalTestingItem += _TestingItem.Value + 2;
                            //    NewTubeGroupNull.TestingItemConfigurations.Add(_WanTaiEntities.TestingItemConfigurations.Where(TestingItem => TestingItem.TestingItemID == _TestingItem.Key).FirstOrDefault());
                            //}
                           

                            //int HBVPosition = 2;
                            //int HCVPosition = 2;
                            //int HIVPosition = 2;
                            //if (DelTubesBatch.HBVNumber > 0)
                            //{
                            //    PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                            //    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                            //    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                            //    _PCRPlatePosition.TestName = "HBV";
                            //    _PCRPlatePosition.Position = 1;
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_2);
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                            //    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                            //    _PCRPlatePosition = new PCRPlatePosition();
                            //    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                            //    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                            //    _PCRPlatePosition.TestName = "HBV";
                            //    _PCRPlatePosition.Position = 2;
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_2);
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                            //    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);


                            //    PCRCSV[0] = PlateName.DWPlate5+",1,20,"+PlateName.PCRPlate+",1";
                            //    PCRCSV[1] = PlateName.DWPlate5+",2,20,"+PlateName.PCRPlate+",2";
                            //}
                            //if (DelTubesBatch.HCVNumber > 0)
                            //{
                            //    if (DelTubesBatch.HBVNumber > 0)
                            //        HCVPosition = 2 + DelTubesBatch.HBVNumber + 2;


                            //    PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                            //    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                            //    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                            //    _PCRPlatePosition.TestName = "HCV";
                            //    _PCRPlatePosition.Position = (HCVPosition - 1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_2);
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                            //    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                            //    _PCRPlatePosition = new PCRPlatePosition();
                            //    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                            //    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                            //    _PCRPlatePosition.TestName = "HCV";
                            //    _PCRPlatePosition.Position = HCVPosition;
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_2);
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                            //    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                            //    PCRCSV[HCVPosition - 2] = PlateName.DWPlate5+",1,20,"+PlateName.PCRPlate+"," + (HCVPosition - 1).ToString();
                            //    PCRCSV[HCVPosition - 1] = PlateName.DWPlate5+",2,20,"+PlateName.PCRPlate+"," + HCVPosition.ToString();
                            //}
                            //if (DelTubesBatch.HIVNumber > 0)
                            //{
                            //    if (DelTubesBatch.HBVNumber > 0)
                            //        HIVPosition = 2 + DelTubesBatch.HBVNumber + 2;
                            //    if (DelTubesBatch.HCVNumber > 0)
                            //        HIVPosition = 2 + DelTubesBatch.HCVNumber + 2;

                            //    PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                            //    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                            //    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                            //    _PCRPlatePosition.TestName = "HIV";
                            //    _PCRPlatePosition.Position = (HIVPosition - 1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_2);
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                            //    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                            //    _PCRPlatePosition = new PCRPlatePosition();
                            //    _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                            //    _PCRPlatePosition.PlateID = PCRPlate.PlateID;
                            //    _PCRPlatePosition.TestName = "HIV";
                            //    _PCRPlatePosition.Position = HIVPosition;
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_1);
                            //    _PCRPlatePosition.DWPlatePositions.Add(DWPlate_NegativeControl_2);
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                            //    //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                            //    _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);
                            //    PCRCSV[HIVPosition - 2] = PlateName.DWPlate5+",1,20,"+PlateName.PCRPlate+"," + (HIVPosition - 1).ToString();
                            //    PCRCSV[HIVPosition - 1] = PlateName.DWPlate5+",2,20,"+PlateName.PCRPlate+"," + HIVPosition.ToString();
                            //}
                            #endregion

                            int HolePosition = workDeskType != "100" ? 2 : Total100NPItem; //96孔板起始位置

                            List<SystemFluidConfiguration> SystemFluid = null;
                            int SystemFluidIndex = 0;

                          
                            //_WanTaiEntities.SaveChanges();
                            if (SessionInfo.BatchTimes > 1) {
                                TubeGroupList = SessionInfo.BatchTubeGroups.Concat(TubeGroupList).ToList<TubeGroup>();
                                if (int.Parse(SessionInfo.BatchType) < SessionInfo.BatchTimes)
                                {
                                    SessionInfo.BatchTubeGroups = (List<TubeGroup>)TubeGroupList;    
                                }
                            }
                            foreach (TubeGroup _TubeGroup in TubeGroupList)
                            {
                                DWPlatePosition DWPlate_1_Position = null;
                                DWPlatePosition DWPlate_2_Position = null;

                                PoolingRulesConfiguration PollingRules = _WanTaiEntities.PoolingRulesConfigurations.Where(PollingRule => PollingRule.PoolingRulesID == _TubeGroup.PoolingRulesID).FirstOrDefault();
                                float Volume = workDeskType != "100" ? 960 / PollingRules.TubeNumber : 500;

                                #region  新建TubeGroup
                                TubeGroup NewTubeGroup = new TubeGroup();
                                NewTubeGroup.CreateTime = DateTime.Now;
                                NewTubeGroup.ExperimentID = _TubesBatch.ExperimentID;
                                NewTubeGroup.PoolingRulesID = _TubeGroup.PoolingRulesID;
                                NewTubeGroup.isComplement = _TubeGroup.isComplement;
                                NewTubeGroup.PoolingRulesName = _TubeGroup.PoolingRulesName;
                                NewTubeGroup.RowIndex = _TubeGroup.RowIndex;
                                NewTubeGroup.TestintItemName = _TubeGroup.TestintItemName;
                                NewTubeGroup.TubesPosition = _TubeGroup.TubesPosition; // 是否需要转换成19-36列?
                                NewTubeGroup.TubesGroupName = _TubeGroup.TubesGroupName;
                                NewTubeGroup.TubesNumber = _TubeGroup.TubesNumber;
                                NewTubeGroup.BatchType = _TubeGroup.BatchType;
                                foreach (TestingItemConfiguration _TestingItemConfiguration in _TubeGroup.TestingItemConfigurations)
                                {
                                    NewTubeGroup.TestingItemConfigurations.Add(_WanTaiEntities.TestingItemConfigurations.Where(p => p.TestingItemID == _TestingItemConfiguration.TestingItemID).FirstOrDefault());
                                }
                                NewTubeGroup.TubeGroupID = WanTaiObjectService.NewSequentialGuid();
                                NewTubeGroup.TubesBatchID = _TubesBatch.TubesBatchID;
                                _WanTaiEntities.AddToTubeGroups(NewTubeGroup);


                                #endregion


                                if (String.IsNullOrEmpty(SessionInfo.BatchType)) {
                                    SystemFluid = _WanTaiEntities.SystemFluidConfigurations.ToList().Where(systemFluidConfiguration => (int)systemFluidConfiguration.ItemType == 1 && systemFluidConfiguration.BatchType == "1").ToList();
                                } else {    
                                    SystemFluid = _WanTaiEntities.SystemFluidConfigurations.ToList().Where(systemFluidConfiguration => (int)systemFluidConfiguration.ItemType == 1 && systemFluidConfiguration.BatchType == SessionInfo.BatchType).ToList();
                                }

                                string[] TubesPosition = NewTubeGroup.TubesPosition.Split(']');
                                if (PollingRules.TubeNumber == 1)
                                {
                                    #region 单检
                                    Volume = workDeskType != "100" ? 480 : 500;
                                    StringBuilder ONETubesPosition2 = new StringBuilder();
                                    foreach (string str in TubesPosition)
                                    {
                                        #region 保存tube信息
                                        if (string.IsNullOrEmpty(str)) continue;
                                        string TubePosition = str.Remove(0, 1);
                                        int ColumnIndex = int.Parse(TubePosition.Split(',')[0]);
                                        int RowIndex = int.Parse(TubePosition.Split(',')[1]) - 1;
                                        Tube tube = new Tube();
                                        tube.BarCode = (!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType))
                                            ? SessionInfo.BatchTubeList[int.Parse(NewTubeGroup.BatchType) - 1].Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString() : Tubes.Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString();
                                        tube.ExperimentID = ExperimentID;
                                        tube.Position = RowIndex + 1;
                                        tube.Grid = ColumnIndex;
                                        tube.TubeGroupID = NewTubeGroup.TubeGroupID;
                                        tube.TubeID = WanTaiObjectService.NewSequentialGuid();
                                        tube.TubePosBarCode = (!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType))
                                            ? SessionInfo.BatchTubeList[int.Parse(NewTubeGroup.BatchType) - 1].Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString() : Tubes.Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString();
                                        tube.TubeType = (int)Tubetype.Tube;
                                        tube.Volume = Volume;
                                        #endregion

                                        DWPlate_1_Position = new DWPlatePosition();
                                        DWPlate_1_Position.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                        DWPlate_1_Position.TubeGroupID = NewTubeGroup.TubeGroupID;
                                        DWPlate_1_Position.Position = HolePosition + 1;
                                        DWPlate_1_Position.PlateID = DWPlate_1.PlateID;
                                        if (workDeskType != "100")
                                        {
                                            DWPlate_2_Position = new DWPlatePosition();
                                            DWPlate_2_Position.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            DWPlate_2_Position.TubeGroupID = NewTubeGroup.TubeGroupID;
                                            DWPlate_2_Position.Position = HolePosition + 1;
                                            DWPlate_2_Position.PlateID = DWPlate_2.PlateID;
                                        }

                                        //PCR文件
                                        foreach (TestingItemConfiguration TestionItem in TestingItemList)
                                        {
                                            if (NewTubeGroup.TestingItemConfigurations.Where(_TestionItem => _TestionItem.TestingItemID == TestionItem.TestingItemID).Count() == 0)
                                                continue;

                                            PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                                            _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            _PCRPlatePosition.PlateID = TestionItem.PlateID;// PCRPlate.PlateID;
                                            _PCRPlatePosition.TestName = TestionItem.TestingItemName;
                                            _PCRPlatePosition.Position = TestionItem.TestingItemPosition;
                                            _PCRPlatePosition.DWPlatePositions.Add(DWPlate_1_Position);
                                            if (workDeskType != "100")
                                                _PCRPlatePosition.DWPlatePositions.Add(DWPlate_2_Position);
                                            _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                                            PCRCSV[TestionItem.TestingItemPCR - 1] = PlateName.DWPlate5 + "," + (HolePosition + 1).ToString() + ",20," + PlateName.PCRPlate + "," + TestionItem.TestingItemPosition.ToString();
                                            TestionItem.TestingItemPosition += 1;
                                            TestionItem.TestingItemPCR += 1;
                                        }

                                        DWPlate_1_Position.Tubes.Add(tube);
                                        _WanTaiEntities.AddToDWPlatePositions(DWPlate_1_Position);

                                        if (workDeskType != "100")
                                        {
                                            DWPlate_2_Position.Tubes.Add(tube);
                                            _WanTaiEntities.AddToDWPlatePositions(DWPlate_2_Position);
                                        }

                                        if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                        {
                                            DWStreamWriter.WriteLine("Tube" +  tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + 1).ToString());
                                            if (workDeskType != "100")
                                            {
                                                if (ONETubesPosition2.Length > 0)
                                                    ONETubesPosition2.Append(System.Environment.NewLine + "Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                else
                                                    ONETubesPosition2.Append("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                            }
                                        }

                                        HolePosition++;
                                        PoolingWorkListRowCount++;
                                        PoolingWorkListRowCount++;
                                    }
                                    if (workDeskType != "100" && !(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                    {
                                        DWStreamWriter.Write(ONETubesPosition2.ToString() + System.Environment.NewLine);
                                    }
                                }
                                else
                                {
                                    int BalanceTubesCount = NewTubeGroup.TubesNumber;//剩余
                                    int TubesPositionIndex = 0; //
                                    int demand = PollingRules.TubeNumber  * 8;//需求量  48
                                    int demandIndex = 0;///需求量index
                                    StringBuilder ONETubesPosition2 = new StringBuilder();
                                    int tubeCount = 0;
                                    List<DWPlatePosition> DWPlate_1_PositionList = new List<DWPlatePosition>();
                                    List<DWPlatePosition> DWPlate_2_PositionList = new List<DWPlatePosition>();
                                    #region 按8个一组
                                    while (BalanceTubesCount  >= demand)
                                    {
                                        string str =  TubesPosition[TubesPositionIndex];
                                        #region 保存tube信息
                                        if (string.IsNullOrEmpty(str)) continue;
                                        string TubePosition = str.Remove(0, 1);
                                        int ColumnIndex = int.Parse(TubePosition.Split(',')[0]);
                                        int RowIndex = int.Parse(TubePosition.Split(',')[1]) - 1;
                                        Tube tube = new Tube();
                                        tube.BarCode = (!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType))
                                            ? SessionInfo.BatchTubeList[int.Parse(NewTubeGroup.BatchType) - 1].Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString() : Tubes.Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString();
                                        tube.ExperimentID = ExperimentID;
                                        tube.Position = RowIndex + 1;
                                        tube.Grid = ColumnIndex;
                                        tube.TubeGroupID = NewTubeGroup.TubeGroupID;
                                        tube.TubeID = WanTaiObjectService.NewSequentialGuid();
                                        tube.TubePosBarCode = (!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType))
                                            ? SessionInfo.BatchTubeList[int.Parse(NewTubeGroup.BatchType) - 1].Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString() : Tubes.Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString();
                                        tube.TubeType = (int)Tubetype.Tube;
                                        tube.Volume = Volume;
                                        #endregion
                                        if (demand > PollingRules.TubeNumber / 2 * 8)
                                        {
                                            if (DWPlate_1_PositionList.Count < 8)
                                            {
                                                DWPlate_1_Position = new DWPlatePosition();
                                                DWPlate_1_Position.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                                DWPlate_1_Position.TubeGroupID = NewTubeGroup.TubeGroupID;
                                                DWPlate_1_Position.Position = HolePosition + 1 + demandIndex;
                                                DWPlate_1_Position.PlateID = DWPlate_1.PlateID;
                                                DWPlate_1_Position.Tubes.Add(tube);
                                                _WanTaiEntities.AddToDWPlatePositions(DWPlate_1_Position);
                                                DWPlate_1_PositionList.Add(DWPlate_1_Position);
                                            }else
                                                DWPlate_1_PositionList[demandIndex].Tubes.Add(tube);
                                            if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + demandIndex + 1).ToString());
                                        }
                                        else
                                        {
                                            if (DWPlate_2_PositionList.Count < 8)
                                            {
                                                DWPlate_2_Position = new DWPlatePosition();
                                                DWPlate_2_Position.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                                DWPlate_2_Position.TubeGroupID = NewTubeGroup.TubeGroupID;
                                                DWPlate_2_Position.Position = HolePosition + 1 + demandIndex;
                                                DWPlate_2_Position.PlateID = DWPlate_2.PlateID;

                                                DWPlate_2_Position.Tubes.Add(tube);
                                                _WanTaiEntities.AddToDWPlatePositions(DWPlate_2_Position);
                                                DWPlate_2_PositionList.Add(DWPlate_2_Position);
                                            }
                                            else
                                                DWPlate_2_PositionList[demandIndex].Tubes.Add(tube);
                                            if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + demandIndex + 1).ToString());
                                        }
                                        PoolingWorkListRowCount++;
                                        if (demand <= 8 && (tubeCount++) < 8)
                                        {
                                            //PCR文件
                                            foreach (TestingItemConfiguration TestionItem in TestingItemList)
                                            {
                                                if (NewTubeGroup.TestingItemConfigurations.Where(_TestionItem => _TestionItem.TestingItemID == TestionItem.TestingItemID).Count() == 0)
                                                    continue;
                                                //if (!DelTubesBatch.TestingItem.ContainsKey(TestionItem.TestingItemID)) continue;
                                                //if (DelTubesBatch.TestingItem[TestionItem.TestingItemID] == 0) continue;
                                                PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                                                _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                                _PCRPlatePosition.PlateID = TestionItem.PlateID;// PCRPlate.PlateID;
                                                _PCRPlatePosition.TestName = TestionItem.TestingItemName;
                                                _PCRPlatePosition.Position = TestionItem.TestingItemPosition;
                                                _PCRPlatePosition.DWPlatePositions.Add(DWPlate_1_PositionList[tubeCount-1]);
                                                _PCRPlatePosition.DWPlatePositions.Add(DWPlate_2_PositionList[tubeCount-1]);
                                                //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                                                //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                                                _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                                                PCRCSV[TestionItem.TestingItemPCR - 1] = PlateName.DWPlate5 + "," + (HolePosition + demandIndex + 1).ToString() + ",20," + PlateName.PCRPlate + "," + TestionItem.TestingItemPosition.ToString();
                                                TestionItem.TestingItemPosition += 1;
                                                TestionItem.TestingItemPCR += 1;
                                            }
                                        }
                                      
                                        TubesPositionIndex++;
                                        /*参数下调*/
                                        BalanceTubesCount--;
                                        demand--;
                                        demandIndex ++ ;
                                        if (demandIndex == 8)
                                            demandIndex = 0;
                                        if (demand == 0)
                                        {
                                            DWPlate_1_PositionList = new List<DWPlatePosition>();
                                            DWPlate_2_PositionList = new List<DWPlatePosition>();
                                            tubeCount = 0;
                                            demand = PollingRules.TubeNumber * 8;
                                            HolePosition = HolePosition + 8 ;
                                        }
                                    }
                                    #endregion 
                                    int GroupNumber = BalanceTubesCount / PollingRules.TubeNumber;
                                    int GroupResidue = BalanceTubesCount % PollingRules.TubeNumber;
                                    int TubesIndex = 0;
                                    int CTubesIndex = 0;
                                    int Hole1TubesNumber = PollingRules.TubeNumber / 2;
                                    int Hole2TubesNumber = PollingRules.TubeNumber / 2;
                                    #region  原worklist生成规则
                                    // foreach (string str in _TubeGroup.TubesPosition.Split(']'))
                                    while (TubesPosition.Length > TubesPositionIndex)
                                    {
                                        string str = TubesPosition[TubesPositionIndex++];
                                        #region 保存tube信息
                                        if (string.IsNullOrEmpty(str)) continue;
                                        string TubePosition = str.Remove(0, 1);
                                        int ColumnIndex = int.Parse(TubePosition.Split(',')[0]);
                                        int RowIndex = int.Parse(TubePosition.Split(',')[1]) - 1;
                                        Tube tube = new Tube();
                                        // tube.BarCode = Tubes.Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString();
                                        tube.BarCode = (!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType))
                                             ? SessionInfo.BatchTubeList[int.Parse(NewTubeGroup.BatchType) - 1].Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString() : Tubes.Rows[RowIndex]["BarCode" + ColumnIndex.ToString()].ToString();
                                        tube.ExperimentID = ExperimentID;
                                        tube.Position = RowIndex + 1;
                                        tube.Grid = ColumnIndex;
                                        tube.TubeGroupID = NewTubeGroup.TubeGroupID;
                                        tube.TubeID = WanTaiObjectService.NewSequentialGuid();
                                        // tube.TubePosBarCode = Tubes.Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString();
                                        tube.TubePosBarCode = (!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType))
                                             ? SessionInfo.BatchTubeList[int.Parse(NewTubeGroup.BatchType) - 1].Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString() : Tubes.Rows[RowIndex]["TubePosBarCode" + ColumnIndex.ToString()].ToString();

                                        tube.TubeType = (int)Tubetype.Tube;
                                        tube.Volume = Volume;
                                        #endregion
                                        #region 开始先保存PCR
                                        if (CTubesIndex == 0)
                                        {
                                            DWPlate_1_Position = new DWPlatePosition();
                                            DWPlate_1_Position.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            DWPlate_1_Position.TubeGroupID = NewTubeGroup.TubeGroupID;
                                            DWPlate_1_Position.Position = HolePosition + 1;
                                            DWPlate_1_Position.PlateID = DWPlate_1.PlateID;

                                            DWPlate_2_Position = new DWPlatePosition();
                                            DWPlate_2_Position.DWPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            DWPlate_2_Position.TubeGroupID = NewTubeGroup.TubeGroupID;
                                            DWPlate_2_Position.Position = HolePosition + 1;
                                            DWPlate_2_Position.PlateID = DWPlate_2.PlateID;

                                            //PCR文件
                                            foreach (TestingItemConfiguration TestionItem in TestingItemList)
                                            {
                                                if (NewTubeGroup.TestingItemConfigurations.Where(_TestionItem => _TestionItem.TestingItemID == TestionItem.TestingItemID).Count() == 0)
                                                    continue;
                                                //if (!DelTubesBatch.TestingItem.ContainsKey(TestionItem.TestingItemID)) continue;
                                                //if (DelTubesBatch.TestingItem[TestionItem.TestingItemID] == 0) continue;
                                                PCRPlatePosition _PCRPlatePosition = new PCRPlatePosition();
                                                _PCRPlatePosition.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                                _PCRPlatePosition.PlateID = TestionItem.PlateID;// PCRPlate.PlateID;
                                                _PCRPlatePosition.TestName = TestionItem.TestingItemName;
                                                _PCRPlatePosition.Position = TestionItem.TestingItemPosition;
                                                _PCRPlatePosition.DWPlatePositions.Add(DWPlate_1_Position);
                                                _PCRPlatePosition.DWPlatePositions.Add(DWPlate_2_Position);
                                                //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_1.DWPlatePositionID).FirstOrDefault());
                                                //_PCRPlatePosition.DWPlatePositions.Add(_WanTaiEntities.DWPlatePositions.Where(d => d.DWPlatePositionID == DWPlate_NegativeControl_2.DWPlatePositionID).FirstOrDefault());
                                                _WanTaiEntities.AddToPCRPlatePositions(_PCRPlatePosition);

                                                PCRCSV[TestionItem.TestingItemPCR - 1] = PlateName.DWPlate5 + "," + (HolePosition + 1).ToString() + ",20," + PlateName.PCRPlate + "," + TestionItem.TestingItemPosition.ToString();
                                                TestionItem.TestingItemPosition += 1;
                                                TestionItem.TestingItemPCR += 1;
                                            }
                                            #region 废除的代码
                                            //foreach (TestingItemConfiguration _TestingItemConfiguration in _TubeGroup.TestingItemConfigurations)
                                            //{
                                            //    if (_TestingItemConfiguration.TestingItemName == "HBV")
                                            //    {
                                            //        PCRPositionHBV = new PCRPlatePosition();
                                            //        PCRPositionHBV.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            //        PCRPositionHBV.PlateID = PCRPlate.PlateID;
                                            //        PCRPositionHBV.TestName = _TestingItemConfiguration.TestingItemName;
                                            //        PCRPositionHBV.Position = HBVPosition;
                                            //        PCRCSV[HBVPosition++] = PlateName.DWPlate5+"," + (HolePosition).ToString() + ",20,"+PlateName.PCRPlate+"," + HBVPosition;
                                            //    }
                                            //    if (_TestingItemConfiguration.TestingItemName == "HCV")
                                            //    {
                                            //        PCRPositionHCV = new PCRPlatePosition();
                                            //        PCRPositionHCV.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            //        PCRPositionHCV.PlateID = PCRPlate.PlateID;
                                            //        PCRPositionHCV.TestName = _TestingItemConfiguration.TestingItemName;
                                            //        PCRPositionHCV.Position = HCVPosition;
                                            //        PCRCSV[HCVPosition++] = PlateName.DWPlate5+"," + (HolePosition).ToString() + ",20,"+PlateName.PCRPlate+"," + HCVPosition;
                                            //    }
                                            //    if (_TestingItemConfiguration.TestingItemName == "HIV")
                                            //    {
                                            //        PCRPositionHIV = new PCRPlatePosition();
                                            //        PCRPositionHIV.PCRPlatePositionID = WanTaiObjectService.NewSequentialGuid();
                                            //        PCRPositionHIV.PlateID = PCRPlate.PlateID;
                                            //        PCRPositionHIV.TestName = _TestingItemConfiguration.TestingItemName;
                                            //        PCRPositionHIV.Position = HIVPosition;
                                            //        PCRCSV[HIVPosition++] = PlateName.DWPlate5+"," + (HolePosition).ToString() + ",20,"+PlateName.PCRPlate+"," + HIVPosition;
                                            //    }
                                            //}
                                            #endregion
                                        }
                                        #endregion
                                        #region 没有剩余 或 还没到最后余数
                                        if (GroupResidue == 0 || GroupNumber > TubesIndex) ///无剩余，或没到最后
                                        {
                                            Volume = 960 / PollingRules.TubeNumber;
                                            Hole1TubesNumber = PollingRules.TubeNumber / 2;
                                            Hole2TubesNumber = PollingRules.TubeNumber / 2;
                                            CTubesIndex++;
                                            #region 单检
                                            if (PollingRules.TubeNumber == 1)
                                            {
                                                Volume = 480;
                                                DWPlate_1_Position.Tubes.Add(tube);
                                                DWPlate_2_Position.Tubes.Add(tube);
                                                //DWPlate_1_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t=>t.TubeID==tube.TubeID).FirstOrDefault());
                                                //DWPlate_2_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t => t.TubeID == tube.TubeID).FirstOrDefault());
                                                _WanTaiEntities.AddToDWPlatePositions(DWPlate_1_Position);
                                                _WanTaiEntities.AddToDWPlatePositions(DWPlate_2_Position);

                                                //_WanTaiEntities.SaveChanges();

                                                if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                {
                                                    DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + 1).ToString());
                                                    DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                }

                                                HolePosition++;
                                                CTubesIndex = 0;
                                                TubesIndex++;
                                                PoolingWorkListRowCount++;
                                                PoolingWorkListRowCount++;
                                            }
                                            #endregion
                                            else
                                            {
                                                if (CTubesIndex <= Hole1TubesNumber)
                                                {
                                                    DWPlate_1_Position.Tubes.Add(tube);
                                                    //DWPlate_1_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t=>t.TubeID==tube.TubeID).FirstOrDefault());
                                                    if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                        DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + 1).ToString());
                                                    PoolingWorkListRowCount++;
                                                }
                                                else
                                                {
                                                    _WanTaiEntities.AddToDWPlatePositions(DWPlate_1_Position);
                                                    DWPlate_2_Position.Tubes.Add(tube);

                                                    if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                        DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                    PoolingWorkListRowCount++;
                                                }
                                                if (CTubesIndex == PollingRules.TubeNumber)
                                                {
                                                    _WanTaiEntities.AddToDWPlatePositions(DWPlate_2_Position);
                                                    CTubesIndex = 0;
                                                    HolePosition++;
                                                    TubesIndex++;
                                                }
                                                //_WanTaiEntities.SaveChanges();
                                            }
                                            //_WanTaiEntities.SaveChanges();
                                        }
                                        #endregion
                                        else ///最后一批
                                        {
                                            CTubesIndex++;
                                            #region  最后只有一个血管
                                            if (GroupResidue == 1)
                                            {
                                                Volume = 480;
                                                DWPlate_1_Position.Tubes.Add(tube);
                                                _WanTaiEntities.AddToDWPlatePositions(DWPlate_1_Position);
                                                if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                    DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + 1).ToString());
                                                if (NewTubeGroup.isComplement) //补液
                                                {
                                                    if (SystemFluid.Count == 0)
                                                    {
                                                        ErrMsg = "没有样品补充！";
                                                        ErrType = -1;

                                                        DWStreamWriter.Close();
                                                        //PCRStreamWriter.Close();
                                                        DWFile.Close();
                                                        DWFile.Dispose();
                                                        //PCRFile.Close();
                                                        //PCRFile.Dispose();
                                                        File.Delete(DWFileName);
                                                        //File.Delete(PCRFileName);
                                                        return DelTubesBatch;

                                                    }
                                                    Volume = 480;
                                                    while (SystemFluid[SystemFluidIndex].Volume < Volume)
                                                    {
                                                        SystemFluidIndex++;
                                                        //if (NewTubeGroup.BatchType == "B")
                                                        //    SystemFluidIndexB = SystemFluidIndex;
                                                        //else
                                                        //    SystemFluidIndexA = SystemFluidIndex;

                                                        if (SystemFluidIndex >= SystemFluid.Count)
                                                        {
                                                            ErrType = -1;
                                                            ErrMsg = "样品补充液不够！";
                                                            DWStreamWriter.Close();
                                                            //PCRStreamWriter.Close();
                                                            DWFile.Close();
                                                            DWFile.Dispose();
                                                            //PCRFile.Close();
                                                            //PCRFile.Dispose();
                                                            File.Delete(DWFileName);
                                                            //File.Delete(PCRFileName);
                                                            return DelTubesBatch;
                                                        }
                                                    }
                                                    if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                        DWStreamWriter.WriteLine("Tube" + SystemFluid[SystemFluidIndex] + "," + SystemFluid[SystemFluidIndex].Position.ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                    SystemFluid[SystemFluidIndex].Volume -= Volume;
                                                    PoolingWorkListRowCount++;
                                                }
                                                else
                                                {
                                                    DWPlate_2_Position.Tubes.Add(tube);
                                                    //DWPlate_1_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t=>t.TubeID==tube.TubeID).FirstOrDefault());
                                                    //DWPlate_2_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t => t.TubeID == tube.TubeID).FirstOrDefault());
                                                    _WanTaiEntities.AddToDWPlatePositions(DWPlate_2_Position);
                                                    if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                        DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                }
                                                HolePosition++;
                                                CTubesIndex = 0;
                                                TubesIndex++;
                                                PoolingWorkListRowCount++;
                                                PoolingWorkListRowCount++;
                                            }
                                            #endregion
                                            #region 最后大于一个血管
                                            else
                                            {
                                                if (GroupResidue <= (PollingRules.TubeNumber / 2))
                                                {
                                                    Hole1TubesNumber = GroupResidue / 2;
                                                    if (GroupResidue % 2 > 0)
                                                        Hole1TubesNumber++;
                                                    Hole2TubesNumber = GroupResidue - Hole1TubesNumber;
                                                }
                                                else
                                                {
                                                    Hole1TubesNumber = PollingRules.TubeNumber / 2;
                                                    Hole2TubesNumber = GroupResidue - Hole1TubesNumber;
                                                }
                                                if (NewTubeGroup.isComplement) //补液
                                                {
                                                    if (SystemFluid.Count == 0)
                                                    {
                                                        ErrMsg = "没有样品补充！";
                                                        ErrType = -1;
                                                        DWStreamWriter.Close();
                                                        //PCRStreamWriter.Close();
                                                        DWFile.Close();
                                                        DWFile.Dispose();
                                                        //PCRFile.Close();
                                                        //PCRFile.Dispose();
                                                        File.Delete(DWFileName);
                                                        //File.Delete(PCRFileName);
                                                        return DelTubesBatch;
                                                    }
                                                    #region 补液
                                                    if (CTubesIndex <= Hole1TubesNumber)
                                                    {
                                                        DWPlate_1_Position.Tubes.Add(tube);if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                        //DWPlate_1_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t=>t.TubeID==tube.TubeID).FirstOrDefault());
                                                        Volume = 960 / PollingRules.TubeNumber;
                                                        if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                            DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + 1).ToString());
                                                        PoolingWorkListRowCount++;
                                                    }
                                                    else
                                                    {
                                                        _WanTaiEntities.AddToDWPlatePositions(DWPlate_1_Position);
                                                        DWPlate_2_Position.Tubes.Add(tube);
                                                        // DWPlate_2_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t => t.TubeID == tube.TubeID).FirstOrDefault());

                                                        Volume = 960 / PollingRules.TubeNumber;
                                                        if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                            DWStreamWriter.WriteLine("Tube" + tube.Grid  + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                        PoolingWorkListRowCount++;
                                                    }
                                                    //_WanTaiEntities.SaveChanges();
                                                    #region 补液
                                                    //第一块板需要补液
                                                    if (CTubesIndex == Hole1TubesNumber && Hole1TubesNumber != PollingRules.TubeNumber / 2)
                                                    {
                                                        Volume = (960 / PollingRules.TubeNumber) * (PollingRules.TubeNumber / 2 - Hole1TubesNumber);
                                                        while (SystemFluid[SystemFluidIndex].Volume < Volume)
                                                        {
                                                            SystemFluidIndex++;
                                                            //if (NewTubeGroup.BatchType == "B")
                                                            //    SystemFluidIndexB = SystemFluidIndex;
                                                            //else
                                                            //    SystemFluidIndexA = SystemFluidIndex;

                                                            if (SystemFluidIndex >= SystemFluid.Count)
                                                            {
                                                                ErrType = -1;
                                                                ErrMsg = "样品补充液不够！";
                                                                DWStreamWriter.Close();
                                                                // PCRStreamWriter.Close();
                                                                DWFile.Close();
                                                                DWFile.Dispose();
                                                                //PCRFile.Close();
                                                                //PCRFile.Dispose();
                                                                File.Delete(DWFileName);
                                                                // File.Delete(PCRFileName);
                                                                return DelTubesBatch;
                                                            }
                                                        }
                                                        if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                            DWStreamWriter.WriteLine("Tube" + SystemFluid[SystemFluidIndex].Grid + "," + SystemFluid[SystemFluidIndex].Position.ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + 1).ToString());
                                                        SystemFluid[SystemFluidIndex].Volume -= Volume;
                                                        PoolingWorkListRowCount++;
                                                    }
                                                    //第二块板需要补液
                                                    if (CTubesIndex == (Hole1TubesNumber + Hole2TubesNumber) && Hole2TubesNumber != PollingRules.TubeNumber / 2)
                                                    {
                                                        Volume = (960 / PollingRules.TubeNumber) * (PollingRules.TubeNumber / 2 - Hole2TubesNumber);
                                                        while (SystemFluid[SystemFluidIndex].Volume < Volume)
                                                        {
                                                            SystemFluidIndex++;
                                                            SystemFluidIndex++;
                                                            //if (NewTubeGroup.BatchType == "B")
                                                            //    SystemFluidIndexB = SystemFluidIndex;
                                                            //else
                                                            //    SystemFluidIndexA = SystemFluidIndex;

                                                            if (SystemFluidIndex >= SystemFluid.Count)
                                                            {
                                                                ErrType = -1;
                                                                ErrMsg = "样品补充液不够！";
                                                                DWStreamWriter.Close();
                                                                //PCRStreamWriter.Close();
                                                                DWFile.Close();
                                                                DWFile.Dispose();
                                                                // PCRFile.Close();
                                                                //PCRFile.Dispose();
                                                                File.Delete(DWFileName);
                                                                // File.Delete(PCRFileName);
                                                                return DelTubesBatch;
                                                            }
                                                        }
                                                        if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                            DWStreamWriter.WriteLine("Tube" + SystemFluid[SystemFluidIndex].Grid + "," + SystemFluid[SystemFluidIndex].Position.ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                        PoolingWorkListRowCount++;
                                                        SystemFluid[SystemFluidIndex].Volume -= Volume;

                                                    }
                                                    if (CTubesIndex == (Hole1TubesNumber + Hole2TubesNumber))
                                                    {
                                                        _WanTaiEntities.AddToDWPlatePositions(DWPlate_2_Position);
                                                        CTubesIndex = 0;
                                                        HolePosition++;
                                                        TubesIndex++;
                                                    }
                                                    #endregion 补液结束
                                                    #endregion 补液结束
                                                }
                                                else
                                                {
                                                    #region 不补液
                                                    if (CTubesIndex <= Hole1TubesNumber)
                                                    {
                                                        // DWPlate_1_Position.Tubes.Add(_WanTaiEntities.Tubes.Where(t=>t.TubeID==tube.TubeID).FirstOrDefault());
                                                        DWPlate_1_Position.Tubes.Add(tube);
                                                        Volume = (int)(480 / Hole1TubesNumber);
                                                        if (Volume > 480) Volume = 480;
                                                        if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                            DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate1 + "," + (HolePosition + 1).ToString());
                                                        PoolingWorkListRowCount++;
                                                    }
                                                    else
                                                    {
                                                        _WanTaiEntities.AddToDWPlatePositions(DWPlate_1_Position);
                                                        DWPlate_2_Position.Tubes.Add(tube);
                                                        Volume = (int)(480 / Hole2TubesNumber);
                                                        if (Volume > 480) Volume = 480;
                                                        if (!(!String.IsNullOrEmpty(SessionInfo.BatchType) && !String.IsNullOrEmpty(NewTubeGroup.BatchType) && int.Parse(SessionInfo.BatchType) > int.Parse(NewTubeGroup.BatchType)))
                                                            DWStreamWriter.WriteLine("Tube" + tube.Grid + "," + (RowIndex + 1).ToString() + "," + Volume.ToString() + "," + PlateName.DWPlate2 + "," + (HolePosition + 1).ToString());
                                                        PoolingWorkListRowCount++;
                                                    }

                                                    #region 最后
                                                    if (CTubesIndex == (Hole1TubesNumber + Hole2TubesNumber))
                                                    {
                                                        _WanTaiEntities.AddToDWPlatePositions(DWPlate_2_Position);

                                                        CTubesIndex = 0;
                                                        HolePosition++;
                                                        TubesIndex++;
                                                    }
                                                    #endregion

                                                    #endregion 不补液结束
                                                }
                                            }
                                            #endregion
                                        }
                                        tube.Volume = Volume;
                                        //_WanTaiEntities.SaveChanges();
                                        // _WanTaiEntities.Tubes.Where(t=>t.TubeID==tube.TubeID).f;
                                        // NewTubeGroup.Tubes.Add(tube);
                                    }
                                    #endregion
                                }
                                //NewTubeGroup.EntityKey = new EntityKey() { EntityContainerName = "TubeGroups",
                                //                                           EntitySetName = "TubeGroups",
                                //                                           EntityKeyValues = new EntityKeyMember[] { new EntityKeyMember() { Key = "TubeGroups", Value = NewTubeGroup.TubeGroupID } }
                                //};
                            }
                            //foreach (String str in PCRCSV)
                            //    PCRStreamWriter.WriteLine(str);
                            DWStreamWriter.Close();
                           // PCRStreamWriter.Close();

                            PoolCountInTotal = HolePosition;
                            formulaParameters.PoolCountInTotal = PoolCountInTotal;
                            formulaParameters.PoolingWorkListRowCount = PoolingWorkListRowCount++;
                            if (!SessionInfo.RotationFormulaParameters.ContainsKey(Guid.Empty))
                            {
                                SessionInfo.RotationFormulaParameters.Add(Guid.Empty, formulaParameters);
                            }
                            else 
                            {
                                SessionInfo.RotationFormulaParameters[Guid.Empty] = formulaParameters;
                            }

                            string SampleNumberFileName = WanTai.Common.Configuration.GetEvoVariableOutputPath()+(ExperimentRotation == null ? "" : ExperimentRotation.RotationID.ToString()) + WanTai.Common.Configuration.GetSampleNumberFileName();
                            using (StreamWriter writer = new StreamWriter(new FileStream(SampleNumberFileName, FileMode.Create, FileAccess.Write)))
                            {
                                writer.WriteLine("SAMPLE_NUM_EX");
                                writer.WriteLine(PoolCountInTotal);
                            }
                            string MixSampleNumberFileName = WanTai.Common.Configuration.GetEvoVariableOutputPath() + (ExperimentRotation == null ? "" : ExperimentRotation.RotationID.ToString()) + WanTai.Common.Configuration.GetMixSampleNumberFileName();
                            using (StreamWriter writer = new StreamWriter(new FileStream(MixSampleNumberFileName, FileMode.Create, FileAccess.Write)))
                            {
                                int index = 0;
                                foreach (TestingItemConfiguration TestionItem in TestingItemList)
                                {
                                    //if (File.Exists(CSVPath + TestionItem.WorkListFileName))
                                    //    File.Delete(CSVPath + TestionItem.WorkListFileName);
                                    int testItemCount = 0;
                                    //if (!DelTubesBatch.TestingItem.ContainsKey(TestionItem.TestingItemID)) continue;
                                    //if (DelTubesBatch.TestingItem[TestionItem.TestingItemID] == 0) continue;
                                    if (DelTubesBatch.TestingItem.ContainsKey(TestionItem.TestingItemID))
                                        testItemCount=(DelTubesBatch.TestingItem[TestionItem.TestingItemID] + npNumber);
                                    writer.WriteLine("MIXSAMPLE_" + TestionItem.TestingItemName + "_NUM" + "," + testItemCount.ToString());
                                    if (!DelTubesBatch.TestingItem.ContainsKey(TestionItem.TestingItemID)) continue;
                                    if (DelTubesBatch.TestingItem[TestionItem.TestingItemID] == 0) continue;
                                    if (string.IsNullOrEmpty(TestionItem.WorkListFileName)) continue;

                                    using (StreamWriter PCR = new StreamWriter(new FileStream(CSVPath + (ExperimentRotation == null ? "" : ExperimentRotation.RotationID.ToString()) + TestionItem.WorkListFileName, FileMode.Create, FileAccess.Write)))
                                    {
                                        PCR.WriteLine("Source Labware Label,Source Position,Volume,Destination Labware Label,Destination Position");
                                        for (int TestionCount=0; TestionCount < DelTubesBatch.TestingItem[TestionItem.TestingItemID] + npNumber; TestionCount++, index++)
                                            PCR.WriteLine(PCRCSV[index]);
                                        PCR.Close();
                                    }
                                }
                                writer.Close();
                            }
                        }
                    }
                    #endregion 开始保存文件
                    if (String.IsNullOrEmpty(SessionInfo.BatchType) || int.Parse(SessionInfo.BatchType) == SessionInfo.BatchTimes)
                        _WanTaiEntities.SaveChanges();

                    
                  //  DataModel.FormulaParameters.PCRWorkListRowCount = DelTubesBatch.HBVNumber + (DelTubesBatch.HBVNumber > 0 ? 2 : 0) + DelTubesBatch.HCVNumber + (DelTubesBatch.HCVNumber > 0 ? 2 : 0) + DelTubesBatch.HIVNumber + (DelTubesBatch.HIVNumber > 0 ? 2 : 0);
                    return _TubesBatch;
                }
            }
                /*
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "SaveTubesGroup()", SessionInfo.ExperimentID);
                throw;
            }*/
        }
    }
}