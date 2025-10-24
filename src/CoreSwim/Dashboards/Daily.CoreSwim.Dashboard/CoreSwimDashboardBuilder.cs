using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Daily.CoreSwim.Dashboard
{
    public class CoreSwimDashboardBuilder(IServiceCollection service)
    {
        public IServiceCollection Service { get; internal set; } = service;
    }
}