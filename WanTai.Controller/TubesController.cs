﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using WanTai.DataModel;
using System.Data;
using WanTai.DataModel.Configuration;
using WanTai.Controller.EVO;
using System.IO;
namespace WanTai.Controller
{
    public class TubesController
    {
        public bool CallScanScript()
        {
            string scanTubeScriptName = WanTai.Common.Configuration.GetTubesScanScriptName();

            try
            {
                IProcessor processor = ProcessorFactory.GetProcessor();
                processor.StartScript(scanTubeScriptName);
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "CallScanScript()", SessionInfo.ExperimentID);                
                return false;
            }

            return true;
        }
        public void SaveScanParameter(int ColumnCount)
        {
            string ScanTubesColumnNumberFileName = WanTai.Common.Configuration.GetEvoVariableOutputPath()+ WanTai.Common.Configuration.GetScanTubesColumnNumberFileName();
            using (StreamWriter writer = new StreamWriter(new FileStream(ScanTubesColumnNumberFileName, FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine(ColumnCount.ToString());
            }
        }
        public DataTable GetTubes(List<LiquidType> LiquidTypeList, DateTime fileBeforeCreatedTime, out string ErrMsg, out string SystemFluid)
        {
            ErrMsg = "success";
            SystemFluid="";
            try
            {
                DataTable Tubes = new DataTable();
                for (int i = 1; i < 37; i++)
                {
                    DataColumn Column = new DataColumn("Position" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("Grid" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("BarCode" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("TubeType" + i.ToString());
                    Column.DefaultValue = Tubetype.IsNull;
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("TubePosBarCode" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("TextItemCount" + i.ToString());
                    Column.DefaultValue = "0,0,#C0C0C0"; //是否扫描到，采血管、阴阳补、选择颜色 #FFC0CB
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("IconName" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("Visibility" + i.ToString());
                    Column.DefaultValue = "Hidden";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("IsEnabled" + i.ToString());
                    Column.DefaultValue = "True";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("IsSelected" + i.ToString());
                    Column.DefaultValue = "#316AC5";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("DetailView" + i.ToString());
                    Column.DefaultValue = "";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("DetailViewTime" + i.ToString());
                    Column.DefaultValue = DateTime.Now;//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);

                    Column = new DataColumn("TestingItem" + i.ToString());
                    Column.DefaultValue = "";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);

                    Column = new DataColumn("Background" + i.ToString());
                    Column.DefaultValue = null;//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                }
                for (int i = 0; i < 16; i++)
                {
                    DataRow dataRow = Tubes.NewRow();
                    Tubes.Rows.Add(dataRow);
                }

                string scanFilePath = WanTai.Common.Configuration.GetEvoOutputPath() + WanTai.Common.Configuration.GetTubeScanResultFileName();
                if (System.IO.File.Exists(scanFilePath))
                {
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(scanFilePath);
                    DateTime fileModifiedTime = fileInfo.LastWriteTime;
                    if (DateTime.Compare(fileBeforeCreatedTime, fileModifiedTime) > 0)
                    {
                        ErrMsg = "System can not find the scan file";
                        throw new Exception("System can not find the scan file");
                    }
                }
                else
                {
                    ErrMsg = "System can not find the scan file";
                    throw new Exception("System can not find the scan file");
                }

                using (System.IO.StreamReader mysr = new System.IO.StreamReader(scanFilePath))
                {
                    string strline;
                    string[] aryline;
                    strline = mysr.ReadLine();
                    //1(Position);1;1(Grid);Tube 13*100mm 16 Pos;Tube1;013/035678;11
                    while ((strline = mysr.ReadLine()) != null)
                    {
                        aryline = strline.Split(new char[] { ';' });
                        if (aryline[6] == "$$$" || aryline[6] == "") continue;
                        int Position = int.Parse(aryline[0]);
                        int Grid = int.Parse(aryline[2]) - 1;
                        Tubes.Rows[Grid]["Position" + Position.ToString()] = Position;
                        Tubes.Rows[Grid]["Grid" + Position.ToString()] = Grid + 1;
                        Tubes.Rows[Grid]["BarCode" + Position.ToString()] = aryline[6];
                        Tubes.Rows[Grid]["TubeType" + Position.ToString()] = Tubetype.Tube;
                        if (!string.IsNullOrEmpty(aryline[5]))
                            Tubes.Rows[Grid]["TubePosBarCode" + Position.ToString()] = aryline[5];
                        Tubes.Rows[Grid]["TextItemCount" + Position.ToString()] = "1,1,#C0C0C0";
                        Tubes.Rows[Grid]["IconName" + Position.ToString()] = "1";
                        Tubes.Rows[Grid]["Visibility" + Position.ToString()] = "Visible";
                        Tubes.Rows[Grid]["IsEnabled" + Position.ToString()] = "True";
                        Tubes.Rows[Grid]["IsSelected" + Position.ToString()] = "#316AC5";
                    }
                }
                StringBuilder builder = new StringBuilder();
               
                StringBuilder SystemFluidName = new StringBuilder();
                using (WanTai.DataModel.WanTaiEntities _WanTaiEntities = new WanTaiEntities())
                {
                    foreach (SystemFluidConfiguration _SystemFluidConfiguration in _WanTaiEntities.SystemFluidConfigurations)
                    {
                        if (Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["TubeType" + _SystemFluidConfiguration.Grid.ToString()].ToString() == "-1")
                            continue;

                        if (Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["TubeType" + _SystemFluidConfiguration.Grid.ToString()].ToString() == "Tube")
                            builder.Append("[" + _SystemFluidConfiguration.Grid.ToString() + "," + _SystemFluidConfiguration.Position.ToString() + "]");
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["TubeType" + _SystemFluidConfiguration.Grid.ToString()] = (Tubetype)_SystemFluidConfiguration.ItemType;
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["Visibility" + _SystemFluidConfiguration.Grid.ToString()] = "Visible";
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["IsEnabled" + _SystemFluidConfiguration.Grid.ToString()] = "True";
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["IsSelected" + _SystemFluidConfiguration.Grid.ToString()] = "#316AC5";
                      //  Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["BarCode" + _SystemFluidConfiguration.Grid.ToString()] = Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["BarCode" + _SystemFluidConfiguration.Grid.ToString()].ToString()+ "[不能分组]";
                        LiquidType _LiquidType = LiquidTypeList.Find(delegate(LiquidType lt) { return (lt.TypeId == _SystemFluidConfiguration.ItemType); });
                        if (_LiquidType != null)
                        {
                            Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["TextItemCount" + _SystemFluidConfiguration.Grid.ToString()] = "1,2," + _LiquidType.Color;
                            SystemFluidName.Append(_LiquidType.TypeName+";");
                        }
                    }
                    Tubes.TableName = builder.ToString();
                }
                string message="";
                foreach (LiquidType _liquidType in LiquidTypeList)
                {
                    if (SystemFluidName.ToString().IndexOf(_liquidType.TypeName + ";") < 0)
                    {
                        message += _liquidType.TypeName + "、";
                        SystemFluid += _liquidType.TypeId + ",";
                    }
                }
                if (message.Length > 0)
                    ErrMsg = "没有" + message.Substring(0, message.Length-1)+"!";

                Tubes.Namespace = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                return Tubes;
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "GetTubes()", SessionInfo.ExperimentID);
                throw;
            }
        }
        public DataTable GetTubes(Guid BatchID, List<LiquidType> LiquidTypeList)
        {
            try
            {
                DataTable Tubes = new DataTable();
                for (int i = 1; i < 37; i++)
                {
                    DataColumn Column = new DataColumn("Position" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("Grid" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("BarCode" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("TubeType" + i.ToString());
                    Column.DefaultValue = Tubetype.IsNull;
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("TubePosBarCode" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("TextItemCount" + i.ToString());
                    Column.DefaultValue = "0,0,#C0C0C0"; //是否扫描到，采血管、阴阳补、选择颜色
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("IconName" + i.ToString());
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("Visibility" + i.ToString());
                    Column.DefaultValue = "Hidden";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("IsEnabled" + i.ToString());
                    Column.DefaultValue = "True";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("IsSelected" + i.ToString());
                    Column.DefaultValue = "#316AC5";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("DetailView" + i.ToString());
                    Column.DefaultValue = "";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                    Column = new DataColumn("DetailViewTime" + i.ToString());
                    Column.DefaultValue = DateTime.Now;//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);

                    Column = new DataColumn("TestingItem" + i.ToString());
                    Column.DefaultValue = "";//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);

                    Column = new DataColumn("Background" + i.ToString());
                    Column.DefaultValue = null;//Collapsed Hidden Visible
                    Tubes.Columns.Add(Column);
                }
                for (int i = 0; i < 16; i++)
                {
                    DataRow dataRow = Tubes.NewRow();
                    Tubes.Rows.Add(dataRow);
                }
                //1(Position);1;1(Grid);Tube 13*100mm 16 Pos;Tube1;013/035678;11
                using (WanTai.DataModel.WanTaiEntities wanTaiEntities = new WanTaiEntities())
                {
                    var TubesGroup = wanTaiEntities.TubeGroups.Where(Batch => Batch.TubesBatchID == BatchID);
                    foreach (TubeGroup tubeGroup in TubesGroup)
                    {
                        var tubes = wanTaiEntities.Tubes.Where(tube => tube.TubeGroupID == tubeGroup.TubeGroupID);
                        foreach (Tube tube in tubes)
                        {
                            int Position = tube.Grid;
                            int Grid = tube.Position - 1;
                            Tubes.Rows[Grid]["Position" + Position.ToString()] = Position;
                            Tubes.Rows[Grid]["Grid" + Position.ToString()] = Grid + 1;
                            Tubes.Rows[Grid]["BarCode" + Position.ToString()] = tube.BarCode;
                            Tubes.Rows[Grid]["TubeType" + Position.ToString()] = Tubetype.Tube;

                            Tubes.Rows[Grid]["TubePosBarCode" + Position.ToString()] = tube.TubePosBarCode;
                            tubeGroup.TestingItemConfigurations.Load();
                            Tubes.Rows[Grid]["TextItemCount" + Position.ToString()] = "1,1,#C0C0C0";

                            Tubes.Rows[Grid]["DetailView" + Position.ToString()] += tubeGroup.TubesGroupName+" " + tubeGroup.PoolingRulesConfiguration.PoolingRulesName;
                            foreach (TestingItemConfiguration TestingItem in tubeGroup.TestingItemConfigurations)
                            {
                                Tubes.Rows[Grid]["TextItemCount" + Position.ToString()] += ",;" + TestingItem.TestingItemColor;
                                Tubes.Rows[Grid]["DetailView" + Position.ToString()] += " " + TestingItem.TestingItemName;
                            }

                            Tubes.Rows[Grid]["DetailView" + Position.ToString()] += ",";
                            Tubes.Rows[Grid]["IconName" + Position.ToString()] = "1";
                            Tubes.Rows[Grid]["Visibility" + Position.ToString()] = "Visible";
                            Tubes.Rows[Grid]["IsEnabled" + Position.ToString()] = "True";
                            Tubes.Rows[Grid]["IsSelected" + Position.ToString()] = "#316AC5";
                        }
                    }
                    foreach (SystemFluidConfiguration _SystemFluidConfiguration in wanTaiEntities.SystemFluidConfigurations)
                    {
                        if (Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["TubeType" + _SystemFluidConfiguration.Grid.ToString()].ToString() == "-1") continue;
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["TubeType" + _SystemFluidConfiguration.Grid.ToString()] = (Tubetype)_SystemFluidConfiguration.ItemType;
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["Visibility" + _SystemFluidConfiguration.Grid.ToString()] = "Visible";
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["IsEnabled" + _SystemFluidConfiguration.Grid.ToString()] = "True";
                        Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["IsSelected" + _SystemFluidConfiguration.Grid.ToString()] = "#316AC5";
                        LiquidType _LiquidType = LiquidTypeList.Find(delegate(LiquidType lt) { return lt.TypeId == _SystemFluidConfiguration.ItemType; });
                        if (_LiquidType != null)
                            Tubes.Rows[(int)(_SystemFluidConfiguration.Position - 1)]["TextItemCount" + _SystemFluidConfiguration.Grid.ToString()] = "1,2," + _LiquidType.Color;
                    }
                }

                Tubes.Namespace = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                return Tubes;
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->" + "GetTubes()", SessionInfo.ExperimentID);
                throw;
            }
        }
        public bool GetNextTurnStep(DateTime fileBeforeCreatedTime,int value)
        {
            string FileName = WanTai.Common.Configuration.GetEvoOutputPath() + WanTai.Common.Configuration.GetNextTurnStepScriptName();
            if (System.IO.File.Exists(FileName))
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(FileName);
                DateTime fileModifiedTime = fileInfo.LastWriteTime;
                if (DateTime.Compare(fileBeforeCreatedTime, fileModifiedTime) > 0)
                {
                    return false;
                }
            }
            else
                return false;

            using (FileStream fileStream = new System.IO.FileStream(FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            {
                using (System.IO.StreamReader mysr = new System.IO.StreamReader(fileStream))
                {

                    string lineValue = mysr.ReadLine();
                    float lineNumber = 0;
                    if (!string.IsNullOrEmpty(lineValue) && float.TryParse(lineValue, out lineNumber) && float.Parse(lineValue) == value)
                        return true;
                }
            }
            return false;
        }
    }
}