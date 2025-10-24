using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Cluster;
using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Daily.CoreSwim.Dashboard.Cluster
{
    public static class CoreSwimDashboardExtension
    {
        public static IServiceCollection AddCoreSwimCluster(this IServiceCollection services,
            Func<IServiceProvider, RedisClient> redisClient,
            Action<ICoreSwim> options)
        {
            var func = options;
            services.AddSingleton(provider =>
            {
                ICoreSwim coreSwim = new CoreSwimCluster(redisClient(provider));
                func(coreSwim);
                coreSwim.Config.Logger = new AspNetCoreLogger(provider.GetService<ILogger<ICoreSwimLogger>>()!);
                return coreSwim;
            });

            services.AddHostedService<CoreSwimHostedService>();

            return services;
        }
    }
}