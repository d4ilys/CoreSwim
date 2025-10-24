using Daily.CoreSwim.Abstraction;

namespace WebApplicationClusterSimple
{
    public class MyJob01 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //随机异常
            if (new Random().Next(0, 10) == 5)
            {
                throw new Exception("测试异常");
            }

            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第一个任务我执行了..");
            return Task.CompletedTask;
        }
    }

    public class MyJob02 : IJob
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //随机异常
            if (new Random().Next(0, 10) == 5)
            {
                throw new Exception("测试异常");
            }
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第二个任务我执行了..");
        }
    }

    public class MyJob03 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //随机异常
            if (new Random().Next(0, 10) == 5)
            {
                throw new Exception("测试异常");
            }
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第三个任务我执行了..");
            return Task.CompletedTask;
        }
    }

    public class MyJob04 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //随机异常
            if (new Random().Next(0, 10) == 5)
            {
                throw new Exception("测试异常");
            }
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第四个任务我执行了..");
            return Task.CompletedTask;
        }
    }

    public class MyJob05 : IJob
    {

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //随机异常
            if (new Random().Next(0, 10) == 5)
            {
                throw new Exception("测试异常");
            }
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第五个任务我执行了..");
            return Task.CompletedTask;
        }
    }

    public class MyJob06 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第六个任务我执行了..");
            return Task.CompletedTask;
        }
    }
}