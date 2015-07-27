using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;
using System.Data;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace WanTai.Controller.Configuration
{
    public class UserInfoController
    {
        private static string DeleteSuccess = "删除成功！";
        private static string AddSuccess = "添加成功！";
        private static string UpdateSuccess = "修改成功！";
        private static string failed = "操作失败！";

        /// <summary>
        /// Get user and role
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public RoleInfo GetRoleByUserName(string userName)
        {
            RoleInfo roleInfo = new RoleInfo();
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                UserInfo userInfo = _WanTaiEntities.UserInfoes.FirstOrDefault(P => P.LoginName == userName);
                if (userInfo != null)
                {
                    roleInfo = _WanTaiEntities.RoleInfoes.FirstOrDefault(P => P.RoleName == userInfo.RoleName);
                }
                return roleInfo;
            }
        }

        public void Add(UserInfo userInfo, out string message)
        {
            message = string.Empty;
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                _WanTaiEntities.UserInfoes.AddObject(userInfo);
                _WanTaiEntities.SaveChanges();
                message = AddSuccess;
            }
        }

        public void Update(UserInfo userInfo, out string message)
        {
            message = string.Empty;
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                UserInfo formerUser = _WanTaiEntities.UserInfoes.FirstOrDefault(P => P.UserID == userInfo.UserID);
                if (formerUser != null)
                {
                    formerUser.LoginName = userInfo.LoginName;
                    formerUser.LoginPassWord = userInfo.LoginPassWord;
                    formerUser.RoleName = userInfo.RoleName;
                    formerUser.CreateName = userInfo.CreateName;
                    formerUser.UpdateTime = userInfo.UpdateTime;
                    _WanTaiEntities.SaveChanges();
                    message = UpdateSuccess;
                }
            }
        }

        public void Delete(Guid userID, out string message)
        {
            message = string.Empty;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = Common.Configuration.GetConnectionString();
                conn.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = conn;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "DeleteUserInfo";
                SqlParameter parameter = new SqlParameter("userID", userID);
                command.Parameters.Add(parameter);
                command.ExecuteNonQuery();
                conn.Close();
            }
            message = DeleteSuccess;
        }

        public UserInfo GetUserByName(string userName)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                UserInfo userInfo = _WanTaiEntities.UserInfoes.FirstOrDefault(P => P.LoginName == userName);
                return userInfo;
            }
        }

        public List<UserInfo> GetAll()
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                UserInfo currentuser = _WanTaiEntities.UserInfoes.FirstOrDefault(P => P.LoginName == SessionInfo.LoginName);
                RoleInfo currentrole = _WanTaiEntities.RoleInfoes.FirstOrDefault(P => P.RoleName == currentuser.RoleName);
                List<UserInfo> users = _WanTaiEntities.UserInfoes.ToList();
                List<RoleInfo> roles = _WanTaiEntities.RoleInfoes.ToList();
                var qurey = from user in users
                            join role in roles on user.RoleName equals role.RoleName
                            where role.RoleLevel <= currentrole.RoleLevel
                            select user;
                return qurey.ToList();
            }
        }

        public List<UserInfo> GetSubUser(string userName)
        {
            using (WanTaiEntities _WanTaiEntities = new WanTaiEntities())
            {
                List<UserInfo> users = _WanTaiEntities.UserInfoes.Where(P => P.CreateName == userName).ToList();
                return users;
            }
        }

        public DataSet GetMenuAuthorize()
        {
            RoleInfo userInfo = new UserInfoController().GetRoleByUserName(SessionInfo.LoginName);
            StringReader reader = new StringReader(userInfo.RoleModules);
            DataSet dataset = new DataSet();
            DataTable dataTable = new DataTable("Menu");
            DataColumn column1 = new DataColumn("menuName", typeof(string));
            column1.ColumnMapping = MappingType.Attribute;

            DataColumn column2 = new DataColumn("access", typeof(string));
            column2.ColumnMapping = MappingType.Attribute;
            dataTable.Columns.Add(column1);
            dataTable.Columns.Add(column2);
            dataset.Tables.Add(dataTable);
            dataset.ReadXml(reader, XmlReadMode.Fragment);
            reader.Close();
            return dataset;
        }

        public DataSet GetDataAuthorize()
        {
            RoleInfo userInfo = new UserInfoController().GetRoleByUserName(SessionInfo.LoginName);
            StringReader reader = new StringReader(userInfo.RoleModules);
            DataSet dataset = new DataSet();
            DataTable dataTable = new DataTable("authority");
            DataColumn column1 = new DataColumn("dataName", typeof(string));
            column1.ColumnMapping = MappingType.Attribute;

            DataColumn column2 = new DataColumn("access", typeof(string));
            column2.ColumnMapping = MappingType.Attribute;
            dataTable.Columns.Add(column1);
            dataTable.Columns.Add(column2);
            dataset.Tables.Add(dataTable);
            dataset.ReadXml(reader, XmlReadMode.Fragment);
            reader.Close();
            return dataset;
        }

        public string GetAuthorize(string dataName)
        {
            string access = AccessAuthority.All;
            DataSet ds = GetDataAuthorize();
            DataRow[] dr = ds.Tables[0].Select("dataName='" + dataName + "'");

            if (dr.Length > 0)
            {
                access = dr[0]["access"].ToString();
            }
            return access;
        }
    }
}
