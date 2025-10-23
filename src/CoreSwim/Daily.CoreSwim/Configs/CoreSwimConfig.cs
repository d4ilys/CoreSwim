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
        /// 数据持久化
        /// </summary>
        public CoreSwimPersistence Persistence { get; set; } = new CoreSwimPersistence(coreSwim);

        /// <summary>
        /// 任务存储
        /// </summary>
        public IActuatorStore ActuatorStore { get; set; } = new ActuatorStore();

        /// <summary>
        /// 任务实例构建器
        /// </summary>
        public Func<Type, object> ActivatorCreateInstance { get; set; } = type => Activator.CreateInstance(type);
    }
}