# 定时作业平台

[d4ilys/CoreSwim: 支持分布式的 定时调度作业平台](https://github.com/d4ilys/CoreSwim)

✨ [简单实例化](#实例化) <br />
☁️ [DependencyInjection](#DependencyInjection) <br />🎭 [Dashboard](#Dashboard) <br />

## 实例化

### 单机模式

> 适合控制台项目
>
> Install-Pakcage Daily.CoreSwim

**需要注意的是 CoreSwim 一定要`单例模式`**

~~~C#
var coreSwim = new CoreSwim();
coreSwim.AddJob<MyJob01>(CoreSwimActuator.Period(4000));   // 每4秒执行
coreSwim.AddJob<MyJob02>(CoreSwimActuator.DailyAt(2));  // 每天凌晨2点执行
await coreSwim.StartAsync(CancellationToken.None); 
~~~

实现IJob

~~~c#
//这里不支持有参构造函数
public class MyJob01 : IJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第一个任务我执行了..");
        return Task.CompletedTask;
    }
}
~~~

### 集群模式

> 集群模式下，所有节点在执行任务时，会互相抢任务，确保同一个任务只在一个节点上执行
>
> Daily.CoreSwim.Cluster

**需要注意的是 CoreSwimCluster 一点要单例模式**

~~~C#
var redis = new RedisClient("localhost:6379,password=111111");
//分布式模式下依赖Redis
var coreSwim = new CoreSwimCluster(redis);
coreSwim.AddJob<MyJob01>(CoreSwimActuator.Period(4000));   // 每4秒执行
coreSwim.AddJob<MyJob02>(CoreSwimActuator.DailyAt(2));  // 每天凌晨2点执行
await coreSwim.StartAsync(CancellationToken.None); 
~~~

实现IJob

~~~c#
//这里不支持有参构造函数
public class MyJob01 : IJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}-我是第一个任务我执行了..");
        return Task.CompletedTask;
    }
}
~~~

### Job类支持构造函数注入

1.可以安装提供的DependencyInjection

2.由于Job类反射创建，可以配置使用官方自带的ActivatorUtilities 来支持构造函数注入

~~~C#
coreSwim.Config.ActivatorCreateInstance = type => ActivatorUtilities.CreateInstance(provider, type);
~~~

## DependencyInjection

在使用ASP.NET Core / WorkerService时，可以使用 DependencyInjection

### 单机模式

> Install-Package Daily.CoreSwim.DependencyInjection

~~~C#
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Dashboard;
using WebApplicationSimple;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreSwim(swim =>
{
    swim.AddJob<MyJob01>(CoreSwimActuator.SecondlyAt(10).SetDescription("每分钟的第10秒执行"));
    swim.AddJob<MyJob02>(CoreSwimActuator.Period(2000).SetDescription("每20秒执行一次"));
    swim.AddJob<MyJob03>(CoreSwimActuator.DailyAt(13).SetDescription("每天的13点执行"));
    swim.AddJob<MyJob04>(CoreSwimActuator.DateTime(DateTime.Parse("2025-10-23 11:30:00"))
        .SetDescription("2025-10-24 11:30:00执行"));
    swim.AddJob<MyJob05>(CoreSwimActuator.Period(5000).SetDescription("5000毫秒执行一次"));
    swim.AddJob<MyJob06>(CoreSwimActuator.Period(3000).SetDescription("3000毫秒执行一次"));
});
~~~

### 集群模式

> Install-Package Daily.CoreSwim.Cluster.DependencyInjection

~~~C#
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Dashboard;
using Daily.CoreSwim.Dashboard.Cluster;
using Daily.CoreSwim.Dashboard.MySql;
using FreeRedis;
using WebApplicationClusterSimple;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(provider =>
{
    var redis = new RedisClient("192.168.1.123:6379,password=111111");
    return redis;
});

