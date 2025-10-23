using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Configs;

namespace Daily.CoreSwim;

public interface ICoreSwim
{
    /// <summary>
    /// 配置
    /// </summary>
    public CoreSwimConfig Config { get; internal set; }

    /// <summary>
    /// 添加作业
    /// </summary>
    /// <typeparam name="TJob"><see cref="IJob"/> 实现类型</typeparam>
    /// <returns><see cref="CoreSwim"/></returns>
    public CoreSwim AddJob<TJob>(string jobId, ActuatorBuilder actuatorBuilder)
        where TJob : class, IJob;

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <typeparam name="TJob"></typeparam>
    /// <param name="actuatorBuilder"></param>
    /// <returns></returns>
    public CoreSwim AddJob<TJob>(ActuatorBuilder actuatorBuilder)
        where TJob : class, IJob;

    public Task StartAsync(CancellationToken stoppingToken);

    /// <summary>
    /// 删除作业
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    public bool RemoveJob(string jobId);

    public void StopAsync();

    public Task ExecuteJobAsync(string jobId, CancellationToken stoppingToken);
}