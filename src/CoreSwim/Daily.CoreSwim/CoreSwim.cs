using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Configs;
using Daily.CoreSwim.Retaining;
using System.Collections.Concurrent;

namespace Daily.CoreSwim
{
    public sealed class CoreSwim
    {
        private readonly ConcurrentDictionary<string, Actuator> _jobs = new();

        /// <summary>
        /// 配置
        /// </summary>
        public CoreSwimConfig Config { get; set; }

        public CoreSwim()
        {
             Config = new CoreSwimConfig(this);
        }

        /// <summary>
        /// 添加作业
        /// </summary>
        /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
        /// <returns><see cref="CoreSwim"/></returns>
        public CoreSwim AddJob<TJob>(string jobId, ActuatorBuilder actuatorBuilder)
            where TJob : class, IJob
        {
            var actuator = actuatorBuilder.Build();
            actuator.JobId = jobId;
            actuator.JobType = typeof(TJob);
            _jobs.TryAdd(jobId, actuator);
            Config.Persistence?.SaveJobsAsync(actuator);
            Config.Logger.Info<CoreSwim>($"<{actuator.JobId}> Already added and initialized completed.");

            return this;
        }

        public CoreSwim AddJob<TJob>(ActuatorBuilder actuatorBuilder)
            where TJob : class, IJob
        {
            var name = typeof(TJob).FullName;
            if (name != null) AddJob<TJob>(name, actuatorBuilder);
            return this;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            Config.Logger.Info<CoreSwim>("CoreSwim is starting...");

            await RunOnStartJobsAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var isBreak = await BackgroundProcessingAsync(stoppingToken);

                if (!isBreak) break;
            }
        }

        /// <summary>
        /// 删除作业
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool RemoveJob(string jobId)
        {
            return _jobs.TryRemove(jobId, out _);
        }

        public void StopAsync()
        {
            _jobs.Clear();
        }

        public Task ExecuteJobAsync(string jobId, CancellationToken stoppingToken)
        {
            var actuator = _jobs[jobId];

            //判断是否超过最大运行次数
            if (actuator.MaxNumberOfRuns != 0)
            {
                if (actuator.NumberOfRuns >= actuator.MaxNumberOfRuns)
                {
                    Config.Logger.Info<CoreSwim>(
                        $"<{actuator.JobId}> The number of runs has reached the maximum limit and will be deleted.");
                    RemoveJob(jobId);
                    return Task.CompletedTask;
                }
            }

            //判断是否超过最大错误次数
            if (actuator.MaxNumberOfErrors != 0)
            {
                if (actuator.NumberOfErrors >= actuator.MaxNumberOfErrors)
                {
                    Config.Logger.Info<CoreSwim>(
                        $"<{actuator.JobId}> The number of errors has reached the maximum limit and will be deleted.");
                    RemoveJob(jobId);
                    return Task.CompletedTask;
                }
            }


            var startAt = Penetrates.GetStandardDateTime(DateTime.Now);
            return ExecuteJobAsync(actuator, startAt, stoppingToken);
        }

        internal async Task ExecuteJobAsync(Actuator actuator, DateTime startAt, CancellationToken stoppingToken)
        {
            try
            {
                var jobIns = CreateJobIns(actuator, stoppingToken);
                await jobIns.ExecuteAsync(stoppingToken);
                actuator.NextRunTime = actuator.GetNextOccurrence(startAt);
                actuator.LastRunTime = startAt;
                actuator.NumberOfRuns++;
                Config.Logger.Info<CoreSwim>(
                    $"<{actuator.JobId}> Implemented at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            }
            catch (Exception e)
            {
                actuator.NumberOfErrors++;
                Config.Logger.Error<CoreSwim>(
                    $" {DateTime.Now:yyyy-MM-dd HH:mm:ss} <{actuator.JobId}> Execution exception occurred .");
            }
        }

        private async Task<bool> BackgroundProcessingAsync(CancellationToken stoppingToken)
        {
            var startAt = Penetrates.GetStandardDateTime(DateTime.Now);
            try
            {
                //检查是否有任务需要执行
                var nextRunTimes =
                    _jobs.Values.Where(actuator => actuator.NextRunTime.HasValue && actuator.NextRunTime <= startAt);

                //并行执行
                await Parallel.ForEachAsync(nextRunTimes, stoppingToken,
                    async (actuator, token) => await ExecuteJobAsync(actuator, startAt, token));
            }
            catch (Exception e)
            {
                Config.Logger.Error<CoreSwim>(
                    $" {DateTime.Now:yyyy-MM-dd HH:mm:ss} Background processing exception occurred.");
            }

            // 等待下一个触发时间
            return await SleepAsync(startAt, stoppingToken);
        }

        private async Task<bool> SleepAsync(DateTime startAt, CancellationToken stoppingToken)
        {
            // 获取最早触发的时间
            var earliestTriggerTimes =
                _jobs.Values.Where(a => a.NextRunTime.HasValue).Where(actuator => actuator.NextRunTime.HasValue)
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
        /// 执行RunOnStart的任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        private async Task RunOnStartJobsAsync(CancellationToken stoppingToken)
        {
            var allJobs = _jobs.Select(pair => pair.Value).ToList();
            var startAt = Penetrates.GetStandardDateTime(DateTime.Now);
            await Parallel.ForEachAsync(allJobs.Where(a => a.RunOnStart), stoppingToken,
                async (actuator, token) => await ExecuteJobAsync(actuator, startAt, token));

            foreach (var actuator in allJobs.Where(a => !a.RunOnStart))
            {
                actuator.NextRunTime ??= actuator.GetNextOccurrence(startAt);
            }
        }

        /// <summary>
        /// 创建作业实例
        /// </summary>
        /// <param name="actuator"></param>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        internal IJob CreateJobIns(Actuator actuator, CancellationToken stoppingToken)
        {
            var jobType = actuator.JobType;
            actuator.JobInsInstanceBuilder ??= (type) =>
                Activator.CreateInstance(type);

            var jobIns = (IJob)actuator.JobInsInstanceBuilder!(jobType)!;
            return jobIns;
        }
    }
}