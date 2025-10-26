using Daily.CoreSwim.Actuators;
using FreeSql.DataAnnotations;

namespace Daily.CoreSwim.Dashboard.MySql;

[Table(Name = "core_swim_actuator_description")]
[Index("job_id_index", "job_id")]
public class ActuatorDescriptionTable
{
    /// <summary>
    /// Job ID
    /// </summary>
    [Column(Name = "job_id", IsPrimary = true)]
    public string JobId { get; set; }

    /// <summary>
    /// Job是否在线
    /// </summary>
    [Column(Name = "job_status")]
    public ActuatorStatus JobStatus { get; set; }

    /// <summary>
    /// Job描述
    /// </summary>
    [Column(Name = "description", StringLength = 300)]
    public string Description { get; set; }

    /// <summary>
    /// 是否在启动时运行
    /// </summary>
    [Column(Name = "run_on_start")]
    public bool RunOnStart { get; set; }

    /// <summary>
    /// 起始时间
    /// </summary>
    [Column(Name = "start_time")]
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 最近运行时间
    /// </summary>
    [Column(Name = "last_run_time")]
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    /// 下一次运行时间
    /// </summary>
    [Column(Name = "next_run_time")]
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    /// 触发次数
    /// </summary>
    [Column(Name = "number_of_runs")]
    public long NumberOfRuns { get; set; }

    /// <summary>
    /// 最大触发次数
    /// </summary>
    /// <remarks>
    /// <para>0：不限制</para>
    /// <para>n：N 次</para>
    /// </remarks>
    [Column(Name = "max_number_of_runs")]
    public long MaxNumberOfRuns { get; set; }

    /// <summary>
    /// 总出错次数
    /// </summary>
    [Column(Name = "number_of_errors")]
    public long NumberOfErrors { get; set; }

    /// <summary>
    /// 最大出错次数
    /// </summary>
    /// <remarks>
    /// <para>0：不限制</para>
    /// <para>n：N 次</para>
    /// </remarks>
    [Column(Name = "max_number_of_errors")]
    public long MaxNumberOfErrors { get; set; }

    /// <summary>
    /// 周期方式
    /// </summary>
    [Column(Name = "repeat_interval")]
    public string RepeatInterval { get; set; }
}