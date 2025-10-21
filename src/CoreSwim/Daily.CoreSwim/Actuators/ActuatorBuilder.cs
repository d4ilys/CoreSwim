using Daily.CoreSwim.Retaining;

namespace Daily.CoreSwim.Actuators;

public class ActuatorBuilder
{
    private Actuator? _actuator;

    public ActuatorBuilder Period(long interval)
    {
        _actuator = new PeriodActuator(interval);
        return this;
    }

    public ActuatorBuilder Cron(string expression, object args)
    {
        _actuator = new CronActuator(expression, args);
        return this;
    }

    public ActuatorBuilder DateTime(DateTime dateTime)
    {
        _actuator = new Actuator
        {
            NextRunTime = Penetrates.GetStandardDateTime(dateTime)
        };
        return this;
    }

    public ActuatorBuilder SetDescription(string description)
    {
        Validate();
        _actuator!.Description = description;
        return this;
    }


    public ActuatorBuilder SetRunOnStart(bool runOnStart)
    {
        Validate();
        _actuator!.RunOnStart = runOnStart;
        return this;
    }

    public ActuatorBuilder SetMaxNumberOfRuns(long maxNumberOfRuns)
    {
        Validate();
        _actuator!.MaxNumberOfRuns = maxNumberOfRuns;
        return this;
    }

    public ActuatorBuilder SetMaxNumberOfErrors(long maxNumberOfErrors)
    {
        Validate();
        _actuator!.MaxNumberOfErrors = maxNumberOfErrors;
        return this;
    }

    //通用方法判断_actuator是否为null
    public void Validate()
    {
        if (_actuator == null)
        {
            throw new InvalidOperationException("请先调用 Period 或 Cron 方法");
        }
    }


    public ActuatorBuilder SetJobTypeJobInsInstanceBuilder(Func<Type, object> jobInsInstanceBuilder)
    {
        _actuator!.JobInsInstanceBuilder = jobInsInstanceBuilder;
        return this;
    }

    internal Actuator Build()
    {
        return _actuator;
    }
}