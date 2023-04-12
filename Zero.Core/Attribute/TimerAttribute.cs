using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;
using NLog.Web;

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
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TimerAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly int _timeOutSeconds = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeOutSeconds"></param>
        public TimerAttribute(int timeOutSeconds = 2000)
        {
            this._timeOutSeconds = timeOutSeconds;
        }

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

            if (time > _timeOutSeconds)
            {
                var controllerName = context.RouteData.Values["Controller"].ToString();

                var actionName = context.RouteData.Values["Action"].ToString();

                var message = string.Format("Controller:[{0}] Action:[{1}],本次请求耗时 {2} 秒.", controllerName, actionName, (double)time / 1000);

                var logger = LoggerFactory.Create(x => x.AddNLog()).CreateLogger<TimerAttribute>();

                logger.LogWarning(message);
            }
        }
    }
}
