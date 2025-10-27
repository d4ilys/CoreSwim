using Microsoft.Extensions.DependencyInjection;

namespace Daily.CoreSwim.Cluster.DependencyInjection
{
    public class CoreSwimDashboardBuilder(IServiceCollection service)
    {
        public IServiceCollection Service { get; internal set; } = service;
    }
}