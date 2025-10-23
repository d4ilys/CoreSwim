using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daily.CoreSwim.Configs
{
    public class CoreSwimTenant
    {
        public IEnumerable<string> Tenants { get; set; } = [];

        public Action<string>? CurrentTenantContextSetting { get; set; }
    }
}
