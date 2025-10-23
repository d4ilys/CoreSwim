using System.Collections.Concurrent;
using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;

namespace Daily.CoreSwim.Distributed
{
    internal class DistributedActuatorStore : IActuatorStore
    {
        private readonly ConcurrentDictionary<string, Actuator> _jobs = new();

        public Task SaveJobsAsync(string jobId, Actuator actuator)
        {
            _jobs.TryAdd(jobId, actuator);
            return Task.CompletedTask;
        }

        public Task<Actuator> GetJobAsync(string jobId)
        {
            return Task.FromResult(_jobs[jobId]);
        }

        public Task<ConcurrentDictionary<string, Actuator>> GetAllJobAsync()
        {
            return Task.FromResult(_jobs);
        }

        public bool RemoveJob(string jobId)
        {
            return _jobs.TryRemove(jobId, out _);
        }

        public void ClearAllJobs()
        {
            _jobs.Clear();
        }
    }
}