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
using System.Data;
using WanTai.Controller.Configuration;
using WanTai.DataModel;
using WanTai.Controller;
using System.Reflection;

namespace WanTai.View.UserManagement
{
    /// <summary>
    /// Interaction logic for UserList.xaml
    /// </summary>
    public partial class UserList : Window
    {
        UserInfoController userInfoController;
        RoleInfoController roleInfoController;
        DataTable dtUsers;
        List<UserInfo> usersList;
        public UserList()
        {
            InitializeComponent();
            roleInfoController = new RoleInfoController();
            userInfoController = new UserInfoController();
            comRole.ItemsSource = roleInfoController.GetAll();
            dtUsers = new DataTable();
            dtUsers.Columns.Add("Index", typeof(int));
            dtUsers.Columns.Add("UserID", typeof(Guid));
            dtUsers.Columns.Add("LoginName", typeof(string));
            dtUsers.Columns.Add("LoginPassWord", typeof(string));
            dtUsers.Columns.Add("RoleName", typeof(string));
            BindUserInfo();
        }

        private void BindUserInfo()
        {
            usersList = userInfoController.GetAll();
            dtUsers.Rows.Clear();
            int i = 1;
            foreach (UserInfo user in usersList)
            {
                dtUsers.Rows.Add(i,user.UserID, user.LoginName, user.LoginPassWord, user.RoleName);
                i++;
            }            
            dgUsers.DataContext = dtUsers;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            string loginName = (string)(((DataRowView)dgUsers.CurrentItem)["LoginName"]);
            if (loginName == SessionInfo.LoginName)
            {
                MessageBox.Show("无法删除正在登陆的用户，请退出后再删除", "系统提示");
                return;
            }
            MessageBoxResult msResult = MessageBox.Show("确认要删除用户吗？", "系统提示", MessageBoxButton.YesNo);
            if (msResult == MessageBoxResult.Yes)
            {
                string message;
                Guid userID = (Guid)(((DataRowView)dgUsers.CurrentItem)["UserID"]);
                userInfoController.Delete(userID, out message);
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate,"删除用户 "+ message, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
                MessageBox.Show(message, "系统提示");
                BindUserInfo();
            }
        }

        private void comRole_selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            string message;
            DataRowView rowView = (DataRowView)dgUsers.SelectedItem;
            UserInfo user = usersList.FirstOrDefault(P=>P.UserID==(Guid)rowView["UserID"]);
            user.LoginPassWord = rowView["LoginPassWord"].ToString();
            if(user.LoginPassWord==string.Empty)
            {
                MessageBox.Show("密码不允许为空！", "系统提示");
                return;
            }
            user.RoleName = ((ComboBox)dgUsers.Columns[3].GetCellContent(rowView)).SelectedValue.ToString();
            user.UpdateTime = DateTime.Now;
            userInfoController.Update(user,out message);
            LogInfoController.AddLogInfo(LogInfoLevelEnum.Operate, "修改用户 " + message, SessionInfo.LoginName, this.GetType().Name, SessionInfo.ExperimentID);
            MessageBox.Show(message, "系统提示");
        }
    }

    public static class PasswordBoxBindingHelper
    {
        public static bool GetIsPasswordBindingEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPasswordBindingEnabledProperty);
        }

        public static void SetIsPasswordBindingEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPasswordBindingEnabledProperty, value);
        }

        public static readonly DependencyProperty IsPasswordBindingEnabledProperty =
        DependencyProperty.RegisterAttached("IsPasswordBindingEnabled", typeof(bool),
            typeof(PasswordBoxBindingHelper), new UIPropertyMetadata(false, OnIsPasswordBindingEnabledChanged));

        private static void OnIsPasswordBindingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = obj as
            PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.PasswordChanged -= PasswordBoxPasswordChanged;
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBoxPasswordChanged;
                }
            }
        }

        //when the passwordBox's password 
        //changed, update the buffer  
        static void PasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (!String.Equals(GetBindedPassword(passwordBox), passwordBox.Password))
            {
                SetBindedPassword(passwordBox, passwordBox.Password);
            }
        }

        public static string GetBindedPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(BindedPasswordProperty);
        }

        public static void SetBindedPassword(DependencyObject obj, string value)
        {
            obj.SetValue(BindedPasswordProperty, value);
        }

        public static readonly DependencyProperty BindedPasswordProperty = DependencyProperty.RegisterAttached
        ("BindedPassword", typeof(string), typeof(PasswordBoxBindingHelper), new UIPropertyMetadata(string.Empty, OnBindedPasswordChanged));

        //when the buffer changed, upate the passwordBox's password  
        private static void OnBindedPasswordChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = obj as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.Password = e.NewValue == null ? string.Empty : e.NewValue.ToString();
                SetPasswordBoxSelection(passwordBox, passwordBox.Password.Length, 0);
            }
        }

        private static void SetPasswordBoxSelection(PasswordBox passwordBox, int start, int length)
        {
            var select = passwordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic);

            select.Invoke(passwordBox, new object[] { start, length });
        }
    }
}
