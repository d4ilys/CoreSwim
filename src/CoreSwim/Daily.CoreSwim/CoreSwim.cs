using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Configs;
using Daily.CoreSwim.Retaining;
using System.Diagnostics;

namespace Daily.CoreSwim
{
    public class CoreSwim : ICoreSwim
    {
        /// <summary>
        /// 配置
        /// </summary>
        public CoreSwimConfig Config { get; set; }

        public CoreSwim()
        {
            Config = new CoreSwimConfig(this);
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
        /// <returns><see cref="CoreSwim"/></returns>
        public CoreSwim AddJob<TJob>(string jobId, ActuatorBuilder actuatorBuilder)
            where TJob : class, IJob
        {
            Config.Aop.AddJobBefore?.Invoke(actuatorBuilder);
            var actuator = actuatorBuilder.Build();
            actuator.JobId = jobId;
            actuator.JobType = typeof(TJob);
            actuator.JobStatus = ActuatorStatus.Ready;
            actuator.StartTime = Penetrates.GetStandardDateTime(DateTime.Now);
            Config.ActuatorStore.SaveJobAsync(jobId, actuator);
            Config.Aop.AddJobAfter?.Invoke(actuator);
            return this;
        }

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <typeparam name="TJob"></typeparam>
        /// <param name="actuatorBuilder"></param>
        /// <returns></returns>
        public CoreSwim AddJob<TJob>(ActuatorBuilder actuatorBuilder)
            where TJob : class, IJob
        {
            var name = typeof(TJob).FullName;
            if (name != null) AddJob<TJob>(name, actuatorBuilder);
            return this;
        }

        /// <summary>
        /// 开始所有任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            return Task.Factory.StartNew(async () =>
            {
                Config.Logger.Info<CoreSwim>("CoreSwim is starting...");

                await PersistenceJobsAsync();

                await InitializeJobsNextRunTimeAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var isBreak = await BackgroundProcessingAsync(stoppingToken);

                    if (!isBreak) break;
                }
            }, TaskCreationOptions.LongRunning);
        }

        private async Task PersistenceJobsAsync()
        {
            var allJob = await Config.ActuatorStore.GetAllJobAsync();
            foreach (var job in allJob)
            {
                await Config.Persistence.SaveJobsAsync(job.Value);
            }
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool RemoveJob(string jobId)
        {
            return Config.ActuatorStore.RemoveJob(jobId);
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool PulseOnJob(string jobId)
        {
            Config.Persistence.UpdateJobStatus(jobId, ActuatorStatus.Ready);
            return Config.ActuatorStore.UpdateStatus(jobId, ActuatorStatus.Ready);
        }

        /// <summary>
        /// 暂停任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool PauseJob(string jobId)
        {
            Config.Persistence.UpdateJobStatus(jobId, ActuatorStatus.Pause);
            return Config.ActuatorStore.UpdateStatus(jobId, ActuatorStatus.Pause);
        }

        public void StopAsync()
        {
            Config.ActuatorStore.ClearAllJobs();
        }

        protected virtual Task<bool> CanItBeExecutedAsync(string jobId)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// 主动触发任务
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public async Task ExecuteJobAsync(string jobId, CancellationToken stoppingToken)
        {
            var actuator = await Config.ActuatorStore.GetJobAsync(jobId);

            if (actuator == null)
            {
                Config.Logger.Warning<CoreSwim>(
                    $"<{jobId}> The job does not exist and will be deleted.");
                return;
            }

            var startAt = Penetrates.GetStandardDateTime(DateTime.Now);
            await ExecuteJobAsync(actuator, startAt, "主动", stoppingToken);
        }

        private async Task ExecuteJobAsync(Actuator actuator, DateTime startAt, string triggerType,
            CancellationToken stoppingToken)
        {
            // 前置AOP调用
            Config.Aop.ExecuteJobBefore?.Invoke(actuator);

            var startTime = DateTime.Now;
            var executionRecord = new ActuatorExecutionRecord
            {
                JobId = actuator.JobId,
                StartTime = startTime
            };

            // 检查是否允许执行
            var canExecute = await CanItBeExecutedAsync(actuator.JobId);

            if (canExecute)
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    // 检查最大运行次数和错误次数限制
                    var (shouldRun, blockReason) = CheckExecutionConstraints(ref actuator);

                    // 检查作业状态
                    var isReady = CheckJobStatus(ref actuator);

                    if (!shouldRun)
                    {
                        Config.Logger.Info<CoreSwim>($"{GetJobLogPrefix(actuator)} {blockReason}");
                        return;
                    }
                    else if (!isReady)
                    {
                        Config.Logger.Info<CoreSwim>($"{GetJobLogPrefix(actuator)} Job is not ready.");
                        return;
                    }
                    else
                    {
                        // 执行作业
                        var jobInstance = CreateJobIns(actuator, stoppingToken);
                        await jobInstance.ExecuteAsync(stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    executionRecord.Exception = ex.ToString();
                    actuator.NumberOfErrors++;
                    Config.Logger.Error<CoreSwim>(
                        $"{GetJobLogPrefix(actuator)} Execution exception at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                }
                finally
                {
                    sw.Stop();
                    actuator.NextRunTime = actuator.GetNextOccurrence(startAt);
                }

                // 完善执行记录
                executionRecord.EndTime = DateTime.Now;
                executionRecord.TriggerType = triggerType;
                executionRecord.Duration = sw.ElapsedMilliseconds;
                executionRecord.ExecuteNode = Environment.MachineName;
                actuator.LastRunTime = startAt;

                Config.Logger.Info<CoreSwim>(
                    $"{GetJobLogPrefix(actuator)} Executed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // 更新作业状态和执行记录
                UpdateJobAndRecord(actuator, executionRecord, startAt);
                await PersistExecutionData(actuator, executionRecord);
            }

            // 不执行时也需要计算下次运行时间
            actuator.NextRunTime = actuator.GetNextOccurrence(startAt);

            // 后置AOP调用
            Config.Aop.ExecuteJobAfter?.Invoke(actuator);
        }


        #region 辅助方法

        private string GetJobLogPrefix(Actuator actuator) => $"<{actuator.JobId}>";

        // 检查执行约束（最大运行次数/错误次数）
        private (bool CanRun, string? BlockReason) CheckExecutionConstraints(ref Actuator actuator)
        {
            if (actuator.MaxNumberOfRuns != 0 && actuator.NumberOfRuns >= actuator.MaxNumberOfRuns)
            {
                var job = Config.ActuatorStore.GetJob(actuator.JobId);

                if (job?.JobStatus != ActuatorStatus.Restriction)
                {
                    Config.ActuatorStore.UpdateStatus(actuator.JobId, ActuatorStatus.Restriction);
                    Config.Persistence.UpdateJobStatus(actuator.JobId, ActuatorStatus.Restriction);
                }

                return (false, "The number of runs has reached the maximum limit.");
            }

            if (actuator.MaxNumberOfErrors != 0 && actuator.NumberOfErrors >= actuator.MaxNumberOfErrors)
            {
                var job = Config.ActuatorStore.GetJob(actuator.JobId);

                if (job?.JobStatus != ActuatorStatus.Restriction)
                {
                    Config.ActuatorStore.UpdateStatus(actuator.JobId, ActuatorStatus.Restriction);
                    Config.Persistence.UpdateJobStatus(actuator.JobId, ActuatorStatus.Restriction);
                }

                return (false, "The number of errors has reached the maximum limit.");
            }

            return (true, null);
        }

        private bool CheckJobStatus(ref Actuator actuator)
        {
            return actuator.JobStatus == ActuatorStatus.Ready;
        }

        // 更新作业和记录信息
        private void UpdateJobAndRecord(Actuator actuator, ActuatorExecutionRecord record, DateTime startAt)
        {
            actuator.NextRunTime = actuator.GetNextOccurrence(startAt);
            actuator.NumberOfRuns++;

            record.NextRunTime = actuator.NextRunTime;
            record.NumberOfRuns = actuator.NumberOfRuns;
        }

        // 持久化执行数据
        private async Task PersistExecutionData(Actuator actuator, ActuatorExecutionRecord record)
        {
            await Config.Persistence.SaveJobsExecutionRecordAsync(record);

            await Config.ActuatorStore.UpdateJobAsync(actuator.JobId, actuator);
        }

        #endregion

        protected async Task<bool> BackgroundProcessingAsync(CancellationToken stoppingToken)
        {
            var startAt = Penetrates.GetStandardDateTime(DateTime.Now);
            try
            {
                var allJob = await Config.ActuatorStore.GetAllJobAsync();
                //检查是否有任务需要执行
                var nextRunTimes =
                    allJob.Values.Where(actuator =>
                        actuator.NextRunTime.HasValue && actuator.NextRunTime <= startAt);

                //并行执行
                await Parallel.ForEachAsync(nextRunTimes, stoppingToken,
                    async (actuator, token) =>
                    {
                        if (Config.Tenant.Tenants.Any())
                        {
                            _ = Parallel.ForEachAsync(Config.Tenant.Tenants, token, async (tenant, cancellationToken) =>
                            {
                                Config.Tenant.CurrentTenantContextSetting?.Invoke(tenant);
                                await ExecuteJobAsync(actuator, startAt, "系统", cancellationToken);
                            });
                        }
                        else
                        {
                            await ExecuteJobAsync(actuator, startAt, "系统", token);
                        }
                    });
            }
            catch (Exception e)
            {
                Config.Logger.Error<CoreSwim>(
                    $" {DateTime.Now:yyyy-MM-dd HH:mm:ss} Background processing exception occurred.");
            }

            // 等待下一个触发时间
            return await DaleyAsync(startAt, stoppingToken);
        }

        /// <summary>
        /// 调度线程休眠
        /// </summary>
        /// <param name="startAt"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected async Task<bool> DaleyAsync(DateTime startAt, CancellationToken stoppingToken)
        {
            var allJob = await Config.ActuatorStore.GetAllJobAsync();

            // 获取最早触发的时间
            var earliestTriggerTimes =
                allJob.Values.Where(a => a.NextRunTime.HasValue).Where(actuator => actuator.NextRunTime.HasValue)
                    .Select(actuator => actuator.NextRunTime).ToList();

            if (earliestTriggerTimes.Any())
            {
                var earliestTriggerTime = earliestTriggerTimes.Min()!.Value;

                // 计算总休眠时间
                var sleepMilliseconds = (earliestTriggerTime - startAt).TotalMilliseconds;

                // 线程休眠
                await Task.Delay((int)sleepMilliseconds, stoppingToken);
                return true;
            }
            else
            {
                //该线程已经没用了，可以结束
                return false;
            }
        }

        /// <summary>
        /// 初始化任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        protected async Task InitializeJobsNextRunTimeAsync(CancellationToken stoppingToken)
        {
            var startAt = Penetrates.GetStandardDateTime(DateTime.Now);

            var allJob = await Config.ActuatorStore.GetAllJobAsync();

            foreach (var job in allJob)
            {
                if (job.Value.RunOnStart)
                {
                    job.Value.NextRunTime = startAt;
                }
                else
                {
                    job.Value.NextRunTime ??= job.Value.GetNextOccurrence(startAt);
                }

                Config.Logger.Info<CoreSwim>($"<{job.Value.JobId}> Already added and initialized completed.");
            }
        }

        /// <summary>
        /// 创建任务实例
        /// </summary>
        /// <param name="actuator"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        internal IJob CreateJobIns(Actuator actuator, CancellationToken stoppingToken)
        {
            var jobType = actuator.JobType;
            var jobIns = (IJob)Config.ActivatorCreateInstance!(jobType)!;
            return jobIns;
        }
    }
}