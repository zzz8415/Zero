using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using NLog;
using System;
using System.Text;
using Zero.NETCore.Attribute;
using Zero.NETCore.Web;

namespace Zero.NETCore
{
    /// <summary>
    /// 日志记录类
    /// 20200423:调整为新的配置文件
    ///<variable name="logDirectory" value="Logs"/>
    ///<targets>
    ///  <target xsi:type="File" name="Default"
    ///          fileName="${logDirectory}/${level}/${date:format=yyyyMMddHH}.txt"
    ///          layout="${longdate} ${message}"
    ///          archiveAboveSize="5000000" />
    ///
    ///  <target xsi:type="File" name="Custom"
    ///          fileName="${logDirectory}/${event-context:DirOrPrefix}/${date:format=yyyyMMddHH}.txt"
    ///          layout="${longdate} ${message}"
    ///          archiveAboveSize="5000000" />
    ///</targets>
    ///<rules>
    ///  <logger name = "*" level="Info" writeTo="Default"/>
    ///  <logger name = "*" level="Trace" writeTo="Default"/>
    ///  <logger name = "*" level="Debug" writeTo="Default"/>
    ///  <logger name = "*" level="Error" writeTo="Default"/>
    ///  <logger name = "*" level="Fatal" writeTo="Default"/>
    ///
    ///  <logger name = "LogCustom" level="Warn" writeTo="Custom" />
    ///</rules>
    /// </summary>
    [Inject(OptionsLifetime = ServiceLifetime.Scoped)]
    public class LogClient
    {
        private HttpRequest Request => _webClient?.Request;

        private readonly WebClient _webClient = null;

        /// <summary>
        /// 构造类(可注入)
        /// </summary>
        /// <param name="webClient"></param>
        public LogClient(WebClient webClient = null)
        {
            _webClient = webClient;
        }

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 写入普通日志消息
        /// </summary>
        /// <param name="info"></param>
        public void WriteInfo(string info)
        {
            if (logger.IsInfoEnabled)
            {
                logger.Info(BuildMessage(info));
            }
        }
        /// <summary>
        /// 写入Debug日志消息
        /// </summary>
        /// <param name="info"></param>
        public void WriteDebug(string info)
        {
            if (logger.IsDebugEnabled)
            {
                logger.Debug(BuildMessage(info));
            }
        }
        /// <summary>
        /// 写入错误日志消息
        /// </summary>
        /// <param name="info"></param>
        public void WriteError(string info)
        {
            if (logger.IsErrorEnabled)
            {
                logger.Error(BuildMessage(info));
            }
        }

        /// <summary>
        /// 写入异常日志信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="frameIndex"></param>
        public void WriteException(Exception ex, int frameIndex = 0)
        {
            string info = string.Empty;
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
            if (st.FrameCount > frameIndex)
            {
                info = st.GetFrame(frameIndex + 1).GetMethod().Name;
            }
            WriteException(info, ex);
        }

        /// <summary>
        /// 写入异常日志信息
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public void WriteException(string info, Exception ex)
        {
            if (logger.IsErrorEnabled)
            {
                logger.Error(BuildMessage(info, ex));
            }
        }
        /// <summary>
        /// 写入严重错误日志消息
        /// </summary>
        /// <param name="info"></param>
        public void WriteFatal(string info)
        {
            if (logger.IsFatalEnabled)
            {
                logger.Fatal(BuildMessage(info));
            }
        }
        /// <summary>
        /// 写入补充日志
        /// </summary>
        /// <param name="info"></param>
        public void WriteTrace(string info)
        {
            if (logger.IsTraceEnabled)
            {
                logger.Trace(BuildMessage(info));
            }
        }

        private string BuildMessage(string info)
        {
            return BuildMessage(info, null);
        }