builder.Services.AddCoreSwimCluster(provider => provider.GetRequiredService<RedisClient>(), swim =>
{
    swim.AddJob<MyJob01>("MyJob01", CoreSwimActuator.SecondlyAt(10).SetDescription("每分钟的第10秒执行"));
    swim.AddJob<MyJob02>("MyJob02", CoreSwimActuator.PeriodMinutes(2).SetDescription("每2分钟执行一次"));
    swim.AddJob<MyJob03>("MyJob03", CoreSwimActuator.DailyAt(15).SetDescription("每天的15点执行"));
    swim.AddJob<MyJob04>("MyJob04", CoreSwimActuator.DateTime(DateTime.Parse("2025-10-24 14:59:00"))
        .SetDescription("2025-10-24 14:50:00 执行"));
    swim.AddJob<MyJob05>("MyJob05",
        CoreSwimActuator.Period(5000 * 60).SetDescription("5000 * 60毫秒执行一次"));
    swim.AddJob<MyJob06>("MyJob06",
        CoreSwimActuator.Period(3000 * 60).SetDescription("3000 * 60毫秒执行一次"));
});
~~~

## Dashboard

### 单机模式

> Install-Package Daily.CoreSwim.Dashboard

~~~C#
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Dashboard;
using WebApplicationSimple;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreSwim(swim =>
{
    swim.AddJob<MyJob01>(CoreSwimActuator.SecondlyAt(10).SetDescription("每分钟的第10秒执行"));
    swim.AddJob<MyJob02>(CoreSwimActuator.Period(2000).SetDescription("每20秒执行一次"));
    swim.AddJob<MyJob03>(CoreSwimActuator.DailyAt(13).SetDescription("每天的13点执行"));
    swim.AddJob<MyJob04>(CoreSwimActuator.DateTime(DateTime.Parse("2025-10-23 11:30:00"))
        .SetDescription("2025-10-24 11:30:00执行"));
    swim.AddJob<MyJob05>(CoreSwimActuator.Period(5000).SetDescription("5000毫秒执行一次"));
    swim.AddJob<MyJob06>(CoreSwimActuator.Period(3000).SetDescription("3000毫秒执行一次"));
});

var app = builder.Build();

//Dashboard
app.UseCoreSwimDashboard(options => options.DashboardPath = "CoreSwim");

app.Run();
~~~

![Simple](./images/Simple.png)

### 集群模式

> Install-Package Daily.CoreSwim.Dashboard.Cluster

**集群模式存储基于FreeSql，根据自身条件选取对应数据库**

**安装FreeSql对应数据库的Provider**

> Install-Package FreeSql.Provider.MySqlConnector

~~~c#
using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Dashboard;
using Daily.CoreSwim.Dashboard.Cluster;
using Daily.CoreSwim.Dashboard.MySql;
using FreeRedis;
using WebApplicationClusterSimple;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy("daily",
    policyBuilder => { policyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); }));

builder.Services.AddSingleton(provider =>
{
    var redis = new RedisClient("192.168.1.123:6379,password=111111");
    return redis;
});

builder.Services.AddCoreSwimCluster(provider => provider.GetRequiredService<RedisClient>(), swim =>
{
    swim.AddJob<MyJob01>("MyJob01", CoreSwimActuator.SecondlyAt(10).SetDescription("每分钟的第10秒执行"));
    swim.AddJob<MyJob02>("MyJob02", CoreSwimActuator.PeriodMinutes(2).SetDescription("每2分钟执行一次"));
    swim.AddJob<MyJob03>("MyJob03", CoreSwimActuator.DailyAt(15).SetDescription("每天的15点执行"));
    swim.AddJob<MyJob04>("MyJob04", CoreSwimActuator.DateTime(DateTime.Parse("2025-10-24 14:59:00"))
        .SetDescription("2025-10-24 14:50:00 执行"));
    swim.AddJob<MyJob05>("MyJob05",
        CoreSwimActuator.Period(5000 * 60).SetDescription("5000 * 60毫秒执行一次"));
    swim.AddJob<MyJob06>("MyJob06",
        CoreSwimActuator.Period(3000 * 60).SetDescription("3000 * 60毫秒执行一次"));
});

builder.Services.AddCoreSwimDashboard().UseFreeSql(options =>
{
    options.DataType = DataType.MySql;
    options.ConnectionString =
        "Data Source=192.168.1.123;Port=3306;User ID=root;Password=123456; Initial Catalog=core_swim_test;Charset=utf8; SslMode=none;";
});

var app = builder.Build();

//Dashboard
app.UseCoreSwimDashboard(options => options.DashboardPath = "CoreSwim");

app.Run();
~~~

![Cluster](./images/Cluster.png)