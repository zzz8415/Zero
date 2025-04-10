using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System;
using System.Collections.Generic;
using System.Linq;

using Zero.Core.Result;

namespace Zero.Core.Attribute
{
    /// <summary>
    /// 模型验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ModelValidAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 模型验证
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.HttpContext.Response.StatusCode = 422;
                context.Result = new JsonResult(new
                {
                    Code = ErrorCode.SYS_PARAM_FORMAT_ERROR,
                    ErrorDesc = context.ModelState?.Values.FirstOrDefault()?.Errors?.FirstOrDefault()?.ErrorMessage
                });
                return;
            }

        }
    }
}
