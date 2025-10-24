using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daily.CoreSwim.Dashboard.MySql
{
    public class CoreSwimDashboardMySqlOptions
    {
        public string DatabaseName { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public long RetainDataDays { get; set; } = 10;
    }
}