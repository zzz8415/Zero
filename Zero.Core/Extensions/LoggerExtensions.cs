using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 日志扩展
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// 自定义日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="dir"></param>
        public static void LogCustom(this ILogger logger, string message, string dir)
        {
            logger.LogInformation(new EventId(-65535, dir), message);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="exception"></param>
        public static void LogFail(this ILogger logger, Exception exception)
        {
            logger.LogError(exception, "Fail");
        }

        /// <summary>
        /// 自定义异常日志
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="exception"></param>
        /// <param name="dir"></param>
        public static void LogFail(this ILogger logger, Exception exception, string dir)
        {
            logger.LogError(new EventId(-65535, dir), exception, "Fail");
        }
    }
}
