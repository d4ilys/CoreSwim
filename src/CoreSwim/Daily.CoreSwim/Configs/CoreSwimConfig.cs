using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;

namespace Daily.CoreSwim.Configs
{
    public class CoreSwimConfig(CoreSwim coreSwim)
    {
        /// <summary>
        /// 日志
        /// </summary>
        public ICoreSwimLogger Logger { get; set; } = new CoreSwimLogger();

        /// <summary>
        /// 任务及执行日志持久化
        /// </summary>
        public CoreSwimPersistence Persistence { get; set; } = new CoreSwimPersistence(coreSwim);

        /// <summary>
        /// 所有任务存储方式
        /// </summary>
        public IActuatorStore ActuatorStore { get; set; } = new ActuatorStore();

        /// <summary>
        /// 任务实例构建器
        /// </summary>
        public Func<Type, object> ActivatorCreateInstance { get; set; } = type => Activator.CreateInstance(type);

        /// <summary>
        /// 拦截器
        /// </summary>
        public CoreSwimAop Aop { get; set; } = new CoreSwimAop();

        /// <summary>
        /// 租户
        /// </summary>
        public CoreSwimTenant Tenant { get; set; } = new CoreSwimTenant();
    }
}