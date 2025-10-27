using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Cluster.DependencyInjection;
using Daily.CoreSwim.Dashboard;
using Daily.CoreSwim.Dashboard.Cluster;
using FreeRedis;
using FreeSql;
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
    swim.AddJob<MyJob01>("MyJob01",
        CoreSwimActuator.SecondlyAt(10).SetDescription("每分钟的第10秒执行").SetMaxNumberOfErrors(15));
    swim.AddJob<MyJob02>("MyJob02",
        CoreSwimActuator.PeriodMinutes(2).SetDescription("每2分钟执行一次").SetMaxNumberOfRuns(10));
    swim.AddJob<MyJob03>("MyJob03", CoreSwimActuator.DailyAt(15).SetDescription("每天的15点执行"));
    //swim.AddJob<MyJob04>("MyJob04", CoreSwimActuator.DateTime(DateTime.Parse("2025-10-24 14:59:00"))
    //    .SetDescription("2025-10-24 14:50:00 执行"));
    swim.AddJob<MyJob05>("MyJob05",
        CoreSwimActuator.Period(5000 * 60).SetDescription("5000 * 60毫秒执行一次"));
    swim.AddJob<MyJob06>("MyJob06",
        CoreSwimActuator.Period(3000 * 60).SetDescription("3000 * 60毫秒执行一次"));
});

builder.Services.AddCoreSwimDashboard().UseFreeSql(options =>
{
    options.DataType = DataType.MySql;
    options.ConnectionString =
        "Data Source=192.168.1.116;Port=3306;User ID=root;Password=123456; Initial Catalog=core_swim_test;Charset=utf8; SslMode=none;";
});

var app = builder.Build();

app.UseCors("daily");

app.UseCoreSwimDashboard(options => options.DashboardPath = "CoreSwim");

app.MapControllers();

app.Run();