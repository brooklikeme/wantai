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

using WanTai.DataModel;
using WanTai.Controller;

namespace WanTai.View.Configuration
{
    /// <summary>
    /// Interaction logic for CreatePoolingRulesConfiguration.xaml
    /// </summary>
    public partial class CreatePoolingRulesConfiguration : Window
    {

        PoolingRulesConfigurationController controller = new PoolingRulesConfigurationController();
        string editPoolingRuleId;
        

        public CreatePoolingRulesConfiguration()
        {
            InitializeComponent();
        }

        public void SetEditedPoolingRuleId(string PoolingRuleId)
        {
            editPoolingRuleId = PoolingRuleId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitField();
        }

        private void InitField()
        {
            if (!string.IsNullOrEmpty(editPoolingRuleId))
            {
                PoolingRulesConfiguration config = controller.GetPoolingRule(new Guid(editPoolingRuleId));

                name_textBox.Text = config.PoolingRulesName;
                tubeNumber_textBox.Text = config.TubeNumber.ToString();
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
            {
                return;
            }

            PoolingRulesConfiguration config = new PoolingRulesConfiguration();
            config.PoolingRulesName = name_textBox.Text;
            config.TubeNumber = int.Parse(tubeNumber_textBox.Text);

            bool result = false;
            if (string.IsNullOrEmpty(editPoolingRuleId))
            {
                config.PoolingRulesID = WanTaiObjectService.NewSequentialGuid();
                config.ActiveStatus = true;
                result = controller.Create(config);
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建混样方式：" + config.PoolingRulesName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
            }
            else
            {
                result = controller.EditPoolingRules(new Guid(editPoolingRuleId), config);
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改混样方式：" + config.PoolingRulesName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
            }

            if (result)
            {
                MessageBox.Show("保存成功", "系统提示");
                this.DialogResult = true;
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

            int outnum = 0;
            if (string.IsNullOrEmpty(tubeNumber_textBox.Text) || !int.TryParse(tubeNumber_textBox.Text, out outnum) || int.Parse(tubeNumber_textBox.Text)<=0)
            {
                MessageBox.Show("请填写正确的采血管数量，必须为数字", "系统提示");
                tubeNumber_textBox.Focus();
                return false;
            }

            return true;
        }        

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }        
    }
}
