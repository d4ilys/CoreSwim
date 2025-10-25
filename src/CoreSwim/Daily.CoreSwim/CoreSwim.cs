using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Configs;
using Daily.CoreSwim.Retaining;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;

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
            actuator.JobOnline = true;
            actuator.StartTime = Penetrates.GetStandardDateTime(DateTime.Now);

            Config.ActuatorStore.SaveJobAsync(jobId, actuator);
            Config.Logger.Info<CoreSwim>($"<{actuator.JobId}> Already added and initialized completed.");
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
            Config.Aop.ExecuteJobBefore?.Invoke(actuator);
            var startTime = DateTime.Now;
            //判断可以执行运行
            var canItBeExecutedAsync = await CanItBeExecutedAsync(actuator.JobId);
            var actuatorExecutionRecord = new ActuatorExecutionRecord
            {
                JobId = actuator.JobId,
                StartTime = startTime,
            };
            if (canItBeExecutedAsync)
            {
                var sw = Stopwatch.StartNew();

                //判断是否超过最大运行次数
                if (actuator.MaxNumberOfRuns != 0)
                {
                    if (actuator.NumberOfRuns >= actuator.MaxNumberOfRuns)
                    {
                        Config.Logger.Info<CoreSwim>(
                            $"<{actuator.JobId}> The number of runs has reached the maximum limit and will be deleted.");
                        RemoveJob(actuator.JobId);
                    }
                }

                //判断是否超过最大错误次数
                if (actuator.MaxNumberOfErrors != 0)
                {
                    if (actuator.NumberOfErrors >= actuator.MaxNumberOfErrors)
                    {
                        Config.Logger.Info<CoreSwim>(
                            $"<{actuator.JobId}> The number of errors has reached the maximum limit and will be deleted.");
                        RemoveJob(actuator.JobId);
                    }
                }

                try
                {
                    var jobIns = CreateJobIns(actuator, stoppingToken);
                    await jobIns.ExecuteAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    actuatorExecutionRecord.Exception = e.ToString();
                    actuator.NumberOfErrors++;
                    Config.Logger.Error<CoreSwim>(
                        $" {DateTime.Now:yyyy-MM-dd HH:mm:ss} <{actuator.JobId}> Execution exception occurred .");
                }
                finally
                {
                    sw.Stop();
                    actuatorExecutionRecord.EndTime = DateTime.Now;
                    actuatorExecutionRecord.TriggerType = triggerType;
                    actuatorExecutionRecord.Duration = sw.ElapsedMilliseconds;
                    actuatorExecutionRecord.ExecuteNode = Environment.MachineName;

                    actuator.LastRunTime = startAt;
                    actuatorExecutionRecord.NumberOfRuns = actuator.NumberOfRuns;
                    Config.Logger.Info<CoreSwim>(
                        $"<{actuator.JobId}> Implemented at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
                }
            }

            actuator.NextRunTime = actuator.GetNextOccurrence(startAt);

            if (canItBeExecutedAsync)
            {
                actuatorExecutionRecord.NextRunTime = actuator.NextRunTime;
                Config.Persistence?.SaveJobsExecutionRecordAsync(actuatorExecutionRecord);
                await Config.ActuatorStore.UpdateJobAsync(actuator.JobId, actuator);
            }

            Config.Aop.ExecuteJobAfter?.Invoke(actuator);
        }

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