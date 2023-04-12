using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
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
        ///// <summary>
        ///// 配置
        ///// </summary>
        //public IConfiguration Configuration { get; }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="configuration"></param>
        //public BaseStartup(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //}

        private IServiceCollection _services = null;

        private ILogger<BaseStartup> _logger = null;

        /// <summary>
        /// 加入超时过滤器,实体验证,json转化,Zero及标准库注入等
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="services"></param>
        public void BaseConfigureServices(ILogger<BaseStartup> logger, IServiceCollection services)
        {
            _logger = logger;

            _services = services;

            services.AddControllers(options =>
            {
                options.Filters.Add<TimerAttribute>();
                options.Filters.Add<ModelValidAttribute>();
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new LongToStringConverter());
            });
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);

            services.AddMemoryCache();

            services.AddCors();

            services.AddHttpClient();

            services.AddHttpContextAccessor();

            services.AddZeroNetCoreAssembly();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assemblyName"></param>
        public void AddAssembly(string assemblyName)
        {
            _services.AddAssembly(assemblyName);
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="options"></param>
        private void HandlerException(IApplicationBuilder options)
        {
            options.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json;charset=utf-8";
                var ex = context.Features.Get<IExceptionHandlerFeature>().Error;
                if (ex != null)
                {
                    _logger.LogError(ex, ex.Message);

                    await context.Response.WriteAsync(new {
                        code = ErrorCode.sys_fail, 
                        errorMsg = $"错误状态码:{ (int)HttpStatusCode.InternalServerError }" 
                    }.ToJson(), Encoding.UTF8);
                }
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

        /// <summary>
        /// 错误信息处理
        /// </summary>
        /// <param name="app"></param>
        public void BaseConfigure(IApplicationBuilder app)
        {

            app.UseExceptionHandler(HandlerException);

            app.UseStatusCodePages(HandlerStatusCode);

            app.Use(async (context, next) =>
            {
                if (context.Request.Method == "POST")
                {
                    HttpRequestRewindExtensions.EnableBuffering(context.Request);
                }
                await next();
            });
        }
    }
}
