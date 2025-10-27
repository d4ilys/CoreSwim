using Daily.CoreSwim.Abstraction;
using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Daily.CoreSwim.Cluster.DependencyInjection
{
    public static class CoreSwimClusterExtensions
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
                coreSwim.Config.Logger = new MicrosoftExtensionsLogging(provider.GetService<ILogger<ICoreSwimLogger>>()!);
                return coreSwim;
            });

            services.AddHostedService<CoreSwimHostedService>();

            return services;
        }
    }
}