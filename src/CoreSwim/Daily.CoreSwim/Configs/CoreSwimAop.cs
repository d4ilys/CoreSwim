using Daily.CoreSwim.Actuators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daily.CoreSwim.Configs
{
    public class CoreSwimAop
    {
        /// <summary>
        /// 添加任务之前
        /// </summary>
        public Action<ActuatorBuilder>? AddJobBefore;

        /// <summary>
        /// 添加任务之后
        /// </summary>
        public Action<Actuator>? AddJobAfter;

        /// <summary>
        /// 执行任务之前
        /// </summary>
        public Action<Actuator>? ExecuteJobBefore;

        /// <summary>
        /// 执行任务之后
        /// </summary>
        public Action<Actuator>? ExecuteJobAfter;
    }
}