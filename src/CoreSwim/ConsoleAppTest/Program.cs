using Daily.CoreSwim;
using Daily.CoreSwim.Actuators;

namespace ConsoleAppTest;

class Program
{
    static async Task Main(string[] args)
    {
        var coreSwim = new CoreSwim();
        coreSwim.AddJob<MyJob01>(CoreSwimActuator.Period(400));
        coreSwim.AddJob<MyJob02>(CoreSwimActuator.PeriodSeconds(2));
        await coreSwim.StartAsync(CancellationToken.None);
    }
}