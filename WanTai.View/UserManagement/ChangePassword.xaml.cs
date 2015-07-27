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
using WanTai.Controller.Configuration;
using WanTai.Controller;

namespace WanTai.View.UserManagement
{
    /// <summary>
    /// Interaction logic for ChangePassword.xaml
    /// </summary>
    public partial class ChangePassword : Window
    {
        UserInfoController userInfoController;
        public ChangePassword()
        {
            InitializeComponent();

            userInfoController = new UserInfoController();
            txtUserName.Text = SessionInfo.LoginName;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            UserInfo user = userInfoController.GetUserByName(SessionInfo.LoginName);
            string prePassword = txtPrePassword.Password.Trim();
            string password = txtPassword.Password.Trim();
            string passwordTwo = txtPasswordTwo.Password.Trim();
            if (password == string.Empty)
            {
                MessageBox.Show("密码不能为空！", "系统提示");
                return;
            }
            if (prePassword != user.LoginPassWord)
            {
                MessageBox.Show("旧密码输入错误。", "系统提示");
                return;
            }

            if (password != passwordTwo)
            {
                MessageBox.Show("确认密码错误。", "系统提示");
                return;
            }
            user.LoginPassWord = password;
            user.UpdateTime = DateTime.Now;
            string message;
            userInfoController.Update(user, out message);
            LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改密码 " + message, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
            MessageBox.Show(message, "系统提示");
            this.Close();
        }
    }
}
