using TimeCrontab;

namespace Daily.CoreSwim.Actuators;

public class CoreSwimActuator
{
    /// <summary>
    /// 创建指定时间作业触发器构建器
    /// </summary>
    /// <param name="dateTime">指定的时间开始执行</param>
    /// <returns></returns>
    public static ActuatorBuilder DateTime(DateTime dateTime)
    {
        return new ActuatorBuilder().DateTime(dateTime);
    }

    /// <summary>
    /// 创建毫秒周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（毫秒）</param>
    public static ActuatorBuilder Period(long interval)
    {
        return new ActuatorBuilder().Period(interval);
    }

    /// <summary>
    /// 创建秒周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（秒）</param>
    public static ActuatorBuilder PeriodSeconds(long interval)
    {
        return Period(interval * 1000);
    }

    /// <summary>
    /// 创建分钟周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（分钟）</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder PeriodMinutes(long interval)
    {
        return Period(interval * 1000 * 60);
    }

    /// <summary>
    /// 创建小时周期（间隔）作业触发器构建器
    /// </summary>
    /// <param name="interval">间隔（小时）</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder PeriodHours(long interval)
    {
        return Period(interval * 1000 * 60 * 60);
    }

    /// <summary>
    /// 创建 Cron 表达式作业触发器构建器
    /// </summary>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型，默认 <see cref="CronStringFormat.Default"/></param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Cron(string expression, CronStringFormat format = CronStringFormat.Default)
    {
        return new ActuatorBuilder().Cron(expression, format);
    }

    /// <summary>
    /// 创建 Cron 表达式作业触发器构建器
    /// </summary>
    /// <param name="expression">Cron 表达式</param>
    /// <param name="args">动态参数类型，支持 <see cref="int"/> 和 object[]</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Cron(string expression, object args)
    {
        return new ActuatorBuilder().Cron(expression, args);
    }


    /// <summary>
    /// 创建每秒开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Secondly()
    {
        return Cron("@secondly");
    }

    /// <summary>
    /// 创建指定特定秒开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder SecondlyAt(params object[] fields)
    {
        return Cron("@secondly", fields);
    }

    /// <summary>
    /// 创建每分钟开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Minutely()
    {
        return Cron("@minutely");
    }

    /// <summary>
    /// 创建每分钟特定秒开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder MinutelyAt(params object[] fields)
    {
        return Cron("@minutely", fields);
    }

    /// <summary>
    /// 创建每小时开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Hourly()
    {
        return Cron("@hourly");
    }

    /// <summary>
    /// 创建每小时特定分钟开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder HourlyAt(params object[] fields)
    {
        return Cron("@hourly", fields);
    }

    /// <summary>
    /// 创建每天（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Daily()
    {
        return Cron("@daily");
    }

    /// <summary>
    /// 创建每天特定小时开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder DailyAt(params object[] fields)
    {
        return Cron("@daily", fields);
    }

    /// <summary>
    /// 创建每月1号（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Monthly()
    {
        return Cron("@monthly");
    }

    /// <summary>
    /// 创建每月特定天（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder MonthlyAt(params object[] fields)
    {
        return Cron("@monthly", fields);
    }

    /// <summary>
    /// 创建每周日（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Weekly()
    {
        return Cron("@weekly");
    }

    /// <summary>
    /// 创建每周特定星期几（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder WeeklyAt(params object[] fields)
    {
        return Cron("@weekly", fields);
    }

    /// <summary>
    /// 创建每年1月1号（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Yearly()
    {
        return Cron("@yearly");
    }

    /// <summary>
    /// 创建每年特定月1号（午夜）开始作业触发器构建器
    /// </summary>
    /// <param name="fields">字段值</param>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder YearlyAt(params object[] fields)
    {
        return Cron("@yearly", fields);
    }

    /// <summary>
    /// 创建每周一至周五（午夜）开始作业触发器构建器
    /// </summary>
    /// <returns><see cref="ActuatorBuilder"/></returns>
    public static ActuatorBuilder Workday()
    {
        return Cron("@workday");
    }
}