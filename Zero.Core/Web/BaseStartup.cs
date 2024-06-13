using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Zero.Core.Attribute;
using Zero.Core.Converter;
using Zero.Core.Extensions;
using Zero.Core.Inject;
using Zero.Core.Result;

namespace Zero.Core.Web
{
    /// <summary>
    /// 启动基类
    /// </summary>
    public class BaseStartup
    {
        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="options"></param>
        private void HandlerException(IApplicationBuilder options)
        {
            options.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json;charset=utf-8";
                var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (error == null)
                {
                    return;
                }

                var logger = context.RequestServices.GetService<ILogger>();

                logger?.LogCustom($"RequestPath:{context.Request.Path} == {error}", "Error");

                await context.Response.WriteAsync(new
                {
                    Code = ErrorCode.sys_fail,
                    ErrorDesc = "ERROR"
                }.ToJson(), Encoding.UTF8);
            });
           
        }

        /// <summary>
        /// 处理异常状态码
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task HandlerStatusCode(StatusCodeContext context)
        {
            _logger.LogCustom(context.HttpContext.Request.Path, $"{(int)context.HttpContext.Response.StatusCode }Error");

            context.HttpContext.Response.ContentType = "application/json";

            await context.HttpContext.Response.WriteAsync(new { 
                    code = ErrorCode.sys_fail, 
                    errorMsg = $"错误状态码:{(int)context.HttpContext.Response.StatusCode }" 
                }.ToJson(), Encoding.UTF8);
        }


    }
}
