using FreeSql;

namespace Daily.CoreSwim.Dashboard.Cluster
{
    public class CoreSwimDashboardMySqlOptions
    {
        public long RetainDataDays { get; set; } = 10;

        public DataType DataType { get; set; } = DataType.MySql;

        public string ConnectionString { get; set; }
    }
}