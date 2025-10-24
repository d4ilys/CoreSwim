using System.Collections.Concurrent;
using System.Diagnostics;
using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Configs;
using Daily.CoreSwim.Retaining;
using FreeRedis;

namespace Daily.CoreSwim.Cluster
{
    /// <summary>
    /// 集群不实现
    /// </summary>
    public class CoreSwimCluster : CoreSwim
    {
        private readonly DistributedTaskCoordinateFactory _distributedTaskCoordinateFactory;

        public CoreSwimCluster(RedisClient redisClient)
        {
            _distributedTaskCoordinateFactory = new(redisClient, this);
            Config.ActuatorStore = new DistributedActuatorStore(redisClient);
            //预热！
            Config.Aop.AddJobAfter += a => _ = _distributedTaskCoordinateFactory.Create(a.JobId);
        }

        /// <summary>
        /// 一个任务同一时刻只能被一个Node执行
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        protected override Task<bool> CanItBeExecutedAsync(string jobId)
        {
            var distributedTaskCoordinate = _distributedTaskCoordinateFactory.Create(jobId);
            return distributedTaskCoordinate.GrabTheTaskAsync();
        }
    }
}