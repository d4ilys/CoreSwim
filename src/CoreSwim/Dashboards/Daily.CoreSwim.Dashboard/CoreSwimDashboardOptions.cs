namespace Daily.CoreSwim.Dashboard
{
    public class CoreSwimDashboardOptions
    {
        public string DashboardPath { get; set; } = "CoreSwim";

        public List<string> IpWhitelist { get; set; } = new();

    }
}