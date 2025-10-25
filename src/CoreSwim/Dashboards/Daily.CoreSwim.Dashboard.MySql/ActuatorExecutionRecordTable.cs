using FreeSql.DataAnnotations;

namespace Daily.CoreSwim.Dashboard.MySql
{
    [Table(Name = "core_swim_actuator_execution_record")]
    [Index("job_id_index", "job_id")]
    [Index("start_time_index", "start_time")]
    [Index("number_of_runs_index", "number_of_runs")]
    public class ActuatorExecutionRecordTable
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Column(Name = "id", IsPrimary = true, IsIdentity = true)]
        public long Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        [Column(Name = "job_id")]
        public string JobId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Column(Name = "start_time")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [Column(Name = "end_time")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 运行时长
        /// </summary>
        [Column(Name = "duration")]
        public long Duration { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        [Column(Name = "exception", StringLength = -1)]
        public string Exception { get; set; }

        /// <summary>
        /// 触发次数
        /// </summary>
        [Column(Name = "number_of_runs")]
        public long NumberOfRuns { get; set; }

        /// <summary>
        /// 触发类型
        /// </summary>
        [Column(Name = "trigger_type", StringLength = 10)]
        public string TriggerType { get; set; }

        /// <summary>
        /// 执行节点
        /// </summary>
        [Column(Name = "execute_node")]
        public string ExecuteNode { get; set; }

        [Column(Name = "next_run_time")]
        public DateTime? NextRunTime { get; set; }
    }
}