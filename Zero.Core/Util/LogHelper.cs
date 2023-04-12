using System;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using NLog;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Zero.NETCore.Util
{
    /// <summary>
    /// ��־��¼��
    /// </summary>
    public class LogHelper
    {
        private static readonly bool _isinit = false;

        private static HttpRequest _request = null;

        static LogHelper()
        {
            if (_isinit == false)
            {
                _isinit = true;
                SetConfig();
            }
        }

        /// <summary>
        /// �����������
        /// </summary>
        /// <param name="request"></param>
        public static void SetRequest(HttpRequest request) {
            _request = request;
        }


        private static bool LogInfoEnable = false;
        private static bool LogErrorEnable = false;
        private static bool LogExceptionEnable = false;
        private static bool LogComplementEnable = false;
        private static bool LogDubugEnable = false;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// ���ó�ʼֵ��
        /// </summary>
        public static void SetConfig()
        {
            LogInfoEnable = logger.IsInfoEnabled;
            LogErrorEnable = logger.IsErrorEnabled;
            LogExceptionEnable = logger.IsErrorEnabled;
            LogComplementEnable = logger.IsTraceEnabled;
            LogDubugEnable = logger.IsDebugEnabled;
        }
        /// <summary>
        /// д����ͨ��־��Ϣ
        /// </summary>
        /// <param name="info"></param>
        public static void WriteInfo(string info)
        {
            if (LogInfoEnable)
            {
                logger.Info(BuildMessage(info));
            }
        }
        /// <summary>
        /// д��Debug��־��Ϣ
        /// </summary>
        /// <param name="info"></param>
        public static void WriteDebug(string info)
        {
            if (LogDubugEnable)
            {
                logger.Debug(BuildMessage(info));
            }
        }
        /// <summary>
        /// д�������־��Ϣ
        /// </summary>
        /// <param name="info"></param>
        public static void WriteError(string info)
        {
            if (LogErrorEnable)
            {
                logger.Error(BuildMessage(info));
            }
        }

        /// <summary>
        /// д���쳣��־��Ϣ
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="frameIndex"></param>
        public static void WriteException(Exception ex, int frameIndex = 0)
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
        /// д���쳣��־��Ϣ
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public static void WriteException(string info, Exception ex)
        {
            if (LogExceptionEnable)
            {
                logger.Error(BuildMessage(info, ex));
            }
        }
        /// <summary>
        /// д�����ش�����־��Ϣ
        /// </summary>
        /// <param name="info"></param>
        public static void WriteFatal(string info)
        {
            if (LogErrorEnable)
            {
                logger.Fatal(BuildMessage(info));
            }
        }
        /// <summary>
        /// д�벹����־
        /// </summary>
        /// <param name="info"></param>
        public static void WriteComplement(string info)
        {
            if (LogComplementEnable)
            {
                logger.Trace(BuildMessage(info));
            }
        }
        /// <summary>
        /// д�벹����־
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ex"></param>
        public static void WriteComplement(string info, Exception ex)
        {
            if (LogComplementEnable)
            {
                logger.Trace(BuildMessage(info, ex));
            }
        }

        static string BuildMessage(string info)
        {
            return BuildMessage(info, null);
        }

        static string BuildMessage(string info, Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Time:{0}-{1}\r\n", DateTime.Now, info);

            if (_request != null)
            {
                sb.AppendFormat("Url:{0}://{1}{2}{3}\r\n", _request.Scheme, _request.Host, _request.Path, _request.QueryString);
                if (_request.Headers.TryGetValue("referrer", out StringValues values))
                {
                    sb.AppendFormat("UrlReferrer:{0}\r\n", values);
                }
                sb.AppendFormat("UserHostAddress:{0}:{1}\r\n", _request.HttpContext.Connection.RemoteIpAddress, _request.HttpContext.Connection.RemotePort);
                sb.AppendFormat("WebServer:{0}:{1}\r\n", _request.HttpContext.Connection.LocalIpAddress, _request.HttpContext.Connection.LocalPort);
            }

            if (ex != null)
            {
                sb.AppendFormat("Exception:{0}\r\n", ex);
            }
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// д���Զ�����־���Զ���Ŀ¼,��������Ӧ��Nlog.config����ʾ����
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">Ҫд�����Ϣ</param>
        /// <param name="dirOrPrefix">
        /// д�뵽����Ŀ¼���ļ�ǰ׺������ַ�������\��������Ŀ¼
        /// ���� aa\bb ��д����ļ���ΪaaĿ¼�µ�bb��ͷ������
        /// </param>
        public static void WriteCustom(string message, string dirOrPrefix)
        {
            WriteCustom(message, dirOrPrefix, null, true);
        }

        /// <summary>
        /// д���Զ�����־���Զ���Ŀ¼,��������Ӧ��Nlog.config����ʾ����
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">Ҫд�����Ϣ</param>
        /// <param name="dirOrPrefix">
        /// д�뵽����Ŀ¼���ļ�ǰ׺������ַ�������\��������Ŀ¼
        /// ���� aa\bb ��д����ļ���ΪaaĿ¼�µ�bb��ͷ������
        /// </param>
        /// <param name="addIpUrl">�Ƿ�Ҫ����ip��url����Ϣ</param>
        public static void WriteCustom(string message, string dirOrPrefix, bool addIpUrl)
        {
            WriteCustom(message, dirOrPrefix, null, addIpUrl);
        }


        /// <summary>
        /// д���Զ�����־���Զ���Ŀ¼,��������Ӧ��Nlog.config����ʾ����
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}${event-context:Suffix}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">Ҫд�����Ϣ</param>
        /// <param name="dirOrPrefix">
        /// д�뵽����Ŀ¼���ļ�ǰ׺������ַ�������\��������Ŀ¼
        /// ���� aa\bb ��д����ļ���ΪaaĿ¼�µ�bb��ͷ������
        /// </param>
        /// <param name="suffix">д�뵽���ļ���׺</param>
        public static void WriteCustom(string message, string dirOrPrefix, string suffix)
        {
            WriteCustom(message, dirOrPrefix, suffix, true);
        }

        /// <summary>
        /// д���Զ�����־���Զ���Ŀ¼,��������Ӧ��Nlog.config����ʾ����
        ///  &lt;targets>
        ///    &lt;target name="LogCustom" xsi:type="File" layout="${message}"
        ///          fileName="${logDirectory}\${event-context:DirOrPrefix}${date:format=yyyyMMddHH}${event-context:Suffix}.txt">&lt;/target>
        ///  &lt;/targets>
        ///  &lt;rules>
        ///    &lt;logger name="LogCustom" level="Warn" writeTo="LogCustom" />
        /// </summary>
        /// <param name="message">Ҫд�����Ϣ</param>
        /// <param name="dirOrPrefix">
        /// д�뵽����Ŀ¼���ļ�ǰ׺������ַ�������\��������Ŀ¼
        /// ���� aa\bb ��д����ļ���ΪaaĿ¼�µ�bb��ͷ������
        /// </param>
        /// <param name="suffix">д�뵽���ļ���׺</param>
        /// <param name="addIpUrl">�Ƿ�Ҫ����ip��url����Ϣ</param>
        public static void WriteCustom(string message, string dirOrPrefix, string suffix, bool addIpUrl)
        {
            if (addIpUrl)
                message = BuildMessage(message);
            Logger logger1 = LogManager.GetLogger("LogCustom");
            LogEventInfo logEvent = new LogEventInfo(LogLevel.Warn, logger1.Name, message);
            logEvent.Properties["DirOrPrefix"] = dirOrPrefix;
            if (suffix != null)
                logEvent.Properties["Suffix"] = suffix;
            logger1.Log(logEvent);
        }
    }
}
