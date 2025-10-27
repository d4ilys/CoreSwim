using Daily.CoreSwim.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Daily.CoreSwim.DependencyInjection
{
    public static class CoreSwimExtensions
    {
        /// <summary>
        /// 添加CoreSwim
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddCoreSwim(this IServiceCollection services, Action<ICoreSwim> options)
        {
            var func = options;
            services.AddSingleton(provider =>
            {
                ICoreSwim coreSwim = new CoreSwim();
                func(coreSwim);
                coreSwim.Config.Logger =
                    new MicrosoftExtensionsLogging(provider.GetService<ILogger<ICoreSwimLogger>>()!);
                return coreSwim;
            });

            services.AddHostedService<CoreSwimHostedService>();

            return services;
        }
    }
}