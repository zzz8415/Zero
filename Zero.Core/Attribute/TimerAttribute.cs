using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();

            await next().ConfigureAwait(false);

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > timeOutMilliseconds)
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<TimerAttribute>>();

                logger.LogCustom($"本次请求耗时 {stopwatch.ElapsedMilliseconds / 1000M} 秒.", "Timeout");
            }
        }
    }
}
