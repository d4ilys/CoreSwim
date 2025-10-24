using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daily.CoreSwim.Abstraction;

namespace Daily.CoreSwim.Actuators
{
    internal class ActuatorStore : IActuatorStore
    {
        private readonly ConcurrentDictionary<string, Actuator> _jobs = new();

        public Task SaveJobAsync(string jobId, Actuator actuator)
        {
            _jobs.TryAdd(jobId, actuator);
            return Task.CompletedTask;
        }

        public Task UpdateJobAsync(string jobId, Actuator actuator)
        {
            return Task.CompletedTask;
        }

        public Task<Actuator?> GetJobAsync(string jobId)
        {
            return Task.FromResult(_jobs.GetValueOrDefault(jobId));
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