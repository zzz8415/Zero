using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zero.NETCore.Result;

namespace Zero.NETCore.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ModelValidAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new JsonResult(new {
                    code = ErrorCode.sys_param_format_error, 
                    errorMsg = context.ModelState?.Values.FirstOrDefault()?.Errors?.FirstOrDefault()?.ErrorMessage 
                });
            }

        }
    }
}
