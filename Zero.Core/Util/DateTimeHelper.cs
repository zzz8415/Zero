using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Util
{
    /// <summary>
    /// 
    /// </summary>
    public class DateTimeHelper
    {
        private static DateTime BaseTime = new DateTime(1970, 1, 1);

        /// <summary>   
        /// 将unix timestamp时间戳(毫秒数) 转换为.NET的DateTime   
        /// </summary>   
        /// <param name="timeStamp">毫秒数</param>   
        /// <returns>转换后的时间</returns>   
        public static DateTime FromUnixTime(long timeStamp)
        {
            return TimeZone.CurrentTimeZone.ToLocalTime(BaseTime).AddMilliseconds(timeStamp);
            //return new DateTime((timeStamp + 8 * 60 * 60) * 10000000 + BaseTime.Ticks);
        }

        /// <summary>   
        /// 将.NET的DateTime转换为unix timestamp时间戳(毫秒数)
        /// </summary>   
        /// <param name="dateTime">待转换的时间</param>   
        /// <returns>转换后的unix time</returns>   
        public static long FromDateTime(DateTime dateTime)
        {
            return (long)(dateTime - (TimeZone.CurrentTimeZone.ToLocalTime(BaseTime))).TotalMilliseconds;
            //return (dateTime.Ticks - BaseTime.Ticks) / 10000000 - 8 * 60 * 60;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string FormatTime(string time)
        {
            DateTime dt;
            if (DateTime.TryParse(time, out dt))
            {
                if (dt == DateTime.MinValue)
                    return "未审核";
                else
                    //return FormatTime(dt);
                    return dt.ToString("yyyy-MM-dd");
            }

            return "时间格式有误";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string FormatTime(DateTime time)
        {
            //TimeSpan ts = DateTime.Now - time;
            //if(ts.TotalSeconds < 60)
            //{
            //    return string.Format("{0}秒前", (int)ts.TotalSeconds);
            //}
            //if (ts.TotalMinutes < 60)
            //{
            //    return string.Format("{0}分钟前", (int)ts.TotalMinutes);
            //}
            //else if (ts.TotalHours < 24)
            //{
            //    return string.Format("{0}小时前", (int)ts.TotalHours);
            //}
            //else if (ts.TotalDays < 3)
            //{
            //    return string.Format("{0}天前", (int)ts.TotalDays);
            //}
            //else
            //{
            return time.ToString("yyyy-MM-dd");
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetNowTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 判断dtBegin到dtEnd这个时间段 和 dtCompareBegin到dtCompareEnd时间段的交集分钟数
        /// </summary>
        /// <param name="dtBegin"></param>
        /// <param name="dtEnd"></param>
        /// <param name="dtCompareBegin"></param>
        /// <param name="dtCompareEnd"></param>
        /// <returns></returns>
        public static double DateDiffMinutes(DateTime dtBegin, DateTime dtEnd, DateTime dtCompareBegin, DateTime dtCompareEnd)
        {
            if (dtEnd.CompareTo(dtCompareBegin) < 0 || dtBegin.CompareTo(dtCompareEnd) > 0)
                return 0;

            if (dtBegin.CompareTo(dtCompareBegin) < 0)
                dtBegin = dtCompareBegin;

            if (dtEnd.CompareTo(dtCompareEnd) > 0)
                dtEnd = dtCompareEnd;

            return dtEnd.Subtract(dtBegin).TotalMinutes;
        }
    }
}
