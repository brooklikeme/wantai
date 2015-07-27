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
    public class LogViewController
    {
        public int GetTotalCount(string experimentName, LogInfoLevelEnum logLevel)
        {
            int count = 0;
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                ///todo: add control,each role user only can see the specific experiments
                string commandText = "SELECT count(LogID) FROM LogInfo left join ExperimentsInfo on LogInfo.ExperimentID = ExperimentsInfo.ExperimentID";
                commandText = commandText + " WHERE LogInfo.LogLevel = " + (int)logLevel;
                if (!string.IsNullOrEmpty(experimentName))
                {
                    commandText = commandText + " AND ExperimentsInfo.ExperimentName like '%" + experimentName + "%'";
                }
                UserInfoController userInfoController = new UserInfoController();
                RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);
                if (userInfoController.GetAuthorize(AccessAuthority.LogInfo) == AccessAuthority.All)
                {
                    commandText = commandText + @" AND (LogInfo.LoginName IS NULL or LogInfo.LoginName in 
                    (SELECT LoginName FROM UserInfo u left join roleinfo r on r.RoleName=u.RoleName WHERE r.RoleLevel<=" + userRole.RoleLevel + " ))";
                }
                else
                {
                    commandText = commandText + " AND LogInfo.LoginName='" + SessionInfo.LoginName + "'";
                }
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
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetTotalCount", SessionInfo.ExperimentID);
                throw;
            }

            return count;
        }

        public void GetNextLogs(string experimentName, int startRowIndex, int rowNumber, LogInfoLevelEnum logLevel, DataTable dataTable, System.Windows.Media.Color alternativeColor, System.Windows.Media.Color defaultColor)
        {
            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                ///todo: add control,each role user only can see the specific experiments
                string commandText = "SELECT * FROM ( SELECT ROW_NUMBER() OVER (ORDER BY CreaterTime desc) AS [ROW_NUMBER], [t0].*, ExperimentsInfo.ExperimentName"
                    + " FROM [dbo].[LogInfo] AS [t0] left join ExperimentsInfo on [t0].ExperimentID = ExperimentsInfo.ExperimentID "
                    + " WHERE [t0].LogLevel= " + (int)logLevel + " {0}) AS [t1] WHERE [t1].[ROW_NUMBER] BETWEEN @startIndex AND @endIndex"
                + " ORDER BY [ROW_NUMBER]";
                StringBuilder sbWhere = new StringBuilder();
                if (!string.IsNullOrEmpty(experimentName))
                {
                    sbWhere.Append(" AND ExperimentsInfo.ExperimentName like '%" + experimentName + "%'");
                }

                UserInfoController userInfoController = new UserInfoController();
                RoleInfo userRole = userInfoController.GetRoleByUserName(SessionInfo.LoginName);
                if (userInfoController.GetAuthorize(AccessAuthority.LogInfo) == AccessAuthority.All)
                {
                    sbWhere.Append(@" AND ([t0].LoginName IS NULL or [t0].LoginName in 
                    (SELECT LoginName FROM UserInfo u left join roleinfo r on r.RoleName=u.RoleName WHERE r.RoleLevel<=" + userRole.RoleLevel + " ))");
                }
                else
                {
                    sbWhere.Append(" AND [t0].LoginName='" + SessionInfo.LoginName + "'");
                }
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
                                System.Data.DataRow dRow = dataTable.NewRow();
                                dRow["Number"] = startRowIndex;
                                dRow["LogContent"] = reader["LogContent"];
                                dRow["Module"] = reader["Module"];
                                dRow["CreaterTime"] = reader["CreaterTime"].ToString();
                                dRow["LoginName"] = reader["LoginName"];
                                dRow["ExperimentName"] = reader["ExperimentName"];
                                if (startRowIndex % 2 == 0)
                                {
                                    dRow["Color"] = alternativeColor;
                                }
                                else
                                {
                                    dRow["Color"] = defaultColor;
                                }

                                dataTable.Rows.Add(dRow);
                                startRowIndex++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string errorMessage = e.Message + System.Environment.NewLine + e.StackTrace;
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetNextLogs", SessionInfo.ExperimentID);
                throw;
            }
        }
    }
}
