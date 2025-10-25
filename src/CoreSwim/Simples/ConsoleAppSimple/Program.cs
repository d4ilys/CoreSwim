using Daily.CoreSwim;
using Daily.CoreSwim.Actuators;

namespace ConsoleAppSimple;

class Program
{
    static async Task Main(string[] args)
    {
        var coreSwim = new CoreSwim();
        coreSwim.AddJob<MyJob01>(CoreSwimActuator.Period(4000));   // 每4秒执行
        coreSwim.AddJob<MyJob02>(CoreSwimActuator.DailyAt(2));  // 每天凌晨2点执行
        await coreSwim.StartAsync(CancellationToken.None);
        //var redis = new RedisClient("192.168.1.123:6379,password=111111");
        //var coreSwim = new CoreSwimDistributed(redis);
        //coreSwim.AddJob<MyJob01>(CoreSwimActuator.Period(4000));
        //coreSwim.AddJob<MyJob02>(CoreSwimActuator.PeriodSeconds(6));
        //await coreSwim.StartAsync(CancellationToken.None);
        //Console.ReadKey();
    }
}