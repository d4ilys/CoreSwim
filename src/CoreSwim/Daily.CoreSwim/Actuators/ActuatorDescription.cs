using System.Text.Json.Serialization;

namespace Daily.CoreSwim.Actuators;

public class ActuatorDescription
{
    /// <summary>
    /// Job ID
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// Job是否在线
    /// </summary>
    public bool JobOnline { get; set; }

    /// <summary>
    /// Job描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 是否在启动时运行
    /// </summary>
    public bool RunOnStart { get; set; }

    /// <summary>
    /// 起始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 最近运行时间
    /// </summary>
    public DateTime? LastRunTime { get; set; }

    /// <summary>
    /// 下一次运行时间
    /// </summary>
    public DateTime? NextRunTime { get; set; }

    /// <summary>
    /// 触发次数
    /// </summary>
    public long NumberOfRuns { get; set; }

    /// <summary>
    /// 最大触发次数
    /// </summary>
    /// <remarks>
    /// <para>0：不限制</para>
    /// <para>n：N 次</para>
    /// </remarks>
    public long MaxNumberOfRuns { get; set; }

    /// <summary>
    /// 出错次数
    /// </summary>
    public long NumberOfErrors { get; set; }

    /// <summary>
    /// 最大出错次数
    /// </summary>
    /// <remarks>
    /// <para>0：不限制</para>
    /// <para>n：N 次</para>
    /// </remarks>
    public long MaxNumberOfErrors { get; set; }

    /// <summary>
    /// 周期方式
    /// </summary>
    public string RepeatInterval { get; set; }
}