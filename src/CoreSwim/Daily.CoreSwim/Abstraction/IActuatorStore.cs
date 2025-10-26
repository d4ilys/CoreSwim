using Daily.CoreSwim.Actuators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daily.CoreSwim.Abstraction
{
    public interface IActuatorStore
    {
        Task SaveJobAsync(string jobId, Actuator actuator);

        Task UpdateJobAsync(string jobId, Actuator actuator);

        Task<Actuator?> GetJobAsync(string jobId);
        Actuator? GetJob(string jobId);

        Task<ConcurrentDictionary<string, Actuator>> GetAllJobAsync();

        Task<ConcurrentDictionary<string, Actuator>> GetLocalAllJobAsync();

        bool RemoveJob(string jobId);

        bool UpdateStatus(string jobId, ActuatorStatus status);

        void ClearAllJobs();
    }
}