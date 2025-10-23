using Daily.CoreSwim.Abstraction;

namespace ConsoleAppTest
{
    public class MyJob01 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第一个任务我执行了..");
            return Task.CompletedTask;
        }
    }

    public class MyJob02 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第二个任务我执行了..");
            return Task.CompletedTask;
        }
    }
}