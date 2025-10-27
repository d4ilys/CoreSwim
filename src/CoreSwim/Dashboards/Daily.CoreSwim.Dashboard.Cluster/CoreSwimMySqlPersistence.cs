using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Configs;

namespace Daily.CoreSwim.Dashboard.Cluster
{
    public class CoreSwimMySqlPersistence(ICoreSwim coreSwim, IFreeSql db) : CoreSwimPersistence(coreSwim)
    {
        public override async Task SaveJobsAsync(ActuatorDescription actuatorParams)
        {
            var actuator = await db.Select<ActuatorDescriptionTable>().Where(a => a.JobId == actuatorParams.JobId)
                .ToOneAsync();
            if (actuator != null)
            {
                await db.Update<ActuatorDescriptionTable>()
                    .SetIf(actuatorParams.Description != actuator.Description, a => a.Description,
                        actuatorParams.Description)
                    .SetIf(actuatorParams.RunOnStart != actuator.RunOnStart, a => a.RunOnStart,
                        actuatorParams.RunOnStart)
                    .SetIf(actuatorParams.MaxNumberOfRuns != actuator.MaxNumberOfRuns, a => a.MaxNumberOfRuns,
                        actuatorParams.MaxNumberOfRuns)
                    .SetIf(actuatorParams.MaxNumberOfErrors != actuator.MaxNumberOfErrors, a => a.MaxNumberOfErrors,
                        actuatorParams.MaxNumberOfErrors)
                    .SetIf(actuatorParams.RepeatInterval != actuator.RepeatInterval, a => a.RepeatInterval,
                        actuatorParams.RepeatInterval)
                    .Where(a => a.JobId == actuatorParams.JobId)
                    .ExecuteAffrowsAsync();
            }
            else
            {
                await db.Insert(new ActuatorDescriptionTable
                {
                    JobId = actuatorParams.JobId,
                    JobStatus = actuatorParams.JobStatus,
                    Description = actuatorParams.Description,
                    RunOnStart = actuatorParams.RunOnStart,
                    StartTime = actuatorParams.StartTime,
                    LastRunTime = actuatorParams.LastRunTime,
                    NextRunTime = actuatorParams.NextRunTime,
                    NumberOfRuns = actuatorParams.NumberOfRuns,
                    MaxNumberOfRuns = actuatorParams.MaxNumberOfRuns,
                    NumberOfErrors = actuatorParams.NumberOfErrors,
                    MaxNumberOfErrors = actuatorParams.MaxNumberOfErrors,
                    RepeatInterval = actuatorParams.RepeatInterval
                }).ExecuteAffrowsAsync();
            }
        }

        public override async Task<(IEnumerable<ActuatorDescription>, int)> GetJobsAsync(int pageNumber, int pageSize,
            string? jobId = null)
        {
            IEnumerable<ActuatorDescription> query = await db.Select<ActuatorDescriptionTable>()
                .WhereIf(!string.IsNullOrEmpty(jobId), x => x.JobId == jobId)
                .OrderBy(x => x.StartTime)
                .Page(pageNumber, pageSize)
                .ToListAsync(t => new ActuatorDescription
                {
                    JobId = t.JobId,
                    JobStatus = t.JobStatus,
                    Description = t.Description,
                    RunOnStart = t.RunOnStart,
                    StartTime = t.StartTime,
                    LastRunTime = t.LastRunTime,
                    NextRunTime = t.NextRunTime,
                    NumberOfRuns = t.NumberOfRuns,
                    MaxNumberOfRuns = t.MaxNumberOfRuns,
                    NumberOfErrors = t.NumberOfErrors,
                    MaxNumberOfErrors = t.MaxNumberOfErrors,
                    RepeatInterval = t.RepeatInterval
                });
            return (query, 0);
        }

        public override async Task SaveJobsExecutionRecordAsync(ActuatorExecutionRecord record)
        {
            //获取最大的number_of_runs 
            var maxNumberOfRuns = await db.Select<ActuatorDescriptionTable>()
                .Where(x => x.JobId == record.JobId)
                .MaxAsync(a => a.NumberOfRuns);

            record.NumberOfRuns = maxNumberOfRuns + 1;

            if (!string.IsNullOrWhiteSpace(record.Exception))
            {
                //更新Job表
                await db.Update<ActuatorDescriptionTable>()
                    .Set(x => x.NumberOfErrors + 1)
                    .Where(a => a.JobId == record.JobId)
                    .ExecuteAffrowsAsync();
            }

            //更新Job表
            await db.Update<ActuatorDescriptionTable>()
                .Set(x => x.LastRunTime,
                    new DateTime(record.StartTime.Year, record.StartTime.Month, record.StartTime.Day,
                        record.StartTime.Hour, record.StartTime.Minute, record.StartTime.Second))
                .Set(x => x.NextRunTime, record.NextRunTime)
                .Set(x => x.NumberOfRuns, record.NumberOfRuns)
                .Where(a => a.JobId == record.JobId)
                .ExecuteAffrowsAsync();

            await db.Insert(new ActuatorExecutionRecordTable
            {
                JobId = record.JobId,
                StartTime = record.StartTime,
                EndTime = record.EndTime,
                Duration = record.Duration,
                Exception = record.Exception,
                NumberOfRuns = record.NumberOfRuns,
                TriggerType = record.TriggerType,
                ExecuteNode = record.ExecuteNode,
            }).ExecuteAffrowsAsync();
        }

        public override async Task<(IEnumerable<ActuatorExecutionRecord>, int)> GetJobsExecutionRecordsAsync(
            string jobId,
            int pageNumber, int pageSize)
        {
            IEnumerable<ActuatorExecutionRecord> query = await db.Select<ActuatorExecutionRecordTable>()
                .Where(x => x.JobId == jobId)
                .OrderByDescending(x => x.StartTime)
                .Page(pageNumber, pageSize)
                .ToListAsync(t => new ActuatorExecutionRecord
                    {
                        JobId = t.JobId,
                        StartTime = t.StartTime,
                        EndTime = t.EndTime,
                        Duration = t.Duration,
                        Exception = t.Exception,
                        NumberOfRuns = t.NumberOfRuns,
                        TriggerType = t.TriggerType,
                    }
                );
            return (query, 0);
        }

        public override void UpdateJobStatus(string jobId, ActuatorStatus status)
        {
            db.Update<ActuatorDescriptionTable>()
                .Set(x => x.JobStatus, status)
                .Where(a => a.JobId == jobId)
                .ExecuteAffrows();
        }
    }
}