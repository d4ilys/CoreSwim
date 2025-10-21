using TimeCrontab;

namespace Daily.CoreSwim.Actuators;

public class CronActuator : Actuator
{
    public CronActuator(string expression, object args)
    {
        Crontab = args switch
        {
            // 处理 int/long 转 CronStringFormat
            int or long => Crontab.Parse(expression, (CronStringFormat)int.Parse(args.ToString() ?? string.Empty)),
            // 处理 CronStringFormat
            CronStringFormat format => Crontab.Parse(expression, format),
            // 处理 Macro At
            object[] fields => Crontab.ParseAt(expression, fields),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// <see cref="Crontab"/> 对象
    /// </summary>
    private Crontab Crontab { get; }

    /// <summary>
    /// 计算下一个触发时间
    /// </summary>
    /// <param name="startAt">起始时间</param>
    /// <returns><see cref="DateTime"/></returns>
    public override DateTime? GetNextOccurrence(DateTime startAt)
    {
        return Crontab.GetNextOccurrence(startAt);
    }

}