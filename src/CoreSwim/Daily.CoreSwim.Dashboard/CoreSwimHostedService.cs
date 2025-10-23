using Daily.CoreSwim.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Daily.CoreSwim.Dashboard
{
    internal class CoreSwimHostedService(ICoreSwim swim, IServiceProvider provider) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            swim.Config.ActivatorCreateInstance = type => ActivatorUtilities.CreateInstance(provider, type);
            return swim.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}