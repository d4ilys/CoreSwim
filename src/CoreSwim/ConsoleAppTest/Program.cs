using Daily.CoreSwim;
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Distributed;
using FreeRedis;

namespace ConsoleAppTest;

class Program
{
    static async Task Main(string[] args)
    {
        var redis = new RedisClient("192.168.1.123:6379,password=111111");
        var coreSwim = new CoreSwimDistributed(redis);
        coreSwim.AddJob<MyJob01>(CoreSwimActuator.Period(4000));
        coreSwim.AddJob<MyJob02>(CoreSwimActuator.PeriodSeconds(6));
        await coreSwim.StartAsync(CancellationToken.None);
        Console.ReadKey();
    }
}