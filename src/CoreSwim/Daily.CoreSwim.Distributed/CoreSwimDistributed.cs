using System.Collections.Concurrent;
using System.Diagnostics;
using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Configs;
using Daily.CoreSwim.Retaining;
using FreeRedis;

namespace Daily.CoreSwim.Distributed
{
    /// <summary>
    /// 分布式实现
    /// </summary>
    public class CoreSwimDistributed : CoreSwim
    {
        private readonly DistributedTaskCoordinateFactory _distributedTaskCoordinateFactory;
        public CoreSwimDistributed(RedisClient redisClient)
        {
            _distributedTaskCoordinateFactory = new(redisClient,this);
            Config.ActuatorStore = new DistributedActuatorStore();
        }

        protected override Task<bool> CanItBeExecutedAsync(string jobId)
        {
            var distributedTaskCoordinate = _distributedTaskCoordinateFactory.Create(jobId);
            return distributedTaskCoordinate.GrabTheTaskAsync();
        }
    }
}