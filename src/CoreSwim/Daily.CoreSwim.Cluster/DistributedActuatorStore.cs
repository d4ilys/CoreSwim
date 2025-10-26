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
        private readonly ConcurrentDictionary<string, Actuator> _localJobs = new();
        private readonly string _jobsHashKey = $"daily_coreSwim_jobs_{Assembly.GetEntryAssembly()!.GetName().Name}";

        public async Task SaveJobAsync(string jobId, Actuator actuator)
        {
            //获取运行的程序集的名称
            _localJobs.TryAdd(jobId, actuator);
            if (!await client.HExistsAsync(_jobsHashKey, jobId))
            {
                var hash = new DistributedActuatorStoreRedisHash
                {
                    NumberOfRuns = actuator.NumberOfRuns,
                    NumberOfErrors = actuator.NumberOfErrors,
                    Status = actuator.JobStatus
                };
                await client.HSetAsync(_jobsHashKey, jobId, JsonSerializer.Serialize(hash));
            }
        }

        public async Task UpdateJobAsync(string jobId, Actuator actuator)
        {
            //获取运行的程序集的名称
            _localJobs[jobId] = actuator;
            var hash = new DistributedActuatorStoreRedisHash
            {
                NumberOfRuns = actuator.NumberOfRuns,
                NumberOfErrors = actuator.NumberOfErrors,
                Status = actuator.JobStatus
            };
            await client.HSetAsync(_jobsHashKey, jobId, JsonSerializer.Serialize(hash));
        }

        public async Task<Actuator?> GetJobAsync(string jobId)
        {
            var jobJsonString = await client.HGetAsync(_jobsHashKey, jobId);
            if (jobJsonString == null)
            {
                return null;
            }

            var jobDesc = JsonSerializer.Deserialize<DistributedActuatorStoreRedisHash>(jobJsonString);
            var valueOrDefault = _localJobs.GetValueOrDefault(jobId);
            if (valueOrDefault != null && jobDesc != null) ;
            {
                valueOrDefault!.NumberOfRuns = jobDesc!.NumberOfRuns;
                valueOrDefault.JobStatus = jobDesc.Status;
                valueOrDefault.NumberOfErrors = jobDesc.NumberOfErrors;
            }
            return valueOrDefault;
        }

        public Actuator? GetJob(string jobId)
        {
            var jobJsonString = client.HGet(_jobsHashKey, jobId);
            if (jobJsonString == null)
            {
                return null;
            }

            var jobDesc = JsonSerializer.Deserialize<DistributedActuatorStoreRedisHash>(jobJsonString);
            var valueOrDefault = _localJobs.GetValueOrDefault(jobId);
            if (valueOrDefault != null && jobDesc != null) ;
            {
                valueOrDefault!.NumberOfRuns = jobDesc!.NumberOfRuns;
                valueOrDefault.JobStatus = jobDesc.Status;
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
                if (_localJobs.TryGetValue(pair.Key, out var actuator))
                {
                    var distributedActuatorStoreRedisHash =
                        JsonSerializer.Deserialize<DistributedActuatorStoreRedisHash>(pair.Value);
                    actuator.NumberOfRuns = distributedActuatorStoreRedisHash!.NumberOfRuns;
                    actuator.NumberOfErrors = distributedActuatorStoreRedisHash.NumberOfErrors;
                    actuator.JobStatus = distributedActuatorStoreRedisHash.Status;
                    jobs.TryAdd(pair.Key, actuator);
                }
            }

            return jobs;
        }

        public Task<ConcurrentDictionary<string, Actuator>> GetLocalAllJobAsync()
        {
            return Task.FromResult(_localJobs);
        }

        public bool RemoveJob(string jobId)
        {
            var hDel = client.HDel(_jobsHashKey, jobId);
            return hDel > 0;
        }

        public bool UpdateStatus(string jobId, ActuatorStatus status)
        {
            var jobJsonString = client.HGet(_jobsHashKey, jobId);
            if (jobJsonString == null) return false;
            var jobDesc = JsonSerializer.Deserialize<DistributedActuatorStoreRedisHash>(jobJsonString);
            if (jobDesc == null) return false;
            jobDesc.Status = status;
            client.HSet(_jobsHashKey, jobId, JsonSerializer.Serialize(jobDesc));
            return true;
        }

        public void ClearAllJobs()
        {
            _localJobs.Clear();
            client.HDel(_jobsHashKey);
        }
    }

    internal class DistributedActuatorStoreRedisHash
    {
        public long NumberOfRuns { get; set; }
        public long NumberOfErrors { get; set; }

        public ActuatorStatus Status { get; set; }
    }
}