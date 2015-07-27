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
    /// Interaction logic for CreateTestingItemConfiguration.xaml
    /// </summary>
    public partial class CreateTestingItemConfiguration : Window
    {

        TestItemController controller = new TestItemController();
        string editTestItemId;


        public CreateTestingItemConfiguration()
        {
            InitializeComponent();
        }

        public void SetEditedTestItemId(string TestItemId)
        {
            editTestItemId = TestItemId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitField();
        }

        private void InitField()
        {
            if (!string.IsNullOrEmpty(editTestItemId))
            {
                TestingItemConfiguration config = controller.GetTestingItem(new Guid(editTestItemId));

                name_textBox.Text = config.TestingItemName;
                color_Control.Background = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(config.TestingItemColor) };
                displaySequence_textBox.Text = config.DisplaySequence.ToString();
                workListFileName_textBox.Text = config.WorkListFileName;
            }
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if (!validate())
            {
                return;
            }

            TestingItemConfiguration config = new TestingItemConfiguration();
            config.TestingItemName = name_textBox.Text;
            config.TestingItemColor = color_Control.Background.ToString();
            config.DisplaySequence = short.Parse(displaySequence_textBox.Text);
            config.WorkListFileName = workListFileName_textBox.Text;

            bool result = false;
            if (string.IsNullOrEmpty(editTestItemId))
            {
                config.TestingItemID = WanTaiObjectService.NewSequentialGuid();
                config.ActiveStatus = true;
                result = controller.Create(config);
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建检测项目：" + config.TestingItemName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
            }
            else
            {
                result = controller.EditTestingItems(new Guid(editTestItemId), config);
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改检测项目：" + config.TestingItemName + " " + (result == true ? "成功" : "失败"), SessionInfo.LoginName, this.GetType().ToString(), null);
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
            if (string.IsNullOrEmpty(displaySequence_textBox.Text) || !int.TryParse(displaySequence_textBox.Text, out outnum) || int.Parse(displaySequence_textBox.Text) < 0)
            {
                MessageBox.Show("请填写正确的显示顺序，必须为数字", "系统提示");
                displaySequence_textBox.Focus();
                return false;
            }

            return true;
        }        

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void color_Control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            string color = color_Control.Background.ToString();
            colorDialog.Color = System.Drawing.ColorTranslator.FromHtml(color);
            colorDialog.ShowDialog();
            color = System.Drawing.ColorTranslator.ToHtml(colorDialog.Color);
            color_Control.Background = new SolidColorBrush() { Color = (Color)ColorConverter.ConvertFromString(color) };
        }
    }
}
