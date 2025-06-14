using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Zero.Core.Attribute;
using Zero.Core.Converter;
using Zero.Core.Extensions;
using Zero.Core.Filter;
using Zero.Core.Result;

namespace Zero.Core.Util
{
    public class StartupHelper
    {
        /// <summary>
        /// api过滤器
        /// </summary>
        /// <param name="builder"></param>
        public static void AddApiFilter(WebApplicationBuilder builder)
        {
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<TimerAttribute>();
                options.Filters.Add<ModelValidAttribute>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString |
                                                                JsonNumberHandling.WriteAsString |
                                                                JsonNumberHandling.Strict;
                options.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
                // 允许尾随逗号
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new EmptyConverter([
                    "yyyy-MM-ddTHH:mm:ssZ",
                    "yyyy-MM-ddTHH:mm:ss.fffZ",
                    "yyyyMMddTHHmmssZ",
                    "o", // Round-trip 格式
        
                    // 常规日期时间
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy/MM/dd HH:mm:ss",
                    "dd-MMM-yyyy HH:mm:ss",
        
                    // 纯日期
                    "yyyy-MM-dd",
                    "yyyy/MM/dd",
                    "MM/dd/yyyy",
                    "dd.MM.yyyy",
        
                    // 其他
                    "ddd, dd MMM yyyy HH:mm:ss GMT"
                ]));
            });
        }

        /// <summary>
        /// signalR过滤器
        /// </summary>
        /// <param name="builder"></param>
        public static void AddSignalRFilter(WebApplicationBuilder builder)
        {
            builder.Services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString |
                                                                    JsonNumberHandling.WriteAsString |
                                                                    JsonNumberHandling.Strict;
                options.PayloadSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
                // 允许尾随逗号
                options.PayloadSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.PayloadSerializerOptions.AllowTrailingCommas = true;
                options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                options.PayloadSerializerOptions.Converters.Add(new EmptyConverter([
                        "yyyy-MM-ddTHH:mm:ssZ",
                        "yyyy-MM-ddTHH:mm:ss.fffZ",
                        "yyyyMMddTHHmmssZ",
                        "o", // Round-trip 格式
                    
                        // 常规日期时间
                        "yyyy-MM-dd HH:mm:ss",
                        "yyyy/MM/dd HH:mm:ss",
                        "dd-MMM-yyyy HH:mm:ss",
                    
                        // 纯日期
                        "yyyy-MM-dd",
                        "yyyy/MM/dd",
                        "MM/dd/yyyy",
                        "dd.MM.yyyy",
                    
                        // 其他
                        "ddd, dd MMM yyyy HH:mm:ss GMT"
                    ]));
            });
        }

        /// <summary>
        /// swagger配置
        /// </summary>
        /// <param name="builder"></param>
        public static void AddSwaggerGen(WebApplicationBuilder builder, Dictionary<string, string> apiArray)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                foreach (var api in apiArray)
                {
                    options.SwaggerDoc(api.Key, new OpenApiInfo
                    {
                        Title = api.Value,
                        Description = api.Value
                    });
                }

                var xmlFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");

                foreach (var xmlPath in xmlFiles) options.IncludeXmlComments(xmlPath);

                options.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "授权码",
                        Name = "Authorization",
                        Type = SecuritySchemeType.ApiKey
                    });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                     {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                             {
                                 Id = "Bearer", //The name of the previously defined security scheme.
                                 Type = ReferenceType.SecurityScheme
                             }
                         },
                         Array.Empty<string>()
                     }
                });
                options.CustomSchemaIds(type => type.FullName);
                options.SchemaFilter<EnumDescriptionSchemaFilter>();
            });
            builder.Services.Configure<Swashbuckle.AspNetCore.Swagger.SwaggerOptions>(c =>
            {
                c.RouteTemplate = "{documentName}/api.json";
            });
        }

        /// <summary>
        /// 异常处理
        /// </summary>
        /// <param name="app"></param>
        public static void HandlerException(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var logger = context.RequestServices.GetRequiredService<ILogger<StartupHelper>>();
                if (exception is DbUpdateConcurrencyException)
                {
                    logger?.LogFail(exception, "DBError");
                }
                else
                {
                    logger?.LogFail(exception);
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json; charset=utf-8";
                var r = new SysResult
                {
                    Code = ErrorCode.SYS_FAIL,
                    ErrorDesc = ErrorCode.SYS_FAIL.GetDescription()
                };

                await context.Response.WriteAsJsonAsync(r);
            });
        }

        /// <summary>
        /// 处理异常状态码
        /// </summary>
        /// <param name="context"></param>
        public static async Task HandlerStatusCode(StatusCodeContext context)
        {
            var logger = context.HttpContext.RequestServices.GetService<ILogger<StartupHelper>>();

            logger?.LogCustom(
                context.HttpContext.Request.Path,
                $"{context.HttpContext.Response.StatusCode}");

            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";

            var r = new SysResult
            {
                Code = ErrorCode.SYS_FAIL,
                ErrorDesc = $"ERROR:{context.HttpContext.Response.StatusCode}"
            };

            await context.HttpContext.Response.WriteAsJsonAsync(r);
        }
    }
}
