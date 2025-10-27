using Daily.CoreSwim.Cluster.DependencyInjection;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;

namespace Daily.CoreSwim.Dashboard.Cluster
{
    public static class CoreSwimDashboardBuilderExtension
    {
        public static CoreSwimDashboardBuilder UseFreeSql(this CoreSwimDashboardBuilder builder,
            Action<CoreSwimDashboardMySqlOptions> options)
        {
            var optionsInternal = new CoreSwimDashboardMySqlOptions();

            options(optionsInternal);

            builder.Service.AddSingleton(optionsInternal);

            builder.Service.AddSingleton(_ =>
                new FreeSqlBuilder().UseConnectionString(optionsInternal.DataType,
                        optionsInternal.ConnectionString)
                    .UseAdoConnectionPool(true)
                    .Build()
            );

            builder.Service.AddHostedService<CoreSwimMySqlPersistenceHostedService>();

            return builder;
        }
        public static CoreSwimDashboardBuilder AddCoreSwimDashboard(this IServiceCollection services)
        {
            return new CoreSwimDashboardBuilder(services);
        }
    }
}