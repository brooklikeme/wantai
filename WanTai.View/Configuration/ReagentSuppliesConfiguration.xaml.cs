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

using WanTai.DataModel.Configuration;
using WanTai.Controller.Configuration;
using WanTai.DataModel;

namespace WanTai.View.Configuration
{
    /// <summary>
    /// Interaction logic for ReagentSuppliesConfiguration.xaml
    /// </summary>
    public partial class ReagentSuppliesConfiguration : Window
    {
        ReagentSuppliesConfigurationController controller = new ReagentSuppliesConfigurationController();
        private string editedItemId;
        public ReagentSuppliesConfiguration()
        {
            InitializeComponent();
        }

        public void SetEditedItemId(string itemId)
        {
            editedItemId = itemId;
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<ReagentSuppliesType>  typeList = WanTai.Common.Configuration.GetReagentSuppliesTypes();
            if (typeList != null)
            {
                foreach (ReagentSuppliesType type in typeList)
                {
                    type_comboBox.Items.Add(new ComboBoxItem() { Content = type.TypeName, DataContext = type });
                }
            }

            List<string> carriers = controller.GetCarriers();
            foreach (string carrierName in carriers)
            {
                carrier_comboBox.Items.Add(new ComboBoxItem(){Content=carrierName});
            }

            InitField();
        }

        private void InitField()
        {
            if (!string.IsNullOrEmpty(editedItemId))
            {
                ReagentAndSuppliesConfiguration item = controller.GetConfiguration(new Guid(editedItemId));
                this.name_textBox.Text = item.EnglishName;
                this.displayName_textBox.Text = item.DisplayName;
                foreach (ComboBoxItem comboxItem in type_comboBox.Items)
                {
                    ReagentSuppliesType reagentSuppliesType = (ReagentSuppliesType)comboxItem.DataContext;
                    if (reagentSuppliesType.TypeId == item.ItemType.ToString())
                    {
                        comboxItem.IsSelected = true;
                        break;
                    }
                }
                
                this.barcode_textBox.Text = item.BarcodePrefix;

                foreach (ComboBoxItem comboxItem in carrier_comboBox.Items)
                {
                    if (comboxItem.Content.ToString() == item.ContainerName)
                    {
                        comboxItem.IsSelected = true;
                        break;
                    }
                }
                
                this.grid_textBox.Text = item.Grid.ToString();
                this.position_textBox.Text = item.Position.ToString();
                this.color_Control.Background = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(item.Color) };
                this.unit_label.Content = item.Unit;
                this.calculation_textBox.Text = item.CalculationFormula;
            }
        }

        private void type_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)type_comboBox.SelectedItem;
            ReagentSuppliesType type = (ReagentSuppliesType)selectedItem.DataContext;
            if (type != null)
            {
                if (!string.IsNullOrEmpty(type.Unit))
                {
                    unit_label.Content = type.Unit;
                }
                else
                {
                    unit_label.Content = "";
                }
            }
            else
            {
                unit_label.Content = "";
            }
        }

        private void color_Control_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            string color  = color_Control.Background.ToString();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(color);
            colorDialog.ShowDialog();
            color = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
            color_Control.Background = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(color) };
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
            {
                return;
            }

            ReagentAndSuppliesConfiguration configuration = new ReagentAndSuppliesConfiguration();
            configuration.WorkDeskType = SessionInfo.WorkDeskType;
            configuration.EnglishName = name_textBox.Text;
            configuration.DisplayName = displayName_textBox.Text;
            configuration.Position = int.Parse(position_textBox.Text);
            configuration.Grid = int.Parse(grid_textBox.Text);
            configuration.Unit = unit_label.Content.ToString();
            ComboBoxItem selectedItem = type_comboBox.SelectedItem as ComboBoxItem;
            ReagentSuppliesType type = (ReagentSuppliesType)selectedItem.DataContext;
            configuration.ItemType = short.Parse(type.TypeId);
            configuration.CalculationFormula = calculation_textBox.Text;
            ComboBoxItem carrier_selectedItem = carrier_comboBox.SelectedItem as ComboBoxItem;
            configuration.ContainerName = carrier_selectedItem.Content.ToString();
            configuration.BarcodePrefix = barcode_textBox.Text;
            configuration.Color = color_Control.Background.ToString();
            bool result = false;

            if (string.IsNullOrEmpty(editedItemId))
            {
                configuration.ItemID = WanTaiObjectService.NewSequentialGuid();
                configuration.ActiveStatus = true;
                result = controller.Create(configuration);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建试剂耗材：" + configuration.EnglishName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);                
            }
            else
            {
                result = controller.EditConfiguration(new Guid(editedItemId), configuration);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改试剂耗材：" + configuration.EnglishName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
            }

            if (result)
            {
                MessageBox.Show("保存成功", "系统提示");
            }
            else
            {
                MessageBox.Show("保存失败", "系统提示");
            }
            this.Close();
        }

        private bool validate()
        {
            if (string.IsNullOrEmpty(name_textBox.Text))
            {
                MessageBox.Show("请输入名称", "系统提示");
                name_textBox.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(displayName_textBox.Text))
            {
                MessageBox.Show("请输入显示名称", "系统提示");
                displayName_textBox.Focus();
                return false;
            }

            ComboBoxItem selectedItem = type_comboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null || string.IsNullOrEmpty(selectedItem.Content.ToString()))
            {
                MessageBox.Show("请选择类型", "系统提示");
                type_comboBox.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(barcode_textBox.Text))
            {
                MessageBox.Show("请输入条码(Barcode)前缀", "系统提示");
                barcode_textBox.Focus();
                return false;
            }

            ComboBoxItem carrier_selectedItem = carrier_comboBox.SelectedItem as ComboBoxItem;
            if (carrier_selectedItem == null || string.IsNullOrEmpty(carrier_selectedItem.Content.ToString()))
            {
                MessageBox.Show("请选择Carrier名称", "系统提示");
                carrier_selectedItem.Focus();
                return false;
            }

            int result = 0;
            if (string.IsNullOrEmpty(grid_textBox.Text) || !int.TryParse(grid_textBox.Text, out result))
            {
                MessageBox.Show("Grid必须为整数", "系统提示");
                grid_textBox.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(position_textBox.Text) || !int.TryParse(position_textBox.Text, out result))
            {
                MessageBox.Show("Position必须为整数", "系统提示");
                position_textBox.Focus();
                return false;
            }

            if (!string.IsNullOrEmpty(calculation_textBox.Text) && !controller.IsFormulaRegula(calculation_textBox.Text))
            {
                MessageBox.Show("检测公式不合法，请重新输入", "系统提示");
                calculation_textBox.Focus();
                return false;
            }

            return true;
        }
    }
}
