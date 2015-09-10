using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WanTai.DataModel;
using System.Data;
using System.Data.SqlClient;
namespace WanTai.Controller
{
    public class LogInfoController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel">0 debug,1 info,2 warning,3 error,10 operate</param>
        /// <param name="logContent"></param>
        /// <param name="loginName"></param>
        /// <param name="module"></param>
        /// <param name="experimentID"></param>
        public static void AddLogInfo(LogInfoLevelEnum logLevel, string logContent, string loginName, string module, Guid? experimentID)
        {
            LogInfo logInfo = new LogInfo()
            {
                LogID = WanTaiObjectService.NewSequentialGuid(),
                LogContent = logContent,
                LogLevel = (short)logLevel,
                LoginName = loginName,
                CreaterTime = DateTime.Now,
                ExperimentID = experimentID,
                Module = module
            };
            using (WanTaiEntities wanTaiEntities = new WanTaiEntities())
            {
                wanTaiEntities.AddToLogInfoes(logInfo);
                wanTaiEntities.SaveChanges();
            }
        }

        public static LogInfo GetLogInfoByID(Guid experimentID)
        {
            LogInfo logInfo;
            using (WanTaiEntities wanTaiEntities = new WanTaiEntities())
            {
                logInfo = wanTaiEntities.LogInfoes.First(P => P.LogID == experimentID);
            }
            return logInfo;
        }

        public static void DeleteLogInfo(Guid experimentID)
        {
            LogInfo logInfo;
            using (WanTaiEntities wanTaiEntities = new WanTaiEntities())
            {
                logInfo = wanTaiEntities.LogInfoes.First(P => P.LogID == experimentID);
                wanTaiEntities.LogInfoes.DeleteObject(logInfo);
                wanTaiEntities.SaveChanges();
            }
        }
        public static string GetRotationLog(Guid RotationID)
        {
            using (WanTaiEntities wanTaiEntities = new WanTaiEntities())
            {
                var RotationOperates=   wanTaiEntities.RotationOperates.Where(Rotation => Rotation.RotationID == RotationID);
                DateTime dtStart = DateTime.MaxValue;
                DateTime dtEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                Guid experimentID = new Guid();
                foreach(RotationOperate _RotationOperate in RotationOperates)
                {
                    experimentID = _RotationOperate.ExperimentID;
                    if(_RotationOperate.StartTime!=null)
                        dtStart = (dtStart > _RotationOperate.StartTime ?(DateTime) _RotationOperate.StartTime : dtStart);
                    if (_RotationOperate.EndTime != null)
                        dtEnd = (dtEnd < _RotationOperate.EndTime ? (DateTime)_RotationOperate.EndTime : dtEnd);
                }
                var Logs = wanTaiEntities.LogInfoes.Where(log => log.ExperimentID == experimentID && log.CreaterTime >= dtStart && log.CreaterTime <= dtEnd && (log.LogLevel == (short)LogInfoLevelEnum.EVORunTime || log.LogLevel == (short)LogInfoLevelEnum.KingFisher));
                StringBuilder LogConnt = new StringBuilder();
                foreach (LogInfo log in Logs)
                    LogConnt.Append(log.CreaterTime.ToString() + ":" + ((LogInfoLevelEnum)log.LogLevel).ToString() + log.Module + "  "+log.LogContent+"\r\n");
                return LogConnt.ToString();
            }
        }
        public static void InstertKingFisherLog(Guid experimentID, DateTime dtStart, DateTime dtEnd, out string ErrMsg)
        {
            ErrMsg = "success";
            using (SqlConnection con = new SqlConnection(WanTai.Common.Configuration.GetKingFisherConnectionString()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "select * from tbl_log where creation_time between @dtStart and @dtEnd ";// + dtStart.ToUniversalTime().ToString() + "' and '" + dtEnd.ToString() + "'";
                    DateTime _dtStart = dtStart.ToUniversalTime();
                    DateTime _dtEnd = dtEnd.ToUniversalTime();
                    cmd.Parameters.AddWithValue("@dtStart", _dtStart);
                    cmd.Parameters.AddWithValue("@dtEnd", _dtEnd);
                    con.Open();
                    SqlDataReader dReader = cmd.ExecuteReader();
                    using (WanTaiEntities wanTaiEntities = new WanTaiEntities())
                    {
                        while (dReader.Read())
                        {
                            LogInfo logInfo = new LogInfo()
                            {
                                LogID = WanTaiObjectService.NewSequentialGuid(),
                                LogContent = dReader["object_name"].ToString() + " " + dReader["message"].ToString(),
                                LogLevel = (int)LogInfoLevelEnum.KingFisher,
                                LoginName = SessionInfo.LoginName,
                                CreaterTime =  DateTime.Parse(dReader["creation_time"].ToString()).ToLocalTime(),
                                ExperimentID = experimentID,
                                Module = "KingFisherLog"
                            };
                            wanTaiEntities.AddToLogInfoes(logInfo);

                        }
                        wanTaiEntities.SaveChanges();
                    }
                }catch(Exception ex)
                {
                    ErrMsg = "连接KingFisher数据库失败!";
                    WanTai.Common.CommonFunction.WriteLog(ex.ToString());
                }
            }
        }
    }
}
