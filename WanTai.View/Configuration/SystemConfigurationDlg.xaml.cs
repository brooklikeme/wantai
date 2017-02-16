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
    /// Interaction logic for SystemConfiguration.xaml
    /// </summary>
    public partial class SystemConfigurationDlg : Window
    {
        SystemConfigurationController controller = new SystemConfigurationController();
        private string editedItemId;
        public SystemConfigurationDlg()
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
                SystemConfiguration item = controller.GetConfiguration(new Guid(editedItemId));
                this.itemName_textBox.Text = item.ItemName;
                this.itemCode_textBox.Text = item.ItemCode;
                this.itemValue_textBox.Text = item.ItemValue;
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
            {
                return;
            }

            SystemConfiguration configuration = new SystemConfiguration();
            configuration.WorkDeskType = SessionInfo.WorkDeskType;
            configuration.ItemCode = itemCode_textBox.Text;
            configuration.ItemName = itemName_textBox.Text;
            configuration.ItemValue = itemValue_textBox.Text;
            bool result = false;

            if (string.IsNullOrEmpty(editedItemId))
            {
                configuration.ItemID = WanTaiObjectService.NewSequentialGuid();
                result = controller.Create(configuration);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建系统参数：" + configuration.ItemName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);                
            }
            else
            {
                result = controller.EditConfiguration(new Guid(editedItemId), configuration);
                WanTai.Controller.LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改系统参数：" + configuration.ItemName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
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
            if (string.IsNullOrEmpty(itemValue_textBox.Text))
            {
                MessageBox.Show("请输入值", "系统提示");
                itemValue_textBox.Focus();
                return false;
            }
            return true;
        }
    }
}
