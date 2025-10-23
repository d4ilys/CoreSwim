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
        Task SaveJobsAsync(string jobId, Actuator actuator);

        Task<Actuator> GetJobAsync(string jobId);

        Task<ConcurrentDictionary<string, Actuator>> GetAllJobAsync();

        bool RemoveJob(string jobId);

        void ClearAllJobs();
    }
}