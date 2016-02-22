﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.ServiceProcess;
using System.Net;
using Microsoft.Win32;
namespace WanTai.CustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult FillDBServerNameAction(Session xiSession)
        {
            Microsoft.Deployment.WindowsInstaller.View lView = xiSession.Database.OpenView("SELECT * FROM ComboBox");
            lView.Execute();
             int lIndex = 1;
            string HostName = Dns.GetHostName();
            ServiceController[] services = ServiceController.GetServices();
            //从机器服务列表中找到本机的SqlServer引擎
            foreach (ServiceController s in services)
            {
                if (s.Status != ServiceControllerStatus.Running) continue;
                if (s.ServiceName.ToLower().IndexOf("mssql$") != -1)
                {
                    Record lRecord = xiSession.Database.CreateRecord(4);
                    lRecord.SetString(1, "DBSERVER");
                    lRecord.SetInteger(2, lIndex);
                    lRecord.SetString(3, HostName + "\\" + s.ServiceName.Substring(s.ServiceName.IndexOf("$") + 1)); // Use lWebsiteName only if you want to look up the site by name.
                    lRecord.SetString(3, HostName + "\\" + s.ServiceName.Substring(s.ServiceName.IndexOf("$") + 1));
                    lView.Modify(ViewModifyMode.InsertTemporary, lRecord);
                    if (lIndex == 1)
                        xiSession["DBSERVER"] = HostName + "\\" + s.ServiceName.Substring(s.ServiceName.IndexOf("$") + 1);
                    lIndex++;
                }
                else if (s.ServiceName.ToLower() == "mssqlserver")
                {
                    Record lRecord = xiSession.Database.CreateRecord(4);
                    lRecord.SetString(1, "DBSERVER");
                    lRecord.SetInteger(2, lIndex);
                    lRecord.SetString(3, HostName); // Use lWebsiteName only if you want to look up the site by name.
                    lRecord.SetString(3, HostName);
                    lView.Modify(ViewModifyMode.InsertTemporary, lRecord);
                    if (lIndex == 1)
                        xiSession["DBSERVER"] = HostName ;
                    lIndex++;
                }
            }           
            lView.Close();
            return ActionResult.Success;           
        }
        [CustomAction]
        public static ActionResult ConnectDBAction1(Session session)
        {
            MessageBox.Show(session["CONNECTSUCCESS"], "系统提示");
            return ActionResult.Success;
        }
        [CustomAction]
        public static ActionResult ConnectDBAction(Session session)
        {
            ///AUTHENTICATION
            //  <ListItem Text="Windows认证" Value="0" />
            //<ListItem Text="Sql Server认证" Value="1" />
            if (session["DBPOSITION"] == "0" && string.IsNullOrEmpty(session["DBSERVER"]))
                session["CONNECTSUCCESS"] = "-3";
            else if (session["DBPOSITION"] == "1" && string.IsNullOrEmpty(session["REMOTEDBSERVER"]))
                session["CONNECTSUCCESS"] = "-4";
            else if (string.IsNullOrEmpty(session["DBNAME"]))
                session["CONNECTSUCCESS"] = "-2";
            else
            {
                string connectionStr;
                if (session["DBPOSITION"] == "0")
                {
                    connectionStr = session["AUTHENTICATION"] == "0" ? session["CONNECTIONSTRINGB"] : session["CONNECTIONSTRINGA"];
                }
                else
                {
                    connectionStr = session["AUTHENTICATION"] == "0" ? session["REMOTECONNECTIONSTRINGB"] : session["REMOTECONNECTIONSTRINGA"];
                }
                // 连接数据库方法 //
                using (SqlConnection con = new SqlConnection())
                {
                    con.ConnectionString = connectionStr;
                    try
                    {
                        MessageBox.Show(con.ConnectionString);
                        con.Open();
                        SqlCommand cmd = new SqlCommand("select * from sysdatabases where name='" + session["DBNAME"] + "'");
                        cmd.Connection = con;
                        if (cmd.ExecuteScalar() != null)
                        {
                            if (session["DBPOSITION"] == "0")
                            {
                                session["CONNECTSUCCESS"] = "-1";
                                if (session["AUTHENTICATION"] == "0")
                                {
                                    session["CONNECTIONSTRING"] = "Data Source=" + session["DBSERVER"] + ";Initial Catalog=" + session["DBNAME"] + ";Integrated Security=True;";
                                    session["CONNECTIONSTRINGKINGFISHER"] = "Data Source=" + session["DBSERVER"].Substring(0, session["DBSERVER"].IndexOf("\\")) + "\\THERMO;Initial Catalog=BindIt_KF_RE;Integrated Security=True;";
                                }
                                else
                                {
                                    session["CONNECTIONSTRING"] = "data source=" + session["DBSERVER"] + ";user=" + session["DBUSERNAME"] + ";password=" + session["DBPASSWORD"] + ";initial catalog=" + session["DBNAME"] + ";Persist Security Info=;";
                                    session["CONNECTIONSTRINGKINGFISHER"] = "data source=" + session["DBSERVER"].Substring(0, session["DBSERVER"].IndexOf("\\")) + "\\THERMO;user=" + session["DBUSERNAME"] + ";password=" + session["DBPASSWORD"] + ";initial catalog=BindIt_KF_RE;Persist Security Info=;";
                                }
                            }
                            else
                            {
                                session["CONNECTSUCCESS"] = "1";
                                if (session["AUTHENTICATION"] == "0")
                                {
                                    session["CONNECTIONSTRING"] = "Data Source=" + session["REMOTEDBSERVER"] + "\\SQLEXPRESS;Initial Catalog=" + session["DBNAME"] + ";Integrated Security=True;";
                                    session["CONNECTIONSTRINGKINGFISHER"] = "Data Source=" + session["REMOTEDBSERVER"] + "\\THERMO;Initial Catalog=BindIt_KF_RE;Integrated Security=True;";
                                }
                                else
                                {
                                    session["CONNECTIONSTRING"] = "data source=" + session["REMOTEDBSERVER"] + "\\SQLEXPRESS;user=" + session["DBUSERNAME"] + ";password=" + session["DBPASSWORD"] + ";initial catalog=" + session["DBNAME"] + ";Persist Security Info=;";
                                    session["CONNECTIONSTRINGKINGFISHER"] = "data source=" + session["REMOTEDBSERVER"] + "\\THERMO;user=" + session["DBUSERNAME"] + ";password=" + session["DBPASSWORD"] + ";initial catalog=BindIt_KF_RE;Persist Security Info=;";
                                }
                            }
                        }
                        else
                        {
                            if (session["DBPOSITION"] == "0")
                            {
                                if (session["AUTHENTICATION"] == "0") {
                                    session["CONNECTIONSTRING"] = "Data Source=" + session["DBSERVER"] + ";Initial Catalog=" + session["DBNAME"] + ";Integrated Security=True;";
                                    session["CONNECTIONSTRINGKINGFISHER"] = "Data Source=" + session["DBSERVER"].Substring(0, session["DBSERVER"].IndexOf("\\")) + "\\THERMO;Initial Catalog=BindIt_KF_RE;Integrated Security=True;";
                                }
                                else
                                {
                                    session["CONNECTIONSTRING"] = "data source=" + session["DBSERVER"] + ";user=" + session["DBUSERNAME"] + ";password=" + session["DBPASSWORD"] + ";initial catalog=" + session["DBNAME"] + ";Persist Security Info=;";
                                    session["CONNECTIONSTRINGKINGFISHER"] = "data source=" + session["DBSERVER"].Substring(0, session["DBSERVER"].IndexOf("\\")) + "\\THERMO;user=" + session["DBUSERNAME"] + ";password=" + session["DBPASSWORD"] + ";initial catalog=BindIt_KF_RE;Persist Security Info=;";
                                }                      
                                session["CONNECTSUCCESS"] = "1";
                            }
                            else
                            {
                                session["CONNECTSUCCESS"] = "-5";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        session["CONNECTSUCCESS"] = "0";
                    }
                    catch
                    {
                        session["CONNECTSUCCESS"] = "0";
                    }
                }
            }
            return ActionResult.Success;
        }
        [CustomAction]
        public static ActionResult InstallDBAction(Session session)
        {
            RegistryKey softWareKey = Registry.LocalMachine.OpenSubKey("Software\\Acme Ltd.\\Foobar 1.0");
            if (softWareKey != null)
            {
                object bj = softWareKey.GetValue("SomeEXEPath");
                MessageBox.Show(bj.ToString());
            }


            return ActionResult.Success;
        }
    }
}
