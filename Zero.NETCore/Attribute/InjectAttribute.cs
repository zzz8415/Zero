using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class InjectAttribute : System.Attribute
    {
        public ServiceLifetime OptionsLifetime { get; set; } = ServiceLifetime.Scoped;
    }
}
