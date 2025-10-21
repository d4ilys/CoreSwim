using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Retaining;

namespace Daily.CoreSwim.Configs
{

    internal class CoreSwimLogger : ICoreSwimLogger
    {
        public void Info<T>(string text)
        {
            CoreSwimConsole.Info<T>(text);
        }

        public void Warning<T>(string text)
        {
            CoreSwimConsole.Warning<T>(text);
        }

        public void Error<T>(string text)
        {
            CoreSwimConsole.Error<T>(text);
        }
    }
}