using Daily.CoreSwim.Abstraction;
using Microsoft.Extensions.Logging;

namespace Daily.CoreSwim.Dashboard.Cluster
{
    internal class AspNetCoreLogger(ILogger<ICoreSwimLogger> logger): ICoreSwimLogger
    {
        public void Info<T>(string text)
        {
            logger.LogInformation(text);
        }

        public void Warning<T>(string text)
        {
            logger.LogWarning(text);
        }

        public void Error<T>(string text)
        {
            logger.LogError(text);
        }
    }
}
