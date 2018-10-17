using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Zero.Core.Extensions;
using Zero.Core.Util;

namespace Zero.Core.Web
{
    /// <summary>
    /// Web客户端信息
    /// </summary>
    public class WebClient
    {
        /// <summary>
        /// http请求信息
        /// </summary>
        public HttpRequestBase Request { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="request"></param>
        public WebClient(HttpRequestBase request)
        {
            this.Request = request;
        }

        /// <summary>
        /// 获取传值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public T GetParam<T>(string paramName) where T : class
        {
            return Request[paramName] as T;
        }

        /// <summary>
        /// 获取上传的字符串值
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public string GetParam(string paramName)
        {
            return Request.Params[paramName];
        }

        private string _postData = null;

        /// <summary>
        /// Post数据流
        /// </summary>
        public string PostData
        {
            get
            {
                if (_postData == null)
                {
                    var bytes = new byte[Request.InputStream.Length];
                    Request.InputStream.Read(bytes, 0, bytes.Length);
                    _postData = Encoding.UTF8.GetString(bytes);
                }
                return _postData;
            }
        }

        private string ip = null;
        /// <summary>
        /// 当前IP
        /// </summary>
        public string IP
        {
            get
            {
                if (ip.IsNullOrEmpty())
                {
                    try
                    {
                        ip = Request.UserHostAddress;
                        if (!ip.IsNullOrEmpty() && ip.StartsWith("10.", StringComparison.Ordinal))
                        {
                            ip = Request.ServerVariables["HTTP_X_REAL_IP"].Split(',')[0].Trim();
                        }
                    }
                    catch(Exception ex) {
                        LogHelper.WriteException(ex);
                    }
                }
                return ip;
            }
        }
    }
}
