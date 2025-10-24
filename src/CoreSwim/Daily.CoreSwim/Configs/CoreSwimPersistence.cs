using Daily.CoreSwim.Actuators;

namespace Daily.CoreSwim.Configs
{
    public class CoreSwimPersistence(ICoreSwim coreSwim)
    {
        private readonly List<ActuatorDescription> _actuatorDescriptions = new();

        private readonly List<ActuatorExecutionRecord> _actuatorExecutionRecords = new();

        /// <summary>
        /// CoreSwim实例
        /// </summary>
        public ICoreSwim CoreSwim { get; set; } = coreSwim;

        /// <summary>
        /// 保存任务
        /// </summary>
        /// <param name="actuatorParams"></param>
        /// <returns></returns>
        public virtual Task SaveJobsAsync(ActuatorDescription actuatorParams)
        {
            _actuatorDescriptions.Add(actuatorParams);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 移除CoreSwim中的任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public void DeleteCoreSwimJob(string jobId)
        {
            CoreSwim.RemoveJob(jobId);
        }


        /// <summary>
        /// 获取任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual Task<(IEnumerable<ActuatorDescription>, int)> GetJobsAsync(int pageNumber,
            int pageSize, string? jobId = null)
        {
            IEnumerable<ActuatorDescription> query = _actuatorDescriptions;

            if (!string.IsNullOrEmpty(jobId))
            {
                query = query.Where(a => a.JobId == jobId);
            }

            // 分页
            var data = query
                .OrderByDescending(a => a.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
            var total = _actuatorDescriptions.Count;
            var result = (data, total);
            return Task.FromResult(result);
        }

        /// <summary>
        /// 保存任务执行记录
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public virtual Task SaveJobsExecutionRecordAsync(ActuatorExecutionRecord record)
        {
            _actuatorExecutionRecords.Add(record);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取任务执行记录
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual Task<(IEnumerable<ActuatorExecutionRecord>, int)> GetJobsExecutionRecordsAsync(
            string jobId,
            int pageNumber,
            int pageSize)
        {
            // 分页
            var data = _actuatorExecutionRecords
                .Where(a => a.JobId == jobId)
                .OrderByDescending(record => record.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
            foreach (var description in data)
            {
                description.StartTime = new DateTime(description.StartTime.Year,
                    description.StartTime.Month,
                    description.StartTime.Day, description.StartTime.Hour, description.StartTime.Minute,
                    description.StartTime.Second);
                description.EndTime = new DateTime(description.EndTime.Year,
                    description.EndTime.Month,
                    description.EndTime.Day, description.EndTime.Hour, description.EndTime.Minute,
                    description.EndTime.Second);
            }

            var total = _actuatorExecutionRecords.Count;
            var result = (data, total);
            return Task.FromResult(result);
        }
    }
}