        private string BuildMessage(string info, Exception ex)
        {
            StringBuilder sb = new StringBuilder(info);
            sb.AppendLine();
            //sb.AppendFormat("Time:{0}-{1}\r\n", DateTime.Now, info);

            if (Request != null)
            {
                sb.AppendFormat("Url:{0}://{1}{2}{3}", Request.Scheme, Request.Host, Request.Path, Request.QueryString);
                sb.AppendLine();
                if (Request.Headers.TryGetValue("referrer", out StringValues values))
                {
                    sb.AppendFormat("UrlReferrer:{0}", values);
                    sb.AppendLine();
                }
                sb.AppendFormat("UserHostAddress:{0}:{1}", Request.HttpContext.Connection.RemoteIpAddress, Request.HttpContext.Connection.RemotePort);
                sb.AppendLine();
                sb.AppendFormat("WebServer:{0}:{1}", Request.HttpContext.Connection.LocalIpAddress, Request.HttpContext.Connection.LocalPort);
                sb.AppendLine();
                if (_webClient.PostData != null)
                {
                    sb.AppendFormat("PostData:{0}", _webClient.PostData);
                    sb.AppendLine();
                }
            }

            if (ex != null)
            {
                sb.AppendFormat("Exception:{0}", ex);
                sb.AppendLine();
            }
      
            return sb.ToString();
        }

        /// <summary>
        /// 写入自定义日志到自定义目录,本方法对应的Nlog.config配置示例：
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">要写入的消息</param>
        /// <param name="dirOrPrefix">
        /// 写入到的子目录或文件前缀，如果字符串包含\，则是子目录
        /// 比如 aa\bb 则写入的文件名为aa目录下的bb开头加日期
        /// </param>
        public void WriteCustom(string message, string dirOrPrefix)
        {
            message = BuildMessage(message);
            Logger logger = LogManager.GetLogger("LogCustom");
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Warn, logger.Name, message);
            logEvent.Properties["DirOrPrefix"] = dirOrPrefix;
            //if (suffix != null)
            //{
            //    logEvent.Properties["Suffix"] = suffix;
            //}

            logger.Log(logEvent);
        }

        /// <summary>
        /// 写入自定义日志到自定义目录,本方法对应的Nlog.config配置示例：
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">要写入的消息</param>
        /// <param name="dirOrPrefix">
        /// 写入到的子目录或文件前缀，如果字符串包含\，则是子目录
        /// 比如 aa\bb 则写入的文件名为aa目录下的bb开头加日期
        /// </param>
        /// <param name="addIpUrl">是否要附加ip和url等信息</param>
        //public void WriteCustom(string message, string dirOrPrefix, bool addIpUrl)
        //{
        //    WriteCustom(message, dirOrPrefix, null, addIpUrl);
        //}


        /// <summary>
        /// 写入自定义日志到自定义目录,本方法对应的Nlog.config配置示例：
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}${event-context:Suffix}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">要写入的消息</param>
        /// <param name="dirOrPrefix">
        /// 写入到的子目录或文件前缀，如果字符串包含\，则是子目录
        /// 比如 aa\bb 则写入的文件名为aa目录下的bb开头加日期
        /// </param>
        /// <param name="suffix">写入到的文件后缀</param>
        //public void WriteCustom(string message, string dirOrPrefix, string suffix)
        //{
        //    WriteCustom(message, dirOrPrefix, suffix, true);
        //}

        /// <summary>
        /// 写入自定义日志到自定义目录,本方法对应的Nlog.config配置示例：
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}${event-context:Suffix}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">要写入的消息</param>
        /// <param name="dirOrPrefix">
        /// 写入到的子目录或文件前缀，如果字符串包含\，则是子目录
        /// 比如 aa\bb 则写入的文件名为aa目录下的bb开头加日期
        /// </param>
        /// <param name="suffix">写入到的文件后缀</param>
        /// <param name="addIpUrl">是否要附加ip和url等信息</param>
        //public void WriteCustom(string message, string dirOrPrefix, string suffix, bool addIpUrl)
        //{
        //    if (addIpUrl)
        //    {
        //        message = BuildMessage(message);
        //    }

        //    Logger logger = LogManager.GetLogger("LogCustom");
        //    LogEventInfo logEvent = new LogEventInfo(LogLevel.Warn, logger.Name, message);
        //    logEvent.Properties["DirOrPrefix"] = dirOrPrefix;
        //    if (suffix != null)
        //    {
        //        logEvent.Properties["Suffix"] = suffix;
        //    }

        //    logger.Log(logEvent);
        //}
    }

}
