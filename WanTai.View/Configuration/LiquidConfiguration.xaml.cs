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
using System.Collections;

using WanTai.Common;
using WanTai.DataModel;
using WanTai.Controller.Configuration;
using WanTai.DataModel.Configuration;

namespace WanTai.View.Configuration
{
    /// <summary>
    /// Interaction logic for LiquidConfiguration.xaml
    /// </summary>
    public partial class LiquidConfiguration : Window
    {
        LiquidConfigurationController controller = new LiquidConfigurationController();
        System.Data.DataTable dTable = new System.Data.DataTable();
        public LiquidConfiguration()
        {
            InitializeComponent();
        }

        private void dataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            e.Row.Height = 30;
            //e.Row.Height = (dataGrid1.Height - 30) / 16;
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LiquidConfigurationColorConvert _LiquidConfigurationColorConvert = new LiquidConfigurationColorConvert();

            string workDeskType = WanTai.Common.Configuration.GetWorkDeskType();
            int range = 6;

            if (workDeskType == "100")
            {
                range = 7;
            }
            else if (workDeskType == "150")
            {
                range = 19;
            }
            else if (workDeskType == "200")
            {
                range = 37;
            }

            for (int i = 1; i < range; i++)
            {
                dTable.Columns.Add(i.ToString());
                
                dTable.Columns.Add("type"+ i);
                DataGridTextColumn _DataGridTextColumn = new DataGridTextColumn() { Header = i, IsReadOnly = true, CanUserResize = false, Binding = new Binding(i.ToString()) };
                Style _style=new System.Windows.Style();                
                _style.Setters.Add(new System.Windows.Setter(BackgroundProperty, new Binding() { Path = new PropertyPath("type" + i), Converter = _LiquidConfigurationColorConvert }));
                _DataGridTextColumn.CellStyle=_style;
                dataGrid1.Columns.Add(_DataGridTextColumn);
            }

            for (int i = 1; i < 17; i++)
            {
                System.Data.DataRow dRow = dTable.NewRow();
                dTable.Rows.Add(dRow);
            }
            
            dataGrid1.ItemsSource = dTable.DefaultView;

            List<LiquidType> types = WanTai.Common.Configuration.GetLiquidTypes();
            if (types != null)
            {                
                foreach (LiquidType type in types)
                {
                    liquidType_comboBox.Items.Add(new ComboBoxItem() { Content = type.TypeName, DataContext = type });
                }
            }

            volumeLabel.Visibility = Visibility.Hidden;
            volume_TextBox.Visibility = Visibility.Hidden;
            volumeUnitlabel.Visibility = Visibility.Hidden;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //dataGrid1.RowHeight = (Grid3.ActualHeight - dataGrid1.ColumnHeaderHeight)/16 - 1;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void liquidType_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)liquidType_comboBox.SelectedItem;
            string content = selectedItem.Content.ToString();
            LiquidType type = (LiquidType)selectedItem.DataContext;
            dataGrid1.UnselectAllCells(); 
            SetRecordsBackGround();

            if (type != null)
            {
                txt_Control.Visibility = System.Windows.Visibility.Visible;
                lab_Control.Visibility = System.Windows.Visibility.Visible;
                txt_Control.Background = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(type.Color) };
                lab_Control.Content = type.TypeName;

                if (type.HasVolume)
                {
                    volumeLabel.Visibility = Visibility.Visible;
                    volume_TextBox.Visibility = Visibility.Visible;
                    volume_TextBox.Text = type.DefaultVolume.ToString();
                    volumeUnitlabel.Visibility = Visibility.Visible;
                }
                else
                {
                    volumeLabel.Visibility = Visibility.Hidden;
                    volume_TextBox.Visibility = Visibility.Hidden;
                    volumeUnitlabel.Visibility = Visibility.Hidden;
                }

