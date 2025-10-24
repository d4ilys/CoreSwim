using FreeRedis;

namespace Daily.CoreSwim.Cluster
{
    internal class DistributedTaskCoordinateFactory(RedisClient client, ICoreSwim coreSwim)
    {
        /// <summary>
        /// 创建单任务执行模型
        /// </summary>
        /// <param name="taskKey"></param>
        /// <returns></returns>
        public DistributedTaskCoordinate Create(string taskKey)
        {
            var lazy = SingeTaskCache.TaskManager.GetOrAdd(taskKey, s => new Lazy<DistributedTaskCoordinate>(() =>
            {
                var component = new DistributedTaskCoordinate(client, coreSwim);
                component.InitializeSingleTask(taskKey);
                return component;
            }));
            return lazy.Value;
        }
    }
}