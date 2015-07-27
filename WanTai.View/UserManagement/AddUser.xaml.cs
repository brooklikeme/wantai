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
using WanTai.Controller.Configuration;
using WanTai.DataModel;
using WanTai.Controller;

namespace WanTai.View.UserManagement
{
    /// <summary>
    /// Interaction logic for AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        private UserInfoController userInfoController;
        public AddUser()
        {
            InitializeComponent();
            userInfoController = new UserInfoController();

            comboRole.ItemsSource = new RoleInfoController().GetAll();
            comboRole.DisplayMemberPath = "RoleName";
            comboRole.SelectedValuePath = "RoleName";
            comboRole.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtUserName.Text.Trim();
            string password = txtPassword.Password.Trim();
            if (userName == string.Empty)
            {
                MessageBox.Show("用户名不允许为空", "系统提示");
                return;
            }
            if (password == string.Empty)
            {
                MessageBox.Show("密码不允许为空", "系统提示");
                return;
            }
            if (System.Text.Encoding.Default.GetByteCount(userName) > 255)
            {
                MessageBox.Show("用户名最大长度为127个汉字。", "系统提示");
                return;
            }
            
            
            string passwordTwo = txtPasswordTwo.Password.Trim();
            if (System.Text.Encoding.Default.GetByteCount(password)>255)
            {
                MessageBox.Show("密码最大长度为127个汉字。", "系统提示");
                return;
            }
            if (password == passwordTwo)
            {
                if (userInfoController.GetUserByName(userName) != null)
                {
                    MessageBox.Show("用户名[" + userName + "]已经存在。", "系统提示");
                }
                else
                {
                    UserInfo userInfo = new UserInfo()
                    {
                        UserID = WanTaiObjectService.NewSequentialGuid(),
                        CreateName = SessionInfo.LoginName,
                        LoginName = userName,
                        LoginPassWord = password,
                        CreateTime = DateTime.Now,
                        RoleName = comboRole.SelectedValue.ToString()
                    };
                    string message;
                    userInfoController.Add(userInfo, out message);
                    LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "添加用户 成功", SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                    MessageBox.Show(message, "系统提示");
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show("两次输入密码不一致！请重新输入。", "系统提示");
            }
        }
    }
}
