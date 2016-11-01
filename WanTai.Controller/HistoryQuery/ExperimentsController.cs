using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

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


        public List<NATInfo> GetNATInfos(string startDate, string endDate)
        {
            int pcrPlateType = WanTai.Common.Configuration.GetPCRPlateType();
            List<NATInfo> infos = new List<NATInfo>();
            Dictionary<string, int> barCodeDict = new Dictionary<string, int>();

            try
            {
                WanTaiEntities _WanTaiEntities = new WanTaiEntities();

                List<SystemFluidConfiguration> SystemFluid = _WanTaiEntities.SystemFluidConfigurations.ToList().Where(systemFluidConfiguration => systemFluidConfiguration.BatchType != "B" && systemFluidConfiguration.ItemType == 4).ToList();
                List<SystemFluidConfiguration> SystemFluidB = _WanTaiEntities.SystemFluidConfigurations.ToList().Where(systemFluidConfiguration => systemFluidConfiguration.BatchType == "B" && systemFluidConfiguration.ItemType == 4).ToList();

                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                ///todo: add control,each role user only can see the specific experiments
                string commandText = "SELECT [ExperimentID], [StartTime] FROM [dbo].[ExperimentsInfo] WHERE [State]=20 and [StartTime] between CONVERT(datetime,'"
                    + startDate + "',101) and DATEADD(dd, 1, CONVERT(datetime,'" + endDate + "',101)) order by startTime desc";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                        {
                            while (reader.Read())
                            {
                                NATInfo info = new NATInfo();
                                info.ExperimentID = (Guid)reader.GetValue(0);
                                info.StartTime = (DateTime)reader.GetValue(1);
                                info.StartTime = info.StartTime.Date;
                                infos.Add(info);
                            }
                        }
                    }

                    // iterate experiment
                    foreach (NATInfo info in infos) {
                        commandText = "select Tubes.TubeID, Tubes.BarCode, Tubes.TubeType, PoolingRulesConfiguration.PoolingRulesName from Tubes, TubeGroup,PoolingRulesConfiguration"
                                    + " where Tubes.TubeGroupID = TubeGroup.TubeGroupID and TubeGroup.PoolingRulesID = PoolingRulesConfiguration.PoolingRulesID and TubeGroup.ExperimentID = @ExperimentID";
                        using (SqlCommand cmd = new SqlCommand(commandText, conn))
                        {
                            cmd.Parameters.AddWithValue("@ExperimentID", info.ExperimentID);
                            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                            {
                                while (reader.Read())
                                {
                                    if ((short)reader.GetValue(2) == (short)Tubetype.Tube && (string)reader.GetValue(3) == "单检")
                                    {
                                        if (barCodeDict.ContainsKey((string)reader.GetValue(1)) && barCodeDict[(string)reader.GetValue(1)] == 1) {
                                           info.Split ++;
                                        }
                                    }
                                    else if ((short)reader.GetValue(2) == (short)Tubetype.Tube && (string)reader.GetValue(3) != "单检")
                                    {
                                        if (!barCodeDict.ContainsKey((string)reader.GetValue(1))) {
                                           barCodeDict.Add((string)reader.GetValue(1), 2);
                                        }
                                    }
                                }
                            }
                        }
                        // process barcode dict
                        string[] keys = new string[barCodeDict.Count];    
                        barCodeDict.Keys.CopyTo(keys, 0);    
                        foreach (string key in keys){
                            if (barCodeDict[key] == 1)
                            {
                                barCodeDict.Remove(key);
                            }
                            else if (barCodeDict[key] == 2)
                            {
                                barCodeDict[key] = 1;
                            }
                        }

                        int qc_single = 0;
                        int qc_mix = 0;
                        commandText = "select Tubes.Grid, Tubes.Position, PoolingRulesConfiguration.PoolingRulesName,TubeGroup.BatchType from Tubes, TubeGroup, PoolingRulesConfiguration"
                            + " where Tubes.TubeGroupID = TubeGroup.TubeGroupID and TubeGroup.PoolingRulesID = PoolingRulesConfiguration.PoolingRulesID and TubeGroup.ExperimentID = @ExperimentID";
                        using (SqlCommand cmd = new SqlCommand(commandText, conn))
                        {
                            cmd.Parameters.AddWithValue("@ExperimentID", info.ExperimentID);
                            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                            {
                                while (reader.Read())
                                {
                                    if (reader.GetValue(3) != null && reader.GetValue(3) == "B")
                                    {
                                        foreach (SystemFluidConfiguration sf in SystemFluidB)
                                        {
                                            if (sf.Grid == (int)reader["Grid"] && sf.Position == (int)reader["Position"])
                                            {
                                                info.QC += 1;
                                                if ((string)reader.GetValue(2) == "单检")
                                                {
                                                    qc_single += 1;
                                                }
                                                else
                                                {
                                                    qc_mix += 1;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (SystemFluidConfiguration sf in SystemFluid)
                                        {
                                            if (sf.Grid == (int)reader["Grid"] && sf.Position == (int)reader["Position"])
                                            {
                                                info.QC += 1;
                                                if ((string)reader.GetValue(2) == "单检")
                                                {
                                                    qc_single += 1;
                                                }
                                                else
                                                {
                                                    qc_mix += 1;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }


                        commandText = "select Tubes.TubeType, PoolingRulesConfiguration.PoolingRulesName,count(*) from Tubes, TubeGroup,PoolingRulesConfiguration"
                                    + " where Tubes.TubeGroupID = TubeGroup.TubeGroupID and TubeGroup.PoolingRulesID = PoolingRulesConfiguration.PoolingRulesID and TubeGroup.ExperimentID = @ExperimentID"
                                    + " group by Tubes.TubeType, PoolingRulesConfiguration.PoolingRulesName";
                        using (SqlCommand cmd = new SqlCommand(commandText, conn))
                        {
                            cmd.Parameters.AddWithValue("@ExperimentID", info.ExperimentID);
                            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                            {
                                while (reader.Read())
                                {
                                    if ((short)reader.GetValue(0) == (short)Tubetype.NegativeControl) 
                                    {
                                        info.NC += (int)reader.GetValue(2);
                                    }
                                    else if ((short)reader.GetValue(0) == (short)Tubetype.PositiveControl)
                                    {
                                        info.PC += (int)reader.GetValue(2);
                                    }
                                    else if ((short)reader.GetValue(0) == (short)Tubetype.Complement)
                                    {
                                        info.Complement += (int)reader.GetValue(2);
                                    }
                                    else if ((short)reader.GetValue(0) == (short)Tubetype.QC)
                                    {
                                        //
                                    }
                                    else if ((short)reader.GetValue(0) == (short)Tubetype.Tube)
                                    {
                                        info.Sample += (int)reader.GetValue(2);
                                        if ((string)reader.GetValue(1) == "单检")
                                        {
                                            info.Single += (int)reader.GetValue(2);
                                        }
                                        else
                                        {
                                            info.Mix += (int)reader.GetValue(2);
                                        }
                                    }
                                }
                            }
                        }

                        info.Single -= qc_single;
                        info.Sample -= qc_single;
                        info.Mix -= qc_mix;

                        /*
                        commandText = "select sum(Volume) from ReagentsAndSuppliesConsumption where ExperimentID=@ExperimentID and VolumeType=3 and ReagentAndSupplieID in (select ItemID from ReagentAndSupplies where ItemType=0 and ExperimentID=@ExperimentID)";
                        using (SqlCommand cmd = new SqlCommand(commandText, conn))
                        {
                            cmd.Parameters.AddWithValue("@ExperimentID", info.ExperimentID);
                            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                            {
                                while (reader.Read())
                                {
                                    info.ReagentCost = (reader.GetValue(0) != null) ? Convert.ToInt32(reader.GetValue(0)) : 0;
                                }
                            }
                        }*/
                        // aggregate data
                        info.Single = info.Single - info.Split;
                        if (SessionInfo.WorkDeskType == "100")
                        {
                            info.ReagentTheory = info.Sample + info.NC + info.PC + info.QC + info.Complement;
                        }
                        else
                        {
                            info.ReagentTheory = (int)Math.Ceiling(info.Mix * 1.0 / 6) + info.Single + info.NC + info.PC + info.QC;
                        }
                        // info.ReagentCost = 8;
                        if (info.ReagentTheory <= 32)
                            info.ReagentCost = 5;
                        else if (info.ReagentTheory > 32 && info.ReagentTheory <= 64)
                            info.ReagentCost = 7;
                        else if (info.ReagentTheory > 64 && info.ReagentTheory <= 96)
                            info.ReagentCost = 10;
                        if (info.ReagentTheory <= 32) info.ReagentCost = 5;
                        // info.ReagentTotal = info.ReagentTheory + info.ReagentCost - 5;
                        info.ReagentTotal = info.ReagentTheory + info.ReagentCost;
                        // info.Diti1000 = (info.NC * 2 + info.PC * 2 + info.QC + info.Mix + info.Single * 2 + info.Split * 2) + 34;
                        if (SessionInfo.WorkDeskType == "100")
                            info.Diti1000 = (int)Math.Ceiling((info.Sample + info.NC + info.PC + info.QC + info.Complement + 16) * 1.0 / 96);
                        else
                            info.Diti1000 = (int)Math.Ceiling(((int)Math.Ceiling(info.Mix * 1.0 / 6) + info.Single * 2 + info.Split * 2 + info.NC + info.PC + info.QC + 34) * 1.0 / 96);
                        // info.Diti200 = (int)Math.Ceiling((info.NC + info.PC + qc_single * 2 + qc_mix + info.Mix + info.Single + info.Split) * 1.0 / 2 + 24);
                        if (SessionInfo.WorkDeskType == "100")
                            info.Diti200 = (int)Math.Ceiling((info.Sample + info.NC + info.PC + info.QC + info.Complement + 10) * 1.0 / 96);
                        else
                            info.Diti200 = (int)Math.Ceiling((info.ReagentTheory * 2 + 22) * 1.0 / 96);
                        if (pcrPlateType == 0)
                        {
                            commandText = "select count(*) from Plates where ExperimentID=@ExperimentID and PlateType=2";
                            using (SqlCommand cmd = new SqlCommand(commandText, conn))
                            {
                                cmd.Parameters.AddWithValue("@ExperimentID", info.ExperimentID);
                                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                                {
                                    while (reader.Read())
                                    {
                                        info.PCR = (reader.GetValue(0) != null) ? Convert.ToInt32(reader.GetValue(0)) : 0;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (SessionInfo.WorkDeskType == "100")
                                info.PCR = (int)Math.Ceiling(info.ReagentTheory * 1.0 / 8);
                            else
                                info.PCR = (int)Math.Ceiling((((info.NC + info.PC + info.QC + info.Mix + info.Single + info.Split) * 1.0 / 6 + 2) * 1.0 / 8) * 3);
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

            return infos;
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

        public int GetEarlierRotationCount(DateTime startTime)
        {
            int count = 0;

            try
            {
                string connectionString = WanTai.Common.Configuration.GetConnectionString();
                ///todo: add control,each role user only can see the specific experiments
                string commandText = "select count(*) from RotationInfo, ExperimentsInfo"
                                   + " where ExperimentsInfo.ExperimentID = RotationInfo.ExperimentID"
                                   + " and ExperimentsInfo.State=20 and RotationInfo.State=20"
                                   + " and ExperimentsInfo.StartTime >= CONVERT(datetime,'" + startTime.ToString("yyyy-MM-dd 00:00:00")
                                   + "',101) and ExperimentsInfo.StartTime < CONVERT(datetime,'" + startTime.ToString("yyyy-MM-dd HH:mm:ss") + "',101)";
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
                LogInfoController.AddLogInfo(LogInfoLevelEnum.Error, errorMessage, SessionInfo.LoginName, this.GetType().ToString() + "->GetEarlierRotationCount", SessionInfo.ExperimentID);
                throw;
            }

            return count;
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

        public bool ExportToPdf(DataTable dt, string fileName, string rawFileName)
        {
            DataTable new_dt = new DataTable();

            new_dt.Columns.Add("序号");
            new_dt.Columns.Add("日期");
            new_dt.Columns.Add("NC");
            new_dt.Columns.Add("PC");
            if (SessionInfo.WorkDeskType == "100")
            {
                new_dt.Columns.Add("定量参考品");
            }
            new_dt.Columns.Add("QC");
            if (SessionInfo.WorkDeskType == "100")
            {
                new_dt.Columns.Add("样本数");
            }
            else
            {
                new_dt.Columns.Add("6混数");
                new_dt.Columns.Add("拆分数");
                new_dt.Columns.Add("单检数");
            }
            new_dt.Columns.Add("PCR理论试剂用量/T");
            new_dt.Columns.Add("试剂损耗/T");
            new_dt.Columns.Add("试剂总用量/T");
            new_dt.Columns.Add("Diti1000");
            new_dt.Columns.Add("Diti200");
            new_dt.Columns.Add("DW 96深孔板/个");
            new_dt.Columns.Add("磁头套管/个");
            new_dt.Columns.Add("100ml试剂槽/个");
            new_dt.Columns.Add("扩增耗材(8联排或PCR板)");

            foreach (DataRow row in dt.Rows)
            {
                DataRow dr = new_dt.NewRow();
                dr["序号"] = row["Number"];
                dr["日期"] = row["StartTime"].ToString();
                dr["NC"] = row["NC"].ToString();
                dr["PC"] = row["PC"].ToString();
                if (SessionInfo.WorkDeskType == "100")
                {
                    dr["定量参考品"] = row["Complement"].ToString();
                }
                dr["QC"] = row["QC"].ToString();
                if (SessionInfo.WorkDeskType == "100")
                {
                    dr["样本数"] = row["Sample"].ToString();
                }
                else
                {
                    dr["6混数"] = row["Mix"].ToString();
                    dr["拆分数"] = row["Split"].ToString();
                    dr["单检数"] = row["Single"].ToString();
                }
                dr["PCR理论试剂用量/T"] = row["ReagentTheory"].ToString();
                dr["试剂损耗/T"] = row["ReagentCost"].ToString();
                dr["试剂总用量/T"] = row["ReagentTotal"].ToString();
                dr["Diti1000"] = row["Diti1000"].ToString();
                dr["Diti200"] = row["Diti200"].ToString();
                dr["DW 96深孔板/个"] = row["DW96"].ToString();
                dr["磁头套管/个"] = row["Microtiter"].ToString();
                dr["100ml试剂槽/个"] = row["ReagentSlot"].ToString();
                dr["扩增耗材(8联排或PCR板)"] = row["PCR"].ToString();
                new_dt.Rows.Add(dr);
            }

            ///设置导出字体
            string FontPath = "C://WINDOWS//Fonts//msyh.ttf"; //"C://WINDOWS//Fonts//simsun.ttc,1";
            int FontSize = 8;

            Boolean result = true;
            //竖排模式,大小为A4，四周边距均为25
            Document document = new Document(PageSize.A4.Rotate(), 25, 25, 20, 20);

            //调用PDF的写入方法流
            //注意FileMode-Create表示如果目标文件不存在，则创建，如果已存在，则覆盖。
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));

            //创建PDF文档中的字体
            BaseFont baseFont = BaseFont.CreateFont(
              FontPath,
              BaseFont.IDENTITY_H,
              BaseFont.NOT_EMBEDDED);

            //根据字体路径和字体大小属性创建字体
            iTextSharp.text.Font font = new iTextSharp.text.Font(baseFont, FontSize);
            iTextSharp.text.Font fontWhite = new iTextSharp.text.Font(baseFont, FontSize);
            fontWhite.Color = iTextSharp.text.Color.WHITE;

            iTextSharp.text.Font fontTitle = new iTextSharp.text.Font(baseFont, 8);

            iTextSharp.text.Font fontSmall = new iTextSharp.text.Font(baseFont, 4);

            iTextSharp.text.Font fontWanTag = new iTextSharp.text.Font(baseFont, 12);
            fontWanTag.IsBold();

            DateTime dTime = DateTime.Now;

            iTextSharp.text.HeaderFooter footer = new iTextSharp.text.HeaderFooter(new Phrase("导出时间：" + dTime.ToString("yyyy/MM/dd HH:mm:ss") + "    页数: "), true);
            footer.Border = Rectangle.NO_BORDER;
            footer.Alignment = Element.ALIGN_RIGHT;
            document.Footer = footer;

            //打开目标文档对象
            document.Open();

            // document.Add(new Paragraph("\n"));
            Paragraph p_WanTag = new Paragraph(rawFileName, fontWanTag);
            p_WanTag.Alignment = Element.ALIGN_CENTER ;
            document.Add(p_WanTag);

            document.Add(new Paragraph("\n"));


            //根据数据表内容创建一个PDF格式的表
            PdfPTable table = new PdfPTable(new_dt.Columns.Count);
            table.WidthPercentage = 100f;            
            if (SessionInfo.WorkDeskType == "100")
                table.SetTotalWidth(new float[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 });
            else
                table.SetTotalWidth(new float[] { 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5 });
            PdfPCell cell;

            // 添加表头，每一页都有表头
            for (int i = 0; i < new_dt.Columns.Count; i++)
            {
                string cellName = new_dt.Columns[i].ColumnName;
                cell = new PdfPCell(new Phrase(cellName, font));

                // cell.UseAscender = true;
                cell.FixedHeight = 20;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = new Color(220, 220, 220);

                table.AddCell(cell);
            }

            // 告诉程序这行是表头，这样页数大于1时程序会自动为你加上表头。
            table.HeaderRows = 1;

            //遍历原datatable的数据行
            for (int i = 0; i < new_dt.Rows.Count; i++)
            {
                for (int j = 0; j < new_dt.Columns.Count - 1; j++)
                {
                    try
                    {
                        cell = new PdfPCell(new Phrase(new_dt.Rows[i][j].ToString(), font));
                        // System.Drawing.Color Color = System.Drawing.ColorTranslator.FromHtml(new_dt.Rows[i]["Color"].ToString());
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                       //  cell.BackgroundColor = new iTextSharp.text.Color(Color);
                        table.AddCell(cell);
                    }
                    catch (Exception e)
                    {
                        result = false;
                    }
                }
            }

            //在目标文档中添加转化后的表数据
            document.Add(table);

            //关闭目标文件
            document.Close();

            //关闭写入流
            writer.Close();
            return result;

        }
    }
}
