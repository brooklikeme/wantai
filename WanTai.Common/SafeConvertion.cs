using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace WanTai.Common
{
    public class SafeConvertion
    {
        #region 安全读取数据方法

        /// <summary>
        /// 安全取得字符串
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <param name="defvalue">默认值</param>
        /// <returns>转换后字符串</returns>
        public string GetSafeString(object o, string defvalue)
        {
            string result = defvalue;

            if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
                result = Convert.ToString(o);

            return result;
        }

        public string addStr(string[] strArray)
        {
            StringBuilder sbTmep = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (i != strArray.Length - 1)
                {
                    sbTmep.Append(string.Format("'{0}',", strArray[i]));
                }
                else
                {
                    sbTmep.Append(string.Format("'{0}'", strArray[i]));
                }
            }
            return sbTmep.ToString();
        }
        /// <summary>
        /// 安全取得字符串,不成功返回空字符串
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns></returns>
        public string GetSafeString(object o)
        {
            return GetSafeString(o, string.Empty);
        }

        /// <summary>
        /// 安全取得字符
        /// </summary>
        /// <param name="o">Object</param>
        /// <param name="defvalue">默认值</param>
        /// <returns></returns>
        public char GetSafeChar(object o, string defvalue)
        {
            char result = Convert.ToChar(defvalue);
            if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
            {
                result = Convert.ToChar(o);
            }
            return result;
        }
        /// <summary>
        /// 安全取得字符
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public char GetSafeChar(object o)
        {
            return GetSafeChar(o, string.Empty);
        }
        /// <summary>
        /// 安全取得整型值
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <param name="defvalue">默认值</param>
        /// <returns>转换后整型值</returns>
        public int GetSafeInt(object o, int defvalue)
        {
            int result = defvalue;
            try
            {
                if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
                    result = Convert.ToInt32(o);
            }
            catch (Exception ex)
            {
                CommonFunction.WriteLog(ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// 安全取得整型值,不成功返回-1
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns>转换后整型值</returns>
        public int GetSafeInt(object o)
        {
            return GetSafeInt(o, 0);
        }

        /// <summary>
        /// 安全取得日期
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <param name="defvalue">默认值</param>
        /// <returns>转换后日期</returns>
        public DateTime GetSafeDateTime(object o, DateTime defvalue)
        {
            DateTime result = defvalue;
            try
            {
                if (o != null &&  o != DBNull.Value && o.ToString()!=string.Empty)
                    result = Convert.ToDateTime(o);
            }
            catch (Exception ex)
            {
                CommonFunction.WriteLog(ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// 安全取得日期,不成功返回最大日期
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns>转换后日期</returns>
        public DateTime GetSafeDateTime(object o)
        {
            //最小日期常量超出了Sql Server能表示的最小日期,但最大值满足,因此使用最大值MaxValue
            return GetSafeDateTime(o, DateTime.MaxValue);
        }

        /// <summary>
        /// 安全区的布尔，不成功返回false
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns>转换后的布尔值</returns>
        public bool GetSafeBoolean(object o)
        {
            bool result = false;
            try
            {
                if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
                    result = Convert.ToBoolean(o);
            }
            catch (Exception ex)
            {
                CommonFunction.WriteLog(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 安全区的布尔，不成功返回false
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns>转换后的布尔值</returns>
        public bool GetSafeBoolean(object o, bool defaultValue)
        {
            bool result = defaultValue;
            try
            {
                if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
                    result = Convert.ToBoolean(o);
            }
            catch (Exception ex)
            {
                CommonFunction.WriteLog(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 安全取的decimal,不成功返回0
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns></returns>
        public decimal GetSafeDecimal(object o)
        {
            return GetSafeDecimal(o, 0);
        }

        /// <summary>
        /// 安全取的decimal
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns>转换后的decimal值</returns>
        public decimal GetSafeDecimal(object o, decimal defaultValue)
        {
            decimal result = defaultValue;
            try
            {
                if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
                    result = Convert.ToDecimal(o);
            }
            catch (Exception ex)
            {
                CommonFunction.WriteLog(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 安全取的double,不成功返回0
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns></returns>
        public double GetSafeDouble(object o)
        {
            return GetSafeDouble(o, 0);
        }

        /// <summary>
        /// 安全取的decimal
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns>转换后的Double值</returns>
        public double GetSafeDouble(object o, double defaultValue)
        {
            double result = defaultValue;
            try
            {
                if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
                    result = Convert.ToDouble(o);
            }
            catch (Exception ex)
            {
                CommonFunction.WriteLog(ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// 安全取的float,不成功返回0
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns></returns>
        public float GetSafeFloat(object o)
        {
            return GetSafeFloat(o, 0);
        }

        /// <summary>
        /// 安全取的float
        /// </summary>
        /// <param name="o">Object实例</param>
        /// <returns>转换后的float值</returns>
        public float GetSafeFloat(object o, float defaultValue)
        {
            float result = defaultValue;
            try
            {
                if (o != null && o != DBNull.Value && o.ToString() != string.Empty)
                    result = Convert.ToSingle(o);
            }
            catch (Exception ex)
            {
                CommonFunction.WriteLog(ex.ToString());
            }
            return result;
        }
        #endregion
    }
}
