using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daily.CoreSwim.Actuators
{
    public class ActuatorExecutionRecord
    {
        /// <summary>
        /// 作业ID
        /// </summary>
        public string JobId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 运行时长
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// 触发次数
        /// </summary>
        public long NumberOfRuns { get; set; }

        /// <summary>
        /// 触发类型
        /// </summary>
        public string TriggerType { get; set; }
    }
}