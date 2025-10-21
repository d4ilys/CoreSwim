using Daily.CoreSwim.Abstraction;

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
    }
}