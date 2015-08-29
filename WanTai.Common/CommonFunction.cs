using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Data;
using System.Data.Odbc;
namespace WanTai.Common
{
    public class CommonFunction
    {
        private static Mutex m_Mutex = new Mutex();
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str">明文</param>
        /// <returns>密文</returns>
        public static string MD5Hash(string str)
        {

            ASCIIEncoding ASC = new ASCIIEncoding();
            byte[] arrPwd = ASC.GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] arrHashPwd = md5.ComputeHash(arrPwd);
            //转成字符串，表示十六进制值
            string sHashPwd = "";
            foreach (byte b in arrHashPwd)
            {
                if (b < 16)
                    sHashPwd = sHashPwd + "0" + b.ToString("X");
                else
                    sHashPwd = sHashPwd + b.ToString("X");
            }
            return sHashPwd;
        }
        /// <summary>
        /// 添加系统日志()
        /// </summary>
        /// <param name="sMessage">日志内容</param>
        /// <returns>无</returns>
        public static string WriteLog(string sMessage)
        {
            try
            {
                m_Mutex.WaitOne();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            try
            {
                string sPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + "\\WanTai\\SysErr\\";; 
                // string sPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\SysErr\\";
                if (!System.IO.Directory.Exists(sPath))
                    System.IO.Directory.CreateDirectory(sPath);

                if (sPath.Substring(sPath.Length - 1, 1) != "\\")
                {
                    sPath += "\\";
                }
                string sFileName = sPath + "Srv" + System.DateTime.Today.ToString("yyyyMMdd") + ".log";
                string sTime = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                System.IO.TextWriter oWrite = System.IO.File.AppendText(sFileName);
                oWrite.WriteLine(sTime + ": " + sMessage);
                oWrite.Close();
                return "ok";
            }
            catch (Exception e)
            {
                EventLog myLog = new EventLog();
                myLog.Source = " VbsServer";
                myLog.WriteEntry("Err:" + sMessage + "\t" + e.ToString());
                return e.ToString();
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// 添加系统日志
        /// </summary>
        /// <param name="sMessage">日志内容</param>
        /// <returns>无</returns>
        public static void WriteLog(string sMessage, string fileName)
        {
            try
            {
                m_Mutex.WaitOne();
            }
            catch (Exception)
            {
                return;
            }
            try
            {
                string sPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "\\DBLog\\";
                if (!System.IO.Directory.Exists(sPath))
                    System.IO.Directory.CreateDirectory(sPath);

                if (sPath.Substring(sPath.Length - 1, 1) != "\\")
                {
                    sPath += "\\";
                }
                string sFileName = sPath + fileName + System.DateTime.Today.ToString("yyyyMMdd") + ".log";
                string sTime = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
                System.IO.TextWriter oWrite = System.IO.File.AppendText(sFileName);
                oWrite.WriteLine(sTime + ": " + sMessage);
                oWrite.Close();
            }
            catch (Exception e)
            {
                EventLog myLog = new EventLog();
                myLog.Source = " VbsServer";
                myLog.WriteEntry("Err:" + sMessage + "\t" + e.ToString());
            }
            finally
            {
                m_Mutex.ReleaseMutex();
            }
        }
        /// <summary>
        /// 执行外部进程
        /// </summary>
        /// <param name="procedure">外部进程地址</param>
        /// <param name="outtime">超时时间</param>
        /// <returns>外部进程输出</returns>
        public static string Execute(string procedure, int outtime)
        {
            string output = "";
            if (procedure != null && procedure != "")
            {
                Process process = new Process();//创建进程对象
                ProcessStartInfo startinfo = new ProcessStartInfo();//创建进程时使用的一组值，如下面的属性
                startinfo.FileName = procedure;//设定需要执行的命令程序
                //以下是隐藏cmd窗口的方法
                //startinfo.Arguments = "/c" + dosCommand;//设定参数，要输入到命令程序的字符，其中"/c"表示执行完命令后马上退出
                //startinfo.UseShellExecute = false;//不使用系统外壳程序启动
                //startinfo.RedirectStandardInput = false;//不重定向输入
                //startinfo.RedirectStandardOutput = true;//重定向输出，而不是默认的显示在dos控制台上
                //startinfo.createNoWindow = true;//不创建窗口
                process.StartInfo = startinfo;

                try
                {
                    if (process.Start())//开始进程
                    {
                        if (outtime == 0)
                        { process.WaitForExit(); }
                        else
                        { process.WaitForExit(outtime); }
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出
                    }
                }
                catch
                {

                }
                finally
                {
                    if (process != null)
                    { process.Close(); }
                }
            }
            return output;
        }

        public static string CC(int y)
        {
            int tm;
            tm = y;
            string c = "";
            int md;
            while (tm > 0)
            {
                md = tm % 26;
                tm = (tm - md) / 26;
                if (md == 0)
                {
                    md = 26;
                    tm = tm - 1;
                }
                c = Chr(64 + md) + c;
            }
            return c;
        }

        public static string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }
        public static string GetDoubleFigure(string str)
        {
            int i = str.IndexOf('.');
            if (str.Length - i + 1 > 2 && i>-1)
            {
                str = float.Parse(str).ToString("f2");
            }
            return str;
        }

        public static DataTable ReadScanFile(string fileName,string SQL)
        {            
            DataTable dtcsv = new DataTable();
            if (fileName != "")
            {
                try
                {
                    string strConn = @"Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=";
                    strConn += fileName + ";Extensions=asc,csv,tab,txt;";
                    using (OdbcConnection objConn = new OdbcConnection(strConn))
                    {
                        OdbcDataAdapter adapter = new OdbcDataAdapter(SQL,objConn);
                        dtcsv.Clear();
                        dtcsv.Columns.Clear();
                        objConn.Open();
                        adapter.Fill(dtcsv);
                    }
                }
                catch (System.Exception e)
                {
                    //Write Log
                    return null;
                }
            }
            return dtcsv;
        }
    }
}
