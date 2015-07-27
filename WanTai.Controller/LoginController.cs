using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using WanTai.DataModel;
using System.Data;
using System.Data.SqlClient;
namespace WanTai.Controller
{
    public class LoginController
    {
        public UserInfo CheckLoginUser(string LoginName, string LoginPassWord,out string ErrMsg)
        {
            ErrMsg = "操作成功！";
            using(SqlConnection con=new SqlConnection(WanTai.Common.Configuration.GetConnectionString()))
            {
        
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "select * from UserInfo where LoginName='" + LoginName+"'";
                cmd.Connection = con;
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                UserInfo LoginUser = new UserInfo();
                if (dr.Read())
                {
                    if (dr["LoginName"].ToString() != LoginName)
                    {
                        ErrMsg = "用户[" + LoginName + "]不存在！";
                        return null;
                    }
                    if (dr["LoginPassWord"].ToString() != LoginPassWord)
                    {
                        ErrMsg = "密码错误！";
                        return null;
                    }
                    LoginUser.CreateName = dr["CreateName"].ToString();
                    LoginUser.CreateTime = (DateTime.Parse(dr["CreateTime"].ToString()));
                    LoginUser.LoginName = dr["LoginName"].ToString();
                    LoginUser.LoginPassWord = dr["LoginPassWord"].ToString();
                    LoginUser.RoleName = dr["RoleName"].ToString();
                    LoginUser.UserID = new Guid(dr["UserID"].ToString());
                    if (!string.IsNullOrEmpty(dr["UpdateTime"].ToString()))
                        LoginUser.UpdateTime = DateTime.Parse(dr["UpdateTime"].ToString());
                    return LoginUser;
                }
                else
                {
                    ErrMsg = "用户[" + LoginName + "]不存在！";
                    return null;
                }
            }
           
            //using (WanTaiEntities _WanTaiEntities=new WanTaiEntities())
            //{
            //    UserInfo LoginUser= _WanTaiEntities.UserInfoes.Where(user => user.LoginName == LoginName).DefaultIfEmpty().First();
            //   if (LoginUser == null)
            //   {
            //       ErrMsg = "用户[" + LoginName + "]不存在！";
            //       return null;
            //   }
            //   if (LoginUser.LoginPassWord != LoginPassWord)
            //   {
            //       ErrMsg = "密码[" + LoginPassWord + "]错误！";
            //       return null;
            //   }
            //   return LoginUser;
            //}
        }
    }
}