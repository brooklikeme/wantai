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
using System.Data.Entity;
using WanTai.DataModel;
using System.IO;
using System.Windows.Markup;
using System.Data;
using WanTai.Controller;
using WanTai.DataModel.Configuration;
using System.Threading;
using System.ComponentModel;
namespace WanTai.View
{  
    /// <summary>
    /// Interaction logic for TubesView.xaml
    /// </summary>
    public partial class TubesView200 : UserControl
    {
        public TubesView200()
        {
            InitializeComponent();
            if (SessionInfo.PraperRotation != null)
                labelRotationName.Content = SessionInfo.PraperRotation.RotationName;
        }

        private bool IsPoack = false;
        private void AddColumns()
        {
            if (SessionInfo.BatchType != "B")
            {
                SessionInfo.BatchIndex++;
            }
            DataGridTemplateColumn _DataGridTemplateColumn = new DataGridTemplateColumn() { Header = "检测项目", Width = new DataGridLength(100, DataGridLengthUnitType.Star) };
            var TestItems = new TestItemController().GetActiveTestItemConfigurations();
            FrameworkElementFactory _StackPanel = new FrameworkElementFactory(typeof(StackPanel));
            _StackPanel.SetValue(StackPanel.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
            _StackPanel.SetValue(StackPanel.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            _StackPanel.SetValue(StackPanel.OrientationProperty,Orientation.Horizontal );
            foreach (TestingItemConfiguration _TestingItem in TestItems)
            {
                FrameworkElementFactory checkBox = new FrameworkElementFactory(typeof(CheckBox));
                checkBox.SetValue(CheckBox.DataContextProperty, _TestingItem);
                checkBox.SetValue(CheckBox.MarginProperty, new System.Windows.Thickness(5, 0, 5, 0));
                checkBox.SetValue(CheckBox.IsCheckedProperty, false);
                checkBox.SetValue(CheckBox.ContentProperty, _TestingItem.TestingItemName);

                checkBox.AddHandler(CheckBox.CheckedEvent, new RoutedEventHandler(checkBox_Checked));
                checkBox.AddHandler(CheckBox.UncheckedEvent, new RoutedEventHandler(checkBox_Unchecked));
                _StackPanel.AppendChild(checkBox);

                TextBlock textBlock = new TextBlock();
                textBlock.Height = 16;
                textBlock.Margin = new Thickness(5,0,0,0);
                textBlock.Width = 16;
                textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                textBlock.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_TestingItem.TestingItemColor));
                sp_pointout.Children.Add(textBlock);

                Label label = new Label();
                label.Content = _TestingItem.TestingItemName;
                label.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                label.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                label.Width = 65;
                sp_pointout.Children.Add(label);
            }
            DataTemplate dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = _StackPanel;
            _DataGridTemplateColumn.CellTemplate = dataTemplate;
            dg_TubesGroup.Columns.Insert(2, _DataGridTemplateColumn);

            PoolingRules = new WanTai.Controller.PoolingRulesConfigurationController().GetPoolingRulesConfiguration();
            List<LiquidType> LiquidTypeList = SessionInfo.LiquidTypeList = WanTai.Common.Configuration.GetLiquidTypes();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (LiquidType _LiquidType in LiquidTypeList)
            {
                if (_LiquidType.TypeId == 1)
                {
                    txt_Complement.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_LiquidType.Color));
                    lab_Complement.Content = _LiquidType.TypeName;
                }
                if (_LiquidType.TypeId == 2)
                {
                    txt_PositiveControl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_LiquidType.Color));
                    lab_PositiveControl.Content = _LiquidType.TypeName;
                }
                if (_LiquidType.TypeId == 3)
                {
                    txt_NegativeControl.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(_LiquidType.Color));
                    lab_NegativeControl.Content = _LiquidType.TypeName;
                }
                stringBuilder.Append(_LiquidType.TypeName + "、");
            }
            if (stringBuilder.ToString().Length > 0)
                lab_tip.Content = "(" + stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 1) + "不能分组！)";
          //  dg_TubesGroup.ItemsSource = (TubeGroupList=new List<TubeGroup>());
      //<TextBlock Height="20" Margin="5,0,0,0"  Width="20" Grid.Row="1" Name="txt_NegativeControl" HorizontalAlignment="Left"  Background="LightGray" VerticalAlignment="Center" />
      //      <Label Content="阴性对照物" Name="lab_NegativeControl" Grid.Row="1" HorizontalAlignment="Left"   VerticalAlignment="Center" />
           // StringBuilder SBuilder = new StringBuilder();
            //SBuilder.Append("<DataGridTemplateColumn ");
            //SBuilder.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
            //SBuilder.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
            //SBuilder.Append("xmlns:local='clr-namespace:WanTai.View;assembly=WanTai.View' ");
            //SBuilder.Append("Header=\"混样模式\" Width=\"120\" >");
            //SBuilder.Append("<DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append("<DataTemplate >");
            //SBuilder.Append(" <ComboBox Name=\"cb_PoolingRulesConfigurations\" HorizontalAlignment=\"Stretch\" SelectedIndex=\"0\" SelectedValue=\"{Binding PoolingRulesID}\" SelectedValuePath=\"PoolingRulesID\" DisplayMemberPath=\"PoolingRulesName\"  ItemsSource=\"{Binding Source={StaticResource PoolingRulesConfigurations }}\" VerticalAlignment=\"Stretch\"  />");
            //SBuilder.Append("</DataTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn>");

            //SBuilder.Append("<DataGridTemplateColumn ");
            //SBuilder.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
            //SBuilder.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
            //SBuilder.Append("xmlns:local='clr-namespace:WanTai.View;assembly=WanTai.View' ");
            //SBuilder.Append("Header=\"检测项目\" Width=\"200\" >");
            //SBuilder.Append("<DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append("<DataTemplate >");
            //SBuilder.Append("<StackPanel HorizontalAlignment=\"Center\"  VerticalAlignment=\"Center\"  >"); 

            //SBuilder.Append("</StackPanel>");
            //SBuilder.Append("</DataTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn>");

            //Stream stream = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(SBuilder.ToString()));
            //DataGridTemplateColumn colIcon = XamlReader.Load(stream) as DataGridTemplateColumn;
            //this.dg_TubesGroup.Columns.Insert(1, colIcon);
            //SBuilder.Append("<DataGridTemplateColumn ");
            //SBuilder.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
            //SBuilder.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
            //SBuilder.Append("xmlns:local='clr-namespace:WanTai.View;assembly=WanTai.View' ");
            //SBuilder.Append("Header=\"操作\" Width=\"100\">");
            //SBuilder.Append(" <DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append("<DataTemplate >");
            //SBuilder.Append("<StackPanel Orientation=\"Horizontal\"  HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" >");
            //SBuilder.Append("<Button Content=\"删除\" Height=\"26\" Width=\"75\" HorizontalAlignment=\"Center\" Click=\"btn_del_Click\"  Name=\"btn_del\" VerticalAlignment=\"Center\"  />");
            //SBuilder.Append(" </StackPanel>");
            //SBuilder.Append("</DataTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn>");
            //SBuilder.Append("<DataGridTemplateColumn ");
            //SBuilder.Append("xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' ");
            //SBuilder.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
            //SBuilder.Append("xmlns:local='clr-namespace:WanTai.View;assembly=WanTai.View' ");

            //SBuilder.Append("Header=\"采血管\" Width=\"*\" >");
            //SBuilder.Append("<DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append(" <DataTemplate >");
            //SBuilder.Append("<StackPanel Orientation=\"Horizontal\"  HorizontalAlignment=\"Left\" VerticalAlignment=\"Center\" >");
            //SBuilder.Append(" <Label Content=\"{Binding Path=TubesPosition}\" HorizontalAlignment=\"Left\"  Name=\"lab_TubesPosition\" VerticalAlignment=\"Center\" />");
            //SBuilder.Append("</StackPanel>");
            //SBuilder.Append("</DataTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn.CellTemplate>");
            //SBuilder.Append("</DataGridTemplateColumn>");


            //Stream stream = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(SBuilder.ToString()));
            //  DataGridTemplateColumn colIcon = XamlReader.Load(stream) as DataGridTemplateColumn;
            //  colIcon.Header = i.ToString();
            // dg_Bules.Columns.Add(colIcon);

        }

        public DataTable Tubes { get; set; }

        private void dg_Bules_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
        private string SystemFluid ;
    
        public event MainPage.NextStepScan onNextStepScan;

        public event MainPage.FirstStepScan onFirstStepScan;

        private LoadFrom loadFrom;

        private void btn_scan_Click(object sender, RoutedEventArgs e)
        {
           //System.Windows.Input.Cursor currentCurson = this.Cursor;
           // this.Cursor = Cursors.Wait;
            TubesScanParameter tubeScanParameter = new TubesScanParameter();
            if (!(bool)tubeScanParameter.ShowDialog())
            {
                MessageBox.Show("请输入要扫描的列数！","系统提示！");
                return;
            }
            btn_detail.IsEnabled = false;
            btn_Group.IsEnabled = false;
            btn_scan.IsEnabled = false;
            btn_Save.IsEnabled = false;
            btn_Next.IsEnabled = false;
            loadFrom = new LoadFrom();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            bool WaitFalg = true;
            bool scanResult=false;
            string ErrMsg="";
            SystemFluid = "";
            DateTime fileCreatedTime = DateTime.MinValue;
            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                //DateTime fileCreatedTime = WanTai.Controller.EVO.ProcessorFactory.GetDateTimeNow();
                TubesController controller = new TubesController();
                if (SessionInfo.NextTurnStep == 0 || SessionInfo.BatchType == "B")
                {
                    scanResult = true;
                    string NextStepScanFinished = onNextStepScan("NextStepScan");
                    if (NextStepScanFinished != "NextStepScanFinished")
                    {
                        ErrMsg="扫描未完成！";
                        return;
                    }
                    else
                    {
                        Tubes = new WanTai.Controller.TubesController().GetTubes(SessionInfo.LiquidTypeList, fileCreatedTime, out ErrMsg, out SystemFluid); 
                        worker.ReportProgress(1, ErrMsg);
                        while (WaitFalg)
                            Thread.Sleep(1000);
                        //onNextStepScan("NextStepScanFinished");
                    }
                }
                else
                {
                    SessionInfo.FirstStepMixing = 1;
                    if (controller.CallScanScript())
                    {
                        SessionInfo.FirstStepMixing = 5;
                        worker.ReportProgress(1, ErrMsg);
                        while (WaitFalg)
                            Thread.Sleep(1000);
                    }
                    else
                    {
                        SessionInfo.FirstStepMixing = 6;
                    }
                }
            };
            worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            {
                if (null != Tubes)
                    dg_Bules.ItemsSource = Tubes.DefaultView;
                WaitFalg = false;
            };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                if (SessionInfo.NextTurnStep == 0 || SessionInfo.BatchType == "B")
                {
                    CloseLoadingForm(scanResult, ErrMsg);
                }              
            };
            btn_scan.IsEnabled = false;
            worker.RunWorkerAsync();

            if (SessionInfo.NextTurnStep != 0 && SessionInfo.BatchType != "B")
            {
                BackgroundWorker wait_worker = new BackgroundWorker();
                wait_worker.WorkerReportsProgress = false;
                wait_worker.WorkerSupportsCancellation = true;
                wait_worker.DoWork += delegate(object s, DoWorkEventArgs args)
                {
                    scanResult = true;
                    string FirstStepScanFinished = onFirstStepScan("FirstStepScan");
                    if (FirstStepScanFinished != "FirstStepScanFinished")
                    {
                        SessionInfo.FirstStepMixing = 3;
                        ErrMsg = "扫描未完成！";
                        return;
                    }
                    else
                    {
                        SessionInfo.FirstStepMixing = 2;
                    }
                };
                wait_worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
                {
                    if (SessionInfo.FirstStepMixing == 2)
                    {
                        Tubes = new WanTai.Controller.TubesController().GetTubes(SessionInfo.LiquidTypeList, fileCreatedTime, out ErrMsg, out SystemFluid);
                        if (null != Tubes)
                            dg_Bules.ItemsSource = Tubes.DefaultView;
                    }
                    CloseLoadingForm(scanResult, ErrMsg);
                };
                wait_worker.RunWorkerAsync();
            }

            loadFrom.ShowDialog();

            //   sp_ScanLoad.Visibility = System.Windows.Visibility.Collapsed;

        }

        private void CloseLoadingForm(Boolean scanResult, string ErrMsg)
        {
            loadFrom.Close();
            btn_scan.IsEnabled = true;
            if (!scanResult)
            {
                MessageBox.Show("execute scan error", "系统提示");
                return;
            }
            if (SystemFluid.IndexOf("2,") > 0 || SystemFluid.IndexOf("3,") > 0)
            {
                MessageBox.Show("阴阳对照物必须有!", "系统提示!");
                return;
            }
            if (ErrMsg != "success")
                MessageBox.Show(ErrMsg, "系统提示!");
            else
            {
                btn_detail.IsEnabled = true;
                btn_Group.IsEnabled = true;
                dg_TubesGroup.Items.Clear();
                RowIndex = 0;
                TuubesGroupName = 0;
                CurrentTubesPositions = "";
            }
        }

        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            if (dg_TubesGroup.SelectedItem != null)
            {
                TubeGroup _TubeGroup = ((TubeGroup)dg_TubesGroup.SelectedItem);
                foreach (string TubesPosition in ((TubeGroup)dg_TubesGroup.SelectedItem).TubesPosition.Split(']'))
                {
                    if (string.IsNullOrEmpty(TubesPosition)) continue;
                    string str = TubesPosition.Remove(0, 1);
                    int ColumnIndex = int.Parse(str.Split(',')[0]);
                    int RowIndex = int.Parse(str.Split(',')[1]);
                    Tubes.Rows[RowIndex-1]["Background" + ColumnIndex.ToString()] = null;
                    string TextItemCount = Tubes.Rows[RowIndex - 1]["TextItemCount" + ColumnIndex.ToString()].ToString();

                   
                    foreach (TestingItemConfiguration _TestingItemConfiguration in _TubeGroup.TestingItemConfigurations)
                    {
                       TextItemCount= TextItemCount.Replace("," + _TubeGroup.RowIndex.ToString() + ";" + _TestingItemConfiguration.TestingItemColor, "");
                    }
                    Tubes.Rows[RowIndex - 1]["TextItemCount" + ColumnIndex.ToString()] = TextItemCount;
                    Tubes.Rows[RowIndex - 1]["DetailView" + ColumnIndex.ToString()] = Tubes.Rows[RowIndex - 1]["DetailView" + ColumnIndex.ToString()].ToString().Replace(_TubeGroup.TubesGroupName+" "+_TubeGroup.PoolingRulesName+_TubeGroup.TestintItemName + ",","");


                    //int i = 0;
                    //while (_TubeGroup.TestingItemConfigurations.Count > i)
                    //{
                    //    TestingItemConfiguration _TestingItemConfiguration = _TubeGroup.TestingItemConfigurations.ToList()[i++];
                    //    Tubes.Rows[RowIndex - 1]["TextItemCount" + ColumnIndex.ToString()] = TextItemCount.Replace("," + _TubeGroup.RowIndex.ToString() + ";" + _TestingItemConfiguration.TestingItemColor, "");
                        
                    //    //((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName = ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName.Replace(" " + _TestingItemConfiguration.TestingItemName, "");
                    //    //if (_TestingItemConfiguration.TestingItemName == "HBV")
                    //    //    HBVNumber -= _TubeGroup.TubesNumber;
                    //    //if (_TestingItemConfiguration.TestingItemName == "HCV")
                    //    //    HCVNumber -= _TubeGroup.TubesNumber;
                    //    //if (_TestingItemConfiguration.TestingItemName == "HIV")
                    //    //    HIVNumber -= _TubeGroup.TubesNumber;
                    //   // bool b = _TubeGroup.TestingItemConfigurations.Remove(_TubeGroup.TestingItemConfigurations.Where(tic => tic.TestingItemID == _TestingItemConfiguration.TestingItemID).FirstOrDefault());
                    //}
                }
                while (_TubeGroup.TestingItemConfigurations.Count > 0)
                {
                    TestingItemConfiguration _TestingItemConfiguration = _TubeGroup.TestingItemConfigurations.First();

                    ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName = ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName.Replace(" " + _TestingItemConfiguration.TestingItemName, "");

                    //if (CurrentTubesBatch.TestingItem == null)
                    //    CurrentTubesBatch.TestingItem = new Dictionary<Guid, int>();

                    //int TestintItemNumber = _TubeGroup.TubesNumber / _TubeGroup.PoolingRulesTubesNumber + (_TubeGroup.TubesNumber % _TubeGroup.PoolingRulesTubesNumber > 0 ? 1 : 0);
                    //if (CurrentTubesBatch.TestingItem.ContainsKey(_TestingItemConfiguration.TestingItemID))
                    //    CurrentTubesBatch.TestingItem[_TestingItemConfiguration.TestingItemID] -= TestintItemNumber;
                    //else
                    //    CurrentTubesBatch.TestingItem.Add(_TestingItemConfiguration.TestingItemID, 0);

               
                    bool b = _TubeGroup.TestingItemConfigurations.Remove(_TubeGroup.TestingItemConfigurations.Where(tic => tic.TestingItemID == _TestingItemConfiguration.TestingItemID).FirstOrDefault());
                }
                dg_TubesGroup.Items.Remove(dg_TubesGroup.SelectedItem);

                for (int i = 0; i < dg_TubesGroup.Items.Count; i++)
                {
                    Label labTuubesGroupName = CommFuntion.FindName(dg_TubesGroup, 0, i, "lab_TubesGroupName") as Label;

                    //  Label labTuubesGroupName = CommFuntion.FindCellControl<Label>("lab_TubesGroupName", dg_TubesGroup, i, 0);
                    if (labTuubesGroupName != null)
                        labTuubesGroupName.Content = "分组" + (i + 1).ToString();
                    //  ((TubeGroup)dg_TubesGroup.Items[i]).TuubesGroupName = "分组" + (i+1).ToString();
                    TuubesGroupName = i + 1;
                }

                if (dg_TubesGroup.Items.Count == 0)
                {
                    TuubesGroupName = 0;
                    btn_Next.IsEnabled = false;
                    btn_Save.IsEnabled = false;
                }
                else
                    dg_TubesGroup.SelectedIndex = TuubesGroupName-1;
            }
        }
       

        private void btn_Group_Click(object sender, RoutedEventArgs e)
        {
            int TubesNumber = 0;
          
            List<TubeCell> tubeCells = new List<TubeCell>();
            DataTable dt = new DataTable();
            dt.Columns.Add("RowIndex",typeof(int));
            dt.Columns.Add("ColumnIndex",typeof(int));
            dt.Columns.Add("TubeCell", typeof(TubeCell));

            foreach (DataGridCellInfo Cell in dg_Bules.SelectedCells)
            {
                int  ColumnIndex = CommFuntion.GetDataGridCellColumnIndex(Cell);
                int  RowIndex = CommFuntion.GetDataGridCellRowIndex(Cell);
                if (Tubes.Rows[RowIndex - 1]["TubeType" + ColumnIndex.ToString()].ToString() != "Tube")
                {
                    if (Tubes.Rows[RowIndex - 1]["TubeType" + ColumnIndex.ToString()].ToString() == "-1") continue;
                    if (MessageBox.Show("单元格[" + ColumnIndex.ToString() + "," + RowIndex.ToString() + "]阴阳对应液及样品补充液不能参加分组，是否继续？","系统提示！",MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                    continue;
                }
              //  CellBuilder.Append("[" + ColumnIndex.ToString() + "," + RowIndex.ToString() + "]");
                DataRow drow = dt.NewRow();
                drow["RowIndex"] = RowIndex;
                drow["ColumnIndex"] = ColumnIndex;
                drow["TubeCell"] = new TubeCell() { ColumnIndex = ColumnIndex, RowIndex = RowIndex, CellValue = "[" + ColumnIndex.ToString() + "," + RowIndex.ToString() + "]" };
                dt.Rows.Add(drow);
                TubesNumber++;
            }
            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("没有选择采血管", "系统提示");
                return;
            }
           
            DataView dView = dt.DefaultView;
            dView.Sort = " ColumnIndex,RowIndex asc";
            StringBuilder CellBuilder = new StringBuilder();
            foreach (DataRow tubeCell in dView.ToTable().Rows)
            {
                string TubesPosition = ((TubeCell)tubeCell["TubeCell"]).CellValue;
                if (string.IsNullOrEmpty(TubesPosition)) continue;
                string str = TubesPosition.Remove(0, 1);
                str = str.Substring(0, str.Length - 1);
                int ColumnIndex = int.Parse(str.Split(',')[0]);
                int RowIndex = int.Parse(str.Split(',')[1]) - 1;
                  CellBuilder.Append("[" + ColumnIndex.ToString() + "," + (RowIndex+1).ToString() + "]");
                Tubes.Rows[RowIndex]["Background" + ColumnIndex.ToString()] = null;
            }

            //foreach (string TubesPosition in CurrentTubesPositions.Split(']'))
            //{
            //    if (string.IsNullOrEmpty(TubesPosition)) continue;
            //    string str = TubesPosition.Remove(0, 1);
            //    int ColumnIndex = int.Parse(str.Split(',')[0]);
            //    int RowIndex = int.Parse(str.Split(',')[1]) - 1;
            //    Tubes.Rows[RowIndex]["Background" + ColumnIndex.ToString()] = null;
            //}
   
           // TubeGroupList.Add(new TubeGroup() { TuubesGroupName = "分组" + (++TuubesGroupName).ToString(), ExperimentID = SessionInfo.ExperimentID, PoolingRulesConfiguration = PoolingRules, PoolingRulesID = PoolingRules.PoolingRulesID, CreateTime = DateTime.Now, TubesPosition = CellBuilder.ToString() });
            int SelectItemIndex = dg_TubesGroup.Items.Add(new TubeGroup() { isComplement = false, TubesNumber = TubesNumber, RowIndex = (this.RowIndex++), TubesGroupName = "分组" + (++TuubesGroupName).ToString(), ExperimentID = SessionInfo.ExperimentID, PoolingRulesConfiguration = PoolingRules, PoolingRulesName = PoolingRules.PoolingRulesName, PoolingRulesID = PoolingRules.PoolingRulesID, CreateTime = DateTime.Now, TubesPosition = CellBuilder.ToString() });
            dg_TubesGroup.SelectedIndex = SelectItemIndex;
            btn_Save.IsEnabled = true;
        }
        private int RowIndex = 0;
        private int TuubesGroupName = 0;
        private PoolingRulesConfiguration PoolingRules
        {
            get;
            set;
        }
        public IList<WanTai.DataModel.PoolingRulesConfiguration> GetPoolingRulesConfigurations()
        {
            return new WanTai.Controller.PoolingRulesConfigurationController().GetActivePoolingRulesConfigurations();
        }
        private void btn_detail_Click(object sender, RoutedEventArgs e)
        {
           // Tubes.Namespace = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            foreach(TubeGroup tubeGroup in dg_TubesGroup.Items)
            {
                foreach (string TubesPosition in tubeGroup.TubesPosition.Split(']'))
                {
                    if (string.IsNullOrEmpty(TubesPosition)) continue;
                    string str = TubesPosition.Remove(0, 1);
                    int ColumnIndex = int.Parse(str.Split(',')[0]);
                    int RowIndex = int.Parse(str.Split(',')[1])-1;
                    if (DateTime.Parse(Tubes.Rows[RowIndex]["DetailViewTime" + ColumnIndex.ToString()].ToString())<DateTime.Parse(Tubes.Namespace))
                       Tubes.Rows[RowIndex]["DetailView" + ColumnIndex.ToString()] = tubeGroup.TubesGroupName;
                    else
                       Tubes.Rows[RowIndex]["DetailView" + ColumnIndex.ToString()] += tubeGroup.TubesGroupName;

                    Tubes.Rows[RowIndex]["DetailView" + ColumnIndex.ToString()] += " "+tubeGroup.PoolingRulesName;

                    //foreach(TestingItemConfiguration TestItem in tubeGroup.TestingItemConfigurations)
                    //    Tubes.Rows[RowIndex]["DetailView" + ColumnIndex.ToString()] += " " + TestItem.TestingItemName;
                    Tubes.Rows[RowIndex]["DetailView" + ColumnIndex.ToString()] += tubeGroup.TestintItemName + ",";
                    
                    Tubes.Rows[RowIndex]["DetailViewTime" + ColumnIndex.ToString()] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            Tubes.Namespace = DateTime.Now.AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss");
            TubesDetailView _TubesDetailView = new TubesDetailView();
            _TubesDetailView.Tubes = Tubes;
            _TubesDetailView.ShowDialog();
        }

        private void dg_Bules_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCellInfo Cell=((System.Windows.Controls.DataGrid)(e.Source)).CurrentCell;

            if (Cell == null || Tubes == null || Cell.Column == null || Cell.Item==null) return;
            if (Tubes.Rows.Count == 0) return;
            int ColumnIndex= CommFuntion.GetDataGridCellColumnIndex(Cell);
            int RowIndex=CommFuntion.GetDataGridCellRowIndex(Cell)-1;
            if (Tubes.Rows[RowIndex]["TubeType" +ColumnIndex.ToString()].ToString() == "-1")
            {
                MessageBox.Show("当前位置没有采血管！", "系统提示");
                return;
            }
            TubeBarCodeEdit _TubeBarCodeEdit = new TubeBarCodeEdit();
            _TubeBarCodeEdit.Cell = ((System.Windows.Controls.DataGrid)(e.Source)).CurrentCell;
            if ((Boolean)_TubeBarCodeEdit.ShowDialog())
            {
                Tubes.Rows[RowIndex]["BarCode"+ColumnIndex.ToString()] = _TubeBarCodeEdit.txt_BarCode.Text;
            }
        }

        private void dg_TubesGroup_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header ="分组"+(e.Row.GetIndex()+1).ToString();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsPoack)
            {
                AddColumns();
                CurrentTubesPositions = "";
            }
            IsPoack = true;
         
            labelRotationName.Content = (SessionInfo.PraperRotation == null ? "" : SessionInfo.PraperRotation.RotationName)
                + (SessionInfo.BatchType == "A" ? "(第1次上样)" : "") + (SessionInfo.BatchType == "B" ? "(第2次上样)" : "");
        }

        public void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (dg_TubesGroup.SelectedItem != null)
            {
               TestingItemConfiguration _TestingItemConfiguration = (TestingItemConfiguration)(((System.Windows.Controls.CheckBox)(sender)).DataContext);
                WanTai.DataModel.TubeGroup tubeGroup= ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem));
                string TubesPositions = tubeGroup.TubesPosition;
               foreach (string TubesPosition in TubesPositions.Split(']'))
               {
                   if (string.IsNullOrEmpty(TubesPosition)) continue;
                   string str = TubesPosition.Remove(0, 1);
                   int ColumnIndex = int.Parse(str.Split(',')[0]);
                   int RowIndex = int.Parse(str.Split(',')[1]);

                   string TextItemCount = Tubes.Rows[RowIndex - 1]["TextItemCount" + ColumnIndex.ToString()].ToString() + "," + tubeGroup.RowIndex.ToString()+";" + _TestingItemConfiguration.TestingItemColor;
                   Tubes.Rows[RowIndex - 1]["TextItemCount" + ColumnIndex.ToString()] = TextItemCount.ToString();
               }
               //((System.Data.DataRowView)(dg_Bules.Items[0]))._row.ItemArray[5] = "2";
               //dg_Bules.ItemsSource = null;
               // dg_Bules.ItemsSource = Tubes.DefaultView;
                //var query = from d in Dic_BandRate
                //        where d.Value == 9600
                //        select d.Key;
               // if(CurrentTubesBatch.TestingItem==null)
               //     CurrentTubesBatch.TestingItem =new Dictionary<Guid,int>();
               // int TestintItemNumber = tubeGroup.TubesNumber / tubeGroup.PoolingRulesTubesNumber + (tubeGroup.TubesNumber % tubeGroup.PoolingRulesTubesNumber > 0 ? 1 : 0);
               // if (CurrentTubesBatch.TestingItem.ContainsKey(_TestingItemConfiguration.TestingItemID))
               //     CurrentTubesBatch.TestingItem[_TestingItemConfiguration.TestingItemID] += TestintItemNumber;
               //else
               //     CurrentTubesBatch.TestingItem.Add(_TestingItemConfiguration.TestingItemID, TestintItemNumber);
              
               ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName = ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName + " " + _TestingItemConfiguration.TestingItemName;
             //  ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestingItemConfigurations.Add(IestingItemList.Where(tic => tic.TestingItemID == _TestingItemConfiguration.TestingItemID).FirstOrDefault());
               ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestingItemConfigurations.Add(new TestingItemConfiguration() { TestingItemID = _TestingItemConfiguration.TestingItemID, TestingItemName = _TestingItemConfiguration.TestingItemName, TestingItemColor = _TestingItemConfiguration.TestingItemColor });
            }
        }
        private List<TestingItemConfiguration> _IestingItemList;
        private List<TestingItemConfiguration> IestingItemList
        {
            get {
                if (_IestingItemList == null)
                    _IestingItemList = new WanTai.Controller.TestItemController().GetActiveTestItemConfigurations().ToList();
                return _IestingItemList;
            }
        }
    
        public void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dg_TubesGroup.SelectedItem != null)
            {
                TestingItemConfiguration _TestingItemConfiguration = (TestingItemConfiguration)(((System.Windows.Controls.CheckBox)(sender)).DataContext);
                WanTai.DataModel.TubeGroup _TubeGroup = ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem));

                foreach (string TubesPosition in _TubeGroup.TubesPosition.Split(']'))
                {
                    if (string.IsNullOrEmpty(TubesPosition)) continue;
                    string str = TubesPosition.Remove(0, 1);
                    int ColumnIndex = int.Parse(str.Split(',')[0]);
                    int RowIndex = int.Parse(str.Split(',')[1]);

                    string TextItemCount = Tubes.Rows[RowIndex - 1]["TextItemCount" + ColumnIndex.ToString()].ToString();
                    Tubes.Rows[RowIndex - 1]["TextItemCount" + ColumnIndex.ToString()] = TextItemCount.Replace("," + _TubeGroup.RowIndex.ToString() + ";" + _TestingItemConfiguration.TestingItemColor, "");
                }
                ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName = ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).TestintItemName.Replace(" " + _TestingItemConfiguration.TestingItemName, "");
                //if (CurrentTubesBatch.TestingItem == null)
                //    CurrentTubesBatch.TestingItem = new Dictionary<Guid, int>();

                //int TestintItemNumber = _TubeGroup.TubesNumber / _TubeGroup.PoolingRulesTubesNumber + (_TubeGroup.TubesNumber % _TubeGroup.PoolingRulesTubesNumber > 0 ? 1 : 0);
                //if (CurrentTubesBatch.TestingItem.ContainsKey(_TestingItemConfiguration.TestingItemID))
                //    CurrentTubesBatch.TestingItem[_TestingItemConfiguration.TestingItemID] -= TestintItemNumber;
                //else
                //    CurrentTubesBatch.TestingItem.Add(_TestingItemConfiguration.TestingItemID, 0);
                bool b = _TubeGroup.TestingItemConfigurations.Remove(_TubeGroup.TestingItemConfigurations.Where(tic => tic.TestingItemID == _TestingItemConfiguration.TestingItemID).FirstOrDefault());
            }
        }

        public event WanTai.View.MainPage.NextStepHandler NextStepEvent;
        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            //update batch to rotation
            if (SessionInfo.PraperRotation != null)
            {
                new RotationInfoController().UpdateTubesBatch(SessionInfo.PraperRotation.RotationID, CurrentTubesBatch.TubesBatchID);
                if (SessionInfo.PraperRotation != null)
                {
                    FormulaParameters formulaParameters = SessionInfo.RotationFormulaParameters[Guid.Empty];
                    SessionInfo.RotationFormulaParameters.Remove(Guid.Empty);
                    if (!SessionInfo.RotationFormulaParameters.ContainsKey(SessionInfo.PraperRotation.RotationID))
                    {
                        SessionInfo.RotationFormulaParameters.Add(SessionInfo.PraperRotation.RotationID, formulaParameters);
                    }
                    else
                    {
                        SessionInfo.RotationFormulaParameters[SessionInfo.PraperRotation.RotationID] = formulaParameters;
                    }
                }
                new PlateController().UpdateRotationId(CurrentTubesBatch.TubesBatchID, SessionInfo.PraperRotation.RotationID);
            }
            dg_TubesGroup.IsEnabled = false;
            dg_Bules.IsEnabled = false;
            //for (int i = 0; i < dg_TubesGroup.Items.Count; i++)
            //{
            //    ComboBox cb_PoolingRulesConfigurations = CommFuntion.FindName(dg_TubesGroup, 1, i, "btn_del") as ComboBox;
            //    if (cb_PoolingRulesConfigurations != null)
            //        cb_PoolingRulesConfigurations.IsEditable = false;
            //    Button btn_del = CommFuntion.FindName(dg_TubesGroup, 3, i, "btn_del") as Button;
            //    if (btn_del != null)
            //        btn_del.IsEnabled = false;

            //    CheckBox ch_Complement = CommFuntion.FindName(dg_TubesGroup, 4, i, "ch_Complement") as CheckBox;
            //    if (ch_Complement != null)
            //        ch_Complement.IsEnabled = false;
                   
            //}
            btn_detail.IsEnabled = false;
            btn_Group.IsEnabled = false;
            btn_Next.IsEnabled = false;
            btn_Save.IsEnabled = false;
            btn_scan.IsEnabled = false;
            SessionInfo.NextButIndex = 1;
            if (SessionInfo.NextTurnStep==0)
              SessionInfo.NextTurnStep = 1;
            if (NextStepEvent != null)
            {
                NextStepEvent(sender, e);
            }
        }

        private void cb_PoolingRulesConfigurations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dg_TubesGroup.SelectedItem != null)
            {
                WanTai.DataModel.TubeGroup tubeGroup = ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem));
                PoolingRulesConfiguration PoolingRules = ((PoolingRulesConfiguration)e.AddedItems[0]);
                
                ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).PoolingRulesID = ((PoolingRulesConfiguration)e.AddedItems[0]).PoolingRulesID;
                ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).PoolingRulesName = ((PoolingRulesConfiguration)e.AddedItems[0]).PoolingRulesName;
                ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).PoolingRulesTubesNumber = ((PoolingRulesConfiguration)e.AddedItems[0]).TubeNumber;
                CheckBox ch_Complement=CommFuntion.FindCellControl<CheckBox>("ch_Complement",dg_TubesGroup,dg_TubesGroup.SelectedIndex,4);

                if (ch_Complement != null)
                {
                    if ((tubeGroup.TubesPosition.Split(']').Length - 1) % PoolingRules.TubeNumber > 0)
                    {
                        ch_Complement.IsEnabled = true;
                        if (MessageBox.Show("该分组需要补液，是否补液？", "系统提示!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            ch_Complement.IsChecked = true;
                        else
                            ch_Complement.IsChecked = false;
                    }
                    else
                    {
                        ch_Complement.IsEnabled = false;
                        ch_Complement.IsChecked = false;
                    }
                }
            }
        }
        private string CurrentTubesPositions { get; set; }
        private void dg_TubesGroup_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridCellInfo Cell = ((System.Windows.Controls.DataGrid)(e.Source)).CurrentCell;

            if (Cell == null || Tubes == null || Cell.Column == null || Cell.Item == null) return;
            if (Tubes.Rows.Count == 0) return;
            if (dg_TubesGroup.Items.Count == 0) return;
            dg_Bules.SelectedCells.Clear();
            foreach (string TubesPosition in CurrentTubesPositions.Split(']'))
            {
                if (string.IsNullOrEmpty(TubesPosition)) continue;
                string str = TubesPosition.Remove(0, 1);
                int ColumnIndex = int.Parse(str.Split(',')[0]);
                int RowIndex = int.Parse(str.Split(',')[1]) - 1;
                Tubes.Rows[RowIndex]["Background" + ColumnIndex.ToString()] =null;
            }
             CurrentTubesPositions = ((WanTai.DataModel.TubeGroup)(Cell.Item)).TubesPosition;
             foreach (string TubesPosition in CurrentTubesPositions.Split(']'))
             {
                 if (string.IsNullOrEmpty(TubesPosition)) continue;
                 string str = TubesPosition.Remove(0, 1);
                 int ColumnIndex = int.Parse(str.Split(',')[0]);
                 int RowIndex = int.Parse(str.Split(',')[1])-1;
                 Tubes.Rows[RowIndex]["Background" + ColumnIndex.ToString()] = "Green";
             }
        }
        private TubesBatch CurrentTubesBatch = new TubesBatch() { TubesBatchID = new Guid(),TestingItem= new Dictionary<Guid,int>() };
        private IList<TubeGroup> TubeGroupList;
        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            int TotalHole = 2;
            TubeGroupList = new List<TubeGroup>();
            CurrentTubesBatch.TestingItem = new Dictionary<Guid, int>();

            if (SessionInfo.BatchType == "A")
            {
                if (null == SessionInfo.BatchATubeGroups)
                    SessionInfo.BatchATubeGroups = new List<TubeGroup>();
                else
                    SessionInfo.BatchATubeGroups.Clear();
                if (null == SessionInfo.BatchATestingItem)
                    SessionInfo.BatchATestingItem = new Dictionary<Guid, int>();
                else
                    SessionInfo.BatchATestingItem.Clear();
                SessionInfo.BatchATotalHoles = 2;
            }
            else if (SessionInfo.BatchType == "B")
            {
                TotalHole += SessionInfo.BatchATotalHoles;
            }

            btn_Next.IsEnabled = false;

            if (SessionInfo.ExperimentID == new Guid())
            {
                NewExperiment Experiment = new NewExperiment();
                if (!(bool)Experiment.ShowDialog())
                    return;
                SessionInfo.BatchIndex = 1;
            }
            if (SystemFluid.IndexOf("2,") >= 0 || SystemFluid.IndexOf("3,") >= 0)
            {
                MessageBox.Show("阴阳对照物必须有!", "系统提示!");
                return;
            }
            bool _SystemFluid = false;
            foreach (TubeGroup Item in dg_TubesGroup.Items)
            {
                if (Item.TestingItemConfigurations.Count == 0)
                {
                     MessageBox.Show(Item.TubesGroupName + "没有选择检测项目，请选择！", "系统提示!");
                     return;
                }
                foreach (TestingItemConfiguration TestingItem in Item.TestingItemConfigurations)
                {
                    int TestintItemNumber = Item.TubesNumber / Item.PoolingRulesTubesNumber + (Item.TubesNumber % Item.PoolingRulesTubesNumber > 0 ? 1 : 0);
                    if (CurrentTubesBatch.TestingItem.ContainsKey(TestingItem.TestingItemID))
                        CurrentTubesBatch.TestingItem[TestingItem.TestingItemID] += TestintItemNumber;
                    else
                        CurrentTubesBatch.TestingItem.Add(TestingItem.TestingItemID, TestintItemNumber);
                }
                TotalHole += Item.TubesNumber / Item.PoolingRulesTubesNumber + (Item.TubesNumber % Item.PoolingRulesTubesNumber > 0 ? 1 : 0);
                if (SessionInfo.BatchType == "A")
                {
                    Item.BatchType = "A";
                    SessionInfo.BatchATubeGroups.Add(Item);
                }
                else if (SessionInfo.BatchType == "B")
                {
                    Item.BatchType = "B";
                }
                TubeGroupList.Add(Item);
                if (Item.isComplement) _SystemFluid = true;
            }
            if (TotalHole > 96)
            {
                MessageBox.Show("混样数大于96,无法进行混样!","系统提示!");
                return;
            }
            if (_SystemFluid)
            {
                if (SystemFluid.IndexOf("1,") >= 0)
                {
                    MessageBox.Show("没有样品补充液!","系统提示!");
                    return;
                }
            }
            string ErrMsg;
            int ErrType;
            CurrentTubesBatch = new WanTai.Controller.TubesGroupController().SaveTubesGroup(SessionInfo.ExperimentID, CurrentTubesBatch, SessionInfo.BatchIndex, TubeGroupList, Tubes, out ErrType, out ErrMsg);
            if (ErrType == -1)
            {
                MessageBox.Show(ErrMsg, "系统提示!");
               
                return; 
            }
            // update batch a info
            if (SessionInfo.BatchType == "A")
            {
                SessionInfo.BatchATotalHoles = TotalHole;
                SessionInfo.BatchATestingItem = CurrentTubesBatch.TestingItem;
                SessionInfo.BatchATubes = Tubes.Copy();
            }
            //CurrentTubesBatch.TestingItem = new Dictionary<Guid, int>();
           // MessageBox.Show("生成成功！", "系统提示!");
            btn_Next.IsEnabled = true;
        }

        private void ch_Complement_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dg_TubesGroup.SelectedItem != null)
            {
                ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).isComplement = false;
            }
        }

        private void ch_Complement_Checked(object sender, RoutedEventArgs e)
        {
            if (dg_TubesGroup.SelectedItem != null)
            {
                ((WanTai.DataModel.TubeGroup)(dg_TubesGroup.SelectedItem)).isComplement = true;
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
             TubesDetailView _TubesDetailView = new TubesDetailView();
             _TubesDetailView.BatchID = new Guid("65dae092-ea75-11e0-a989-001ec94c5271");
            _TubesDetailView.ShowDialog();
         
        }  
        struct TubeCell
        {
           public int RowIndex { get; set; }
           public int ColumnIndex { get; set; }
           public string CellValue { get; set; }
        }
    }
}