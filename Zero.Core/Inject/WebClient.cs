using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Text;
using Zero.Core.Attribute;

namespace Zero.Core.Inject
{
    /// <summary>
    /// Web客户端信息
    /// </summary>
    /// <remarks>
    /// 初始化
    /// </remarks>
    /// <param name="httpContextAccessor"></param>
    [Inject(OptionsLifetime = ServiceLifetime.Scoped)]
    public class WebClient(IHttpContextAccessor httpContextAccessor)
    {
        /// <summary>
        /// http请求信息
        /// </summary>
        public HttpRequest Request => HttpContextAccessor.HttpContext.Request;

        /// <summary>
        /// http上下文
        /// </summary>
        public IHttpContextAccessor HttpContextAccessor { get; set; } = httpContextAccessor;

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

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string IP
        {
            get
            {
                if (Request.Headers.TryGetValue("X-Forwarded-For", out StringValues value))
                {
                    return value.ToString();
                }

                return Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
        private string _postData = null;

        /// <summary>
        /// Post数据流
        /// </summary>
        public string PostData
        {
            get
            {
                if (_postData == null && Request.Body != null)
                {
                    using var reader = new StreamReader(Request.Body);

                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                    _postData = reader.ReadToEndAsync().Result;
                }
                return _postData;
            }
        }
        #endregion

    }
}
