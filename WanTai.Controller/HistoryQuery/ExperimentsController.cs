using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;

using WanTai.DataModel;
using WanTai.Common;
using WanTai.Controller.Configuration;

namespace WanTai.Controller.HistoryQuery
{
    public class ExperimentsController
    {
        public int GetExperimentsTotalCount(string experimentName, string experimentDate, string userName)
        {
            int count = 0;
            
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                ///todo: add control,each role user only can see the specific experiments
                string commandText = "SELECT count(ExperimentID) FROM ExperimentsInfo";
                StringBuilder sbWhere = new StringBuilder();
                if (!string.IsNullOrEmpty(experimentName))
                {
                    string experimentName2 = experimentName.Replace("'", "''").Replace("[","[[]");
                    sbWhere.Append(" WHERE ExperimentName like '%" + experimentName2 + "%'");
                }

                if (!string.IsNullOrEmpty(experimentDate))
                {
                    sbWhere.Append(sbWhere.Length > 0 ? " AND " : " WHERE ");
                    sbWhere.Append(" StartTime between CONVERT(datetime,'" + experimentDate + "',101) and DATEADD(dd, 1, CONVERT(datetime,'" + experimentDate + "',101))");
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    sbWhere.Append(sbWhere.Length > 0 ? " AND " : " WHERE ");
                    sbWhere.Append(" LoginName='"+userName+"'");
                }

//                UserInfoController userInfoController = new UserInfoController();
//                userInfoController.GetRoleByUserName(SessionInfo.LoginName);
//                RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);                
//                sbWhere.Append(sbWhere.Length > 0 ? " AND " : " WHERE ");
//                if (userInfoController.GetAuthorize(AccessAuthority.ExperimentHistory) == AccessAuthority.Self)
//                {
//                    sbWhere.Append(" LoginName='" + SessionInfo.LoginName + "'");
//                }
//                else
//                    sbWhere.Append(@"  1=1 ");
////                {
////                    sbWhere.Append(@" (LoginName is null or LoginName in 
////                    (select LoginName from UserInfo u left join roleinfo r on r.RoleName=u.RoleName where r.RoleLevel<=" + userRole.RoleLevel+ " ))");
////                }
                commandText += sbWhere.ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            if (reader.Read())
                            {
                                count = (int)reader.GetValue(0);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetExperimentsTotalCount", SessionInfo.ExperimentID);
                throw;
            }

            return count;
        }

        public List<ExperimentsInfo> GetNextExperiments(string experimentName, string experimentDate, string userName, int startRowIndex, int rowNumber, string sortColumnName, string sortDirection)
        {
            List<ExperimentsInfo> recordList = new List<ExperimentsInfo>();
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                ///todo: add control,each role user only can see the specific experiments
                string commandText = "SELECT * FROM ( SELECT ROW_NUMBER() OVER (ORDER BY @orderBy) AS [ROW_NUMBER], [t0].*"
                    + " FROM [dbo].[ExperimentsInfo] AS [t0] {0}) AS [t1] WHERE [t1].[ROW_NUMBER] BETWEEN @startIndex AND @endIndex"
                    + " ORDER BY [t1].[ROW_NUMBER]";

                string defaulOrderBy = "StartTime desc";
                if (!string.IsNullOrEmpty(sortColumnName) && !string.IsNullOrEmpty(sortDirection))
                {
                    defaulOrderBy = sortColumnName + " " + sortDirection;
                }

                commandText = commandText.Replace("@orderBy", defaulOrderBy);


                StringBuilder sbWhere = new StringBuilder();
                if (!string.IsNullOrEmpty(experimentName))
                {
                    string experimentName2 = experimentName.Replace("'", "''").Replace("[", "[[]");
                    sbWhere.Append(" WHERE [t0].ExperimentName like '%" + experimentName2 + "%'");
                }

                if (!string.IsNullOrEmpty(experimentDate))
                {
                    sbWhere.Append(sbWhere.Length > 0 ? " AND " : " WHERE ");
                    sbWhere.Append(" StartTime between CONVERT(datetime,'" + experimentDate + "',101) and DATEADD(dd, 1, CONVERT(datetime,'" + experimentDate + "',101))");
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    sbWhere.Append(sbWhere.Length > 0 ? " AND " : " WHERE ");
                    sbWhere.Append(" LoginName='" + userName + "'");
                }

//                UserInfoController userInfoController = new UserInfoController();
//                userInfoController.GetRoleByUserName(SessionInfo.LoginName);
//                RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);
//                sbWhere.Append(sbWhere.Length > 0 ? " AND " : " WHERE ");
//                if (userInfoController.GetAuthorize(AccessAuthority.ExperimentHistory) == AccessAuthority.Self)
//                {
//                    sbWhere.Append(" LoginName='" + SessionInfo.LoginName + "'");
//                }
//                else
//                       sbWhere.Append(@"  1=1 ");
////                {
////                    sbWhere.Append(@" (LoginName is null or LoginName in 
////                    (select LoginName from UserInfo u left join roleinfo r on r.RoleName=u.RoleName where r.RoleLevel<=" + userRole.RoleLevel + " ))");
////                }

                commandText = String.Format(commandText, sbWhere.ToString());

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.Parameters.AddWithValue("@startIndex", startRowIndex);
                        cmd.Parameters.AddWithValue("@endIndex", startRowIndex + rowNumber - 1);

                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read())
                            {
                                ExperimentsInfo info = new ExperimentsInfo();
                                info.ExperimentID = (Guid)reader.GetValue(1);
                                info.ExperimentName = reader.GetValue(2).ToString();
                                info.StartTime = (DateTime)reader.GetValue(3);
                                if (reader.GetValue(4) != DBNull.Value)
                                {
                                    info.EndTime = (DateTime)reader.GetValue(4);
                                }

                                info.LoginName = reader.GetValue(5).ToString();
                                if (reader.GetValue(6) != DBNull.Value)
                                {
                                    info.Remark = reader.GetValue(6).ToString();
                                }

                                info.State = (short)reader.GetValue(7);
                                if (reader.GetValue(8) != DBNull.Value)
                                {
                                    info.MixTimes = (short)reader.GetValue(8);
                                }
                                recordList.Add(info);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetNextExperiments", SessionInfo.ExperimentID);
                throw;
            }

            return recordList;
        }

        public bool Delete(Guid experimentId)
        {
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                ///todo: add control,each role user only can see the specific experiments
                string commandText = "Delete_Experiment";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@experimentId", experimentId);

                        cmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->Delete", SessionInfo.ExperimentID);
                return false;
            }
        }

        public ExperimentsInfo GetExperimentById(Guid experimentId)
        {
            using (WanTaiEntities entities = new WanTaiEntities())
            {
                ExperimentsInfo experimentInfo = entities.ExperimentsInfoes.FirstOrDefault(p => p.ExperimentID == experimentId);
                return experimentInfo;
            }
        }

        public string ConvertEnumStatusToText(ExperimentStatus status)
        {
            if (status == ExperimentStatus.Create)
                return "新建";
            else if (status == ExperimentStatus.Fail)
                return "失败";
            else if (status == ExperimentStatus.Finish)
                return "完成";
            else if (status == ExperimentStatus.Processing)
                return "运行";
            else if (status == ExperimentStatus.Stop)
                return "停止";
            else if (status == ExperimentStatus.Suspend)
                return "暂停";
            else return null;
        }
    }
}
