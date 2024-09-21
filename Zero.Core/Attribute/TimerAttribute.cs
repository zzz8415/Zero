using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Zero.Core.Extensions;
using Zero.Core.Inject;

namespace Zero.Core.Attribute
{
    /// <summary>
    /// 超时输出
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="timeOutSeconds"></param>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TimerAttribute(long timeOutMilliseconds = 2000) : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly long _timeOutMilliseconds = timeOutMilliseconds;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ticks = Environment.TickCount;

            await next();

            var time = Environment.TickCount - ticks;

            if (time > _timeOutMilliseconds)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<TimerAttribute>>();

                logger.LogCustom($"本次请求耗时 {(double)time / 1000} 秒.", "Timeout");
            }
        }
    }
}
