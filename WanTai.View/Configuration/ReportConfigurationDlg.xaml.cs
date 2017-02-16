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
    /// Interaction logic for ReportConfigurationDlg.xaml
    /// </summary>
    public partial class ReportConfigurationDlg : Window
    {
        ReportConfigurationController controller = new ReportConfigurationController();
        private string editedItemId;
        public ReportConfigurationDlg()
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
            InitField();
        }

        private void InitField()
        {
            if (!string.IsNullOrEmpty(editedItemId))
            {
                ReportConfiguration item = controller.GetConfiguration(new Guid(editedItemId));
                this.displayName_textBox.Text = item.DisplayName;
                this.position_textBox.Text = item.Position.ToString();
                this.calculation_textBox.Text = item.CalculationFormula;
            }
        }
        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
            {
                return;
            }

            ReportConfiguration configuration = new ReportConfiguration();
            configuration.WorkDeskType = SessionInfo.WorkDeskType;
            configuration.DisplayName = displayName_textBox.Text;
            configuration.Position = int.Parse(position_textBox.Text);
            configuration.CalculationFormula = calculation_textBox.Text;
            bool result = false;

            if (string.IsNullOrEmpty(editedItemId))
            {
                configuration.ItemID = WanTaiObjectService.NewSequentialGuid();
                configuration.ActiveStatus = true;
                result = controller.Create(configuration);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建试剂耗材查询配置：" + configuration.DisplayName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);                
            }
            else
            {
                result = controller.EditConfiguration(new Guid(editedItemId), configuration);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改试剂耗材查询配置：" + configuration.DisplayName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
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
            if (string.IsNullOrEmpty(displayName_textBox.Text))
            {
                MessageBox.Show("请输入名称", "系统提示");
                displayName_textBox.Focus();
                return false;
            }

            int result = 0;
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
