using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Zero.Core.Extensions;
using Zero.Core.Inject;
using Zero.Core.Result;

namespace Zero.Core.Attribute
{
    /// <summary>
    /// 重复阻止
    /// </summary>
    /// <param name="handleParam"></param>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RepeatBlockAttribute(string handleParam = "_") : ActionFilterAttribute
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var webClient = context.HttpContext.RequestServices.GetService<WebClient>();

            var key = webClient.GetParam(handleParam);

            if (key.IsNullOrEmpty())
            {
                context.Result = new JsonResult(new
                {
                    Code = ErrorCode.SYS_PARAM_FORMAT_ERROR,
                    ErrorDesc = ErrorCode.SYS_PARAM_FORMAT_ERROR.GetDescription()
                });
                return;
            }
            
            using var mutex = new Mutex(true, key, out var flag);

            if (!flag)
            {
                context.Result = new JsonResult(new
                {
                    Code = ErrorCode.USER_CUSTOM,
                    ErrorDesc = "REPEAT ERROR"
                });
                return;
            }

            await next();

        }

    }
}
