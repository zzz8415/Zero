using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Text;

namespace Zero.NETCore.Web
{
    /// <summary>
    /// Web客户端信息
    /// </summary>
    public class WebClient
    {
        /// <summary>
        /// http请求信息
        /// </summary>
        public HttpRequest Request => HttpContextAccessor.HttpContext.Request;

        /// <summary>
        /// http上下文
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="request"></param>
        public WebClient(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        #region 请求相关
        /// <summary>
        /// 获取上传的字符串值
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public string GetParam(string paramName)
        {
            if (Request.Query.TryGetValue(paramName, out StringValues value) ||
                (Request.HasFormContentType && Request.Form.TryGetValue(paramName, out value)))
            {
                return value;
            }
            return null;
        }

        private string _postData = null;

        /// <summary>
        /// Post数据流
        /// </summary>
        public string PostData
        {
            get
            {
                if (_postData == null && Request.Body != null && Request.Body.Length > 0)
                {
                    using(var reader = new StreamReader(Request.Body))
                    {
                        _postData = reader.ReadToEnd();
                    }
                }
                return _postData;
            }
        }
        #endregion

    }
}
