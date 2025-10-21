using Daily.CoreSwim.Abstraction;

namespace ConsoleAppTest
{
    public class MyJob01 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第一个任务我执行了..");
            //List 随机生成100个字典
            var list = new List<Dictionary<string, object>>();
            for (int i = 0; i < 10000; i++)
            {
                var dic = new Dictionary<string, object>
                {
                    { "Name", $"张三{i}" },
                    { "Age", i }
                };
                list.Add(dic);
            }

            var any = list.All(objects => objects.ContainsValue("张三1"));
            Console.WriteLine(any);
            return Task.CompletedTask;
        }
    }

    public class MyJob02 : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第二个任务我执行了..");
            //List 随机生成100个字典
            var list = new List<Dictionary<string, object>>();
            for (int i = 0; i < 10000; i++)
            {
                var dic = new Dictionary<string, object>
                {
                    { "Name", $"张三{i}" },
                    { "Age", i }
                };
                list.Add(dic);
            }

            var any = list.All(objects => objects.ContainsKey("张三1"));
            Console.WriteLine(any);
            return Task.CompletedTask;
        }
    }
}