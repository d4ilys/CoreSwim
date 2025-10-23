using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using FreeRedis;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Daily.CoreSwim.Distributed
{
    internal class DistributedActuatorStore(RedisClient client) : IActuatorStore
    {
        private readonly ConcurrentDictionary<string, Actuator> _jobs = new();
        private readonly string _jobsHashKey = $"daily_coreSwim_jobs_{Assembly.GetEntryAssembly()!.GetName().Name}";

        public async Task SaveJobsAsync(string jobId, Actuator actuator)
        {
            //获取运行的程序集的名称
            _jobs.TryAdd(jobId, actuator);
            await client.HSetAsync(_jobsHashKey, jobId, 1);
        }

        public async Task<Actuator?> GetJobAsync(string jobId)
        {
            var id = await client.HGetAsync(_jobsHashKey, jobId);
            return _jobs.GetValueOrDefault(id);
        }

        public async Task<ConcurrentDictionary<string, Actuator>> GetAllJobAsync()
        {
            var ids = await client.HGetAllAsync(_jobsHashKey);

            var jobs = new ConcurrentDictionary<string, Actuator>();

            foreach (var pair in ids)
            {
                if (_jobs.TryGetValue(pair.Key, out var actuator))
                {
                    jobs.TryAdd(pair.Key, actuator);
                }
            }

            return jobs;
        }

        public bool RemoveJob(string jobId)
        {
            var hDel = client.HDel(_jobsHashKey, jobId);
            return hDel > 0;
        }

        public void ClearAllJobs()
        {
            _jobs.Clear();
            client.HDel(_jobsHashKey);
        }
    }
}