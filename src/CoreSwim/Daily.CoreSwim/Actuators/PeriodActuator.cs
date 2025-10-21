namespace Daily.CoreSwim.Actuators;

public class PeriodActuator : Actuator
{
    public PeriodActuator(long interval)
    {
        // 最低运行毫秒数为 100ms
        if (interval < 100)
            throw new InvalidOperationException(
                $"The interval cannot be less than 100ms, but the value is <{interval}ms>.");

        Interval = interval;
    }

    /// <summary>
    /// 间隔（毫秒）
    /// </summary>
    protected long Interval { get; }

    public override DateTime? GetNextOccurrence(DateTime startAt)
    {
        // 获取间隔触发器周期计算基准时间
        var baseTime = StartTime ?? startAt;

        // 处理基准时间大于当前时间
        if (baseTime > startAt)
        {
            return baseTime;
        }

        // 获取从基准时间开始到现在经过了多少个完整周期
        var elapsedMilliseconds = (startAt - baseTime).Ticks / TimeSpan.TicksPerMillisecond;
        var fullPeriods = elapsedMilliseconds / Interval;

        // 获取下一次执行时间
        var nextRunTime = baseTime.AddMilliseconds(fullPeriods * Interval);

        // 确保下一次执行时间是在当前时间之后
        if (startAt >= nextRunTime)
        {
            nextRunTime = nextRunTime.AddMilliseconds(Interval);
        }

        return nextRunTime;
    }


    /// <summary>
    /// 计算间隔单位
    /// </summary>
    /// <returns></returns>
    private string GetUnit()
    {
        return Interval switch
        {
            // 毫秒
            < 1000 => $"{Interval}ms",
            // 秒
            >= 1000 and < 1000 * 60 when Interval % 1000 == 0 => $"{Interval / 1000}s",
            // 分钟
            >= 1000 * 60 and < 1000 * 60 * 60 when Interval % (1000 * 60) == 0 => $"{Interval / (1000 * 60)}min",
            // 小时
            >= 1000 * 60 * 60 and < 1000 * 60 * 60 * 24 when Interval % (1000 * 60 * 60) == 0 =>
                $"{Interval / (1000 * 60 * 60)}h",
            _ => $"{Interval}ms"
        };
    }
}