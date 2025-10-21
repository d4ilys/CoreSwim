namespace Daily.CoreSwim.Abstraction
{
    public interface ICoreSwimLogger
    {
        void Info<T>(string text);

        void Warning<T>(string text);

        void Error<T>(string text);
    }
}