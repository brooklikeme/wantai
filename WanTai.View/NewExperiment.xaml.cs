using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WanTai.DataModel;
using WanTai.Controller;

namespace WanTai.View
{
    /// <summary>
    /// Interaction logic for NewExperiment.xaml
    /// </summary>
    public partial class NewExperiment : Window
    {

        private List<Guid> TestingItemList = new List<Guid>();

        public NewExperiment()
        {
            InitializeComponent();
            if(SessionInfo.LoginName!=null)
                txtOrperatorName.Text = SessionInfo.LoginName;
            txtExperimentName.Text = System.DateTime.Now.ToString();

            if (SessionInfo.WorkDeskType == "100")
            {
                Label label = new Label();
                label.Content = "检测项目:";
                testPanel.Children.Add(label);

                var TestItems = new TestItemController().GetActiveTestItemConfigurations();
                foreach (TestingItemConfiguration _TestingItem in TestItems)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Tag = _TestingItem;
                    checkBox.Content = _TestingItem.TestingItemName;
                    checkBox.Height = 20;
                    checkBox.Width = 50;
                    checkBox.Margin = new Thickness(5);
                    checkBox.Checked += new RoutedEventHandler(checkBox_Checked);
                    checkBox.Unchecked += new RoutedEventHandler(checkBox_Unchecked);
                    testPanel.Children.Add(checkBox);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string experimentName = txtExperimentName.Text.Trim();
            if (experimentName==string.Empty) 
            {
                errInfo.Text = "实验名称不能为空。";
                return;
            }
            else if(System.Text.Encoding.Default.GetByteCount(experimentName)>255)
            {
                errInfo.Text = "实验名称允许最大长度为127个汉字";
                return;
            }

            if (SessionInfo.WorkDeskType == "100" && TestingItemList.Count <= 0)
            {
                errInfo.Text = "请至少选择一项检测项";
                return;
            }

            ExperimentController controller = new ExperimentController();
            if(controller.ExperimentNameExists(experimentName))
            {
                errInfo.Text = "实验名称\"" + txtExperimentName.Text + "\"已存在。";
                return;
            }
            ExperimentsInfo experimentsInfo = new ExperimentsInfo();
            experimentsInfo.ExperimentID = WanTaiObjectService.NewSequentialGuid();
            experimentsInfo.ExperimentName = experimentName;
            experimentsInfo.LoginName = txtOrperatorName.Text;
            experimentsInfo.Remark = txtRemark.Text;
            experimentsInfo.StartTime = DateTime.Now;
            experimentsInfo.State = (short)ExperimentStatus.Create; ;
            SessionInfo.CurrentExperimentsInfo = experimentsInfo;
            SessionInfo.TestingItemIDs = TestingItemList;
            if (controller.CreateExperiment(experimentsInfo))
            {
                SessionInfo.ExperimentID = experimentsInfo.ExperimentID;
                SessionInfo.RotationFormulaParameters=new Dictionary<Guid,FormulaParameters>();
                SessionInfo.PraperRotation = null;
                SessionInfo.BatchType = null;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "新建实验 成功", SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                this.DialogResult = true;
                this.Close();
            }
            else 
            {
                errInfo.Text = "添加实验失败。";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            TestingItemConfiguration testItem = (TestingItemConfiguration)((CheckBox)sender).Tag;
            TestingItemList.Add(testItem.TestingItemID);
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            TestingItemConfiguration testItem = (TestingItemConfiguration)((CheckBox)sender).Tag;
            TestingItemList.Remove(testItem.TestingItemID);
        }
        private void txtExperimentName_GotFocus(object sender, RoutedEventArgs e)
        {
            errInfo.Text = "";
        }
    }
}
