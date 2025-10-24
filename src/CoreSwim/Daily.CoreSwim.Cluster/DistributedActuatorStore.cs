using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using FreeRedis;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace Daily.CoreSwim.Cluster
{
    internal class DistributedActuatorStore(RedisClient client) : IActuatorStore
    {
        private readonly ConcurrentDictionary<string, Actuator> _jobs = new();
        private readonly string _jobsHashKey = $"daily_coreSwim_jobs_{Assembly.GetEntryAssembly()!.GetName().Name}";

        public async Task SaveJobAsync(string jobId, Actuator actuator)
        {
            //获取运行的程序集的名称
            _jobs.TryAdd(jobId, actuator);
            var hash = new DistributedActuatorStoreRedisHash
            {
                NumberOfRuns = actuator.NumberOfRuns,
                NumberOfErrors = actuator.NumberOfErrors,
            };
            await client.HSetAsync(_jobsHashKey, jobId, JsonSerializer.Serialize(hash));
        }

        public async Task UpdateJobAsync(string jobId, Actuator actuator)
        {
            //获取运行的程序集的名称
            _jobs[jobId] = actuator;
            var hash = new DistributedActuatorStoreRedisHash
            {
                NumberOfRuns = actuator.NumberOfRuns,
                NumberOfErrors = actuator.NumberOfErrors,
            };
            await client.HSetAsync(_jobsHashKey, jobId, JsonSerializer.Serialize(hash));
        }

        public async Task<Actuator?> GetJobAsync(string jobId)
        {
            var jobJsonString = await client.HGetAsync(_jobsHashKey, jobId);
            var jobDesc = JsonSerializer.Deserialize<DistributedActuatorStoreRedisHash>(jobJsonString);
            var valueOrDefault = _jobs.GetValueOrDefault(jobId);
            if (valueOrDefault != null && jobDesc != null) ;
            {
                valueOrDefault!.NumberOfRuns = jobDesc!.NumberOfRuns;
                valueOrDefault.NumberOfErrors = jobDesc.NumberOfErrors;
            }
            return valueOrDefault;
        }

        public async Task<ConcurrentDictionary<string, Actuator>> GetAllJobAsync()
        {
            var ids = await client.HGetAllAsync(_jobsHashKey);

            var jobs = new ConcurrentDictionary<string, Actuator>();

            foreach (var pair in ids)
            {
                if (_jobs.TryGetValue(pair.Key, out var actuator))
                {
                    var distributedActuatorStoreRedisHash = JsonSerializer.Deserialize<DistributedActuatorStoreRedisHash>(pair.Value);
                    actuator.NumberOfRuns =  distributedActuatorStoreRedisHash!.NumberOfRuns;
                    actuator.NumberOfErrors = distributedActuatorStoreRedisHash.NumberOfErrors;
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

    internal class DistributedActuatorStoreRedisHash
    {
        public long NumberOfRuns { get; set; }
        public long NumberOfErrors { get; set; }
    }
}