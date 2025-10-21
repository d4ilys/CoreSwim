using Daily.CoreSwim.Actuators;

namespace Daily.CoreSwim.Abstraction
{
    public class CoreSwimPersistence(CoreSwim coreSwim)
    {
        private readonly List<ActuatorDescription> _actuatorDescriptions = new();

        private readonly List<ActuatorExecutionRecord> _actuatorExecutionRecords = new();

        /// <summary>
        /// CoreSwim实例
        /// </summary>
        public CoreSwim CoreSwim { get; set; } = coreSwim;

        /// <summary>
        /// 保存任务
        /// </summary>
        /// <param name="actuatorDescription"></param>
        /// <returns></returns>
        public virtual Task SaveJobsAsync(ActuatorDescription actuatorDescription)
        {
            _actuatorDescriptions.Add(actuatorDescription);
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
        /// 删除任务
        /// </summary>
        public virtual Task DeleteJobAsync(string jobId, Action coreSwimDeleteJob)
        {
            _actuatorDescriptions.RemoveAll(x => x.JobId == jobId);
            coreSwimDeleteJob();
            return Task.CompletedTask;
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
            int pageNumber,
            int pageSize)
        {
            // 分页
            var data = _actuatorExecutionRecords
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
            var total = _actuatorExecutionRecords.Count;
            var result = (data, total);
            return Task.FromResult(result);
        }
    }
}