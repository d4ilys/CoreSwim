namespace Daily.CoreSwim.Abstraction
{
    public interface IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken);
    }
}