                List<SystemFluidConfiguration> configRecords = controller.GetLiquidConfigurationByTypeId(type.TypeId);
                if (configRecords != null && configRecords.Count > 0)
                {
                    foreach (SystemFluidConfiguration config in configRecords)
                    {
                        volume_TextBox.Text = config.Volume.ToString();

                        int columnIndex = (int)config.Grid;
                        int rowIndex = (int)config.Position;

                        dTable.Rows[rowIndex - 1]["type" + columnIndex] = config.ItemType.ToString();                      
                    }

                    setButtonStatus(true);
                }
                else
                {
                    setButtonStatus(false);
                }
            }            
        }

        private void SetRecordsBackGround()
        {
            List<SystemFluidConfiguration> configRecords = controller.GetAllLiquidConfiguration();
            if (configRecords != null && configRecords.Count > 0)
            {
                foreach (SystemFluidConfiguration config in configRecords)
                {
                    int columnIndex = (int)config.Grid;
                    int rowIndex = (int)config.Position;

                    dTable.Rows[rowIndex - 1]["type" + columnIndex] = "hasRecord";
                }                
            }
        }

        private void setButtonStatus(bool hasRecord)
        {
            if (!hasRecord)
            {
                save.Visibility = System.Windows.Visibility.Visible;
                edit_Button.Visibility = System.Windows.Visibility.Hidden;
                delete_Button.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                save.Visibility = System.Windows.Visibility.Hidden;
                edit_Button.Visibility = System.Windows.Visibility.Visible;
                delete_Button.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void save_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!validate())
                return;            

            bool result = false;
            ComboBoxItem selectedItem = (ComboBoxItem)liquidType_comboBox.SelectedItem;
            foreach (DataGridCellInfo cell in dataGrid1.SelectedCells)
            {
                int columnIndex = CommFuntion.GetDataGridCellColumnIndex(cell);
                int rowIndex = CommFuntion.GetDataGridCellRowIndex(cell);

                SystemFluidConfiguration liquidConfiguration = new SystemFluidConfiguration();
                liquidConfiguration.ItemID = WanTaiObjectService.NewSequentialGuid();
                liquidConfiguration.Position = rowIndex;
                liquidConfiguration.Grid = columnIndex;
                liquidConfiguration.ItemType = ((LiquidType)selectedItem.DataContext).TypeId;
                if(volume_TextBox.IsVisible)
                {
                    liquidConfiguration.Volume = double.Parse(volume_TextBox.Text);
                }

                result = controller.Create(liquidConfiguration);
                if (!result)
                {
                    WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建：" + ((LiquidType)selectedItem.DataContext).TypeName + " 失败", SessionInfo.LoginName, this.GetType().ToString(), null);
                    MessageBox.Show("保存失败！", "系统提示");
                    break;
                }
            }

            if (result)
            {
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建：" + ((LiquidType)selectedItem.DataContext).TypeName + " 成功", SessionInfo.LoginName, this.GetType().ToString(), null);
                MessageBox.Show("保存成功！", "系统提示");          
            }

            this.Close();
        }

        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)liquidType_comboBox.SelectedItem;
            string content = selectedItem.Content.ToString();
            LiquidType type = (LiquidType)selectedItem.DataContext;
            if (type != null)
            {
                bool result = controller.Delete(type.TypeId);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "删除：" + ((LiquidType)selectedItem.DataContext).TypeName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);            
             
                if (result)
                {
                    MessageBox.Show("删除成功！", "系统提示");
                }
                else
                {
                    MessageBox.Show("删除失败！", "系统提示");
                }

                this.Close();
            }
        }

        private void edit_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
                return;

            ComboBoxItem selectedItem = (ComboBoxItem)liquidType_comboBox.SelectedItem;
            List<SystemFluidConfiguration> recordList = new List<SystemFluidConfiguration>();
            short typeId = ((LiquidType)selectedItem.DataContext).TypeId;
            foreach (DataGridCellInfo cell in dataGrid1.SelectedCells)
            {
                int columnIndex = CommFuntion.GetDataGridCellColumnIndex(cell);
                int rowIndex = CommFuntion.GetDataGridCellRowIndex(cell);

                SystemFluidConfiguration liquidConfiguration = new SystemFluidConfiguration();
                liquidConfiguration.Position = rowIndex;
                liquidConfiguration.Grid = columnIndex;
                liquidConfiguration.ItemType = typeId;
                if (volume_TextBox.IsVisible)
                {
                    liquidConfiguration.Volume = double.Parse(volume_TextBox.Text);
                }

                recordList.Add(liquidConfiguration);                
            }

            bool result = controller.EditByTypeId(recordList, typeId);
            WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改：" + ((LiquidType)selectedItem.DataContext).TypeName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);            
                    
            if (result)
            {
                MessageBox.Show("保存成功！", "系统提示");
            }
            else
            {

                MessageBox.Show("保存失败！", "系统提示");
            }

            this.Close();
        }

        private bool validate()
        {
            ComboBoxItem selectedItem = (ComboBoxItem)liquidType_comboBox.SelectedItem;
            if (selectedItem == null || string.IsNullOrEmpty(selectedItem.Content.ToString()))
            {
                MessageBox.Show("请选择设置类型", "系统提示");
                return false;
            }

            IList<DataGridCellInfo> cellsList = dataGrid1.SelectedCells;
            if (cellsList == null || cellsList.Count == 0)
            {
                MessageBox.Show("请选择采血管区域", "系统提示");
                return false;
            }

            foreach (DataGridCellInfo cell in cellsList)
            {
                int columnIndex = CommFuntion.GetDataGridCellColumnIndex(cell);
                int rowIndex = CommFuntion.GetDataGridCellRowIndex(cell);

                if (dTable.Rows[rowIndex - 1]["type" + columnIndex] != null && dTable.Rows[rowIndex - 1]["type" + columnIndex].ToString() == "hasRecord")
                {
                    MessageBox.Show("不能选择已被其他类型保存的区域", "系统提示");
                    return false;
                }                
            }
            
            LiquidType type = (LiquidType)selectedItem.DataContext;
            if (type != null)
            {
                if (!type.CanSelectedMultiCell && cellsList.Count > 1)
                {
                    MessageBox.Show("不能选择多个采血管区域", "系统提示");
                    return false;
                }

                if (type.HasVolume && volume_TextBox.IsVisible)
                {
                    double volume_out = 0;
                    if (string.IsNullOrEmpty(volume_TextBox.Text) || !double.TryParse(volume_TextBox.Text, out volume_out))
                    {
                        MessageBox.Show("容量输入不合法，必须为数字", "系统提示");
                        volume_TextBox.Focus();
                        return false;
                    }
                }
            } 

            return true;
        }
    }
}
