﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
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
        public HttpRequest Request { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="request"></param>
        public WebClient(HttpRequest request)
        {
            Request = request;
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
                if (_postData == null)
                {
                    byte[] bytes = new byte[Request.Body.Length];
                    Request.Body.Read(bytes, 0, bytes.Length);
                    _postData = Encoding.UTF8.GetString(bytes);
                }
                return _postData;
            }
        }
        #endregion

    }
}
