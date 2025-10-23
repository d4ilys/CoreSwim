using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Dashboard;
using Web.Simple;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy("daily",
    policyBuilder => { policyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); }));

builder.Services.AddCoreSwim(swim =>
{
    swim.AddJob<MyJob01>(CoreSwimActuator.SecondlyAt(10).SetDescription("这个作业在每分钟的第10秒执行"));
    swim.AddJob<MyJob02>(CoreSwimActuator.Period(2000).SetDescription("每2秒执行一次"));
    swim.AddJob<MyJob03>(CoreSwimActuator.DailyAt(13).SetDescription("每天13点开始执行"));
    swim.AddJob<MyJob04>(CoreSwimActuator.DateTime(DateTime.Parse("2025-10-23 11:30:00"))
        .SetDescription("2025-10-23 11:30:00准时执行啊"));
    swim.AddJob<MyJob05>(CoreSwimActuator.Period(5000).SetDescription("5000毫秒周期执行"));
    swim.AddJob<MyJob06>(CoreSwimActuator.Period(3000).SetDescription("3秒周期执行"));
});

var app = builder.Build();

app.UseCors("daily");

app.UseCoreSwimDashboard(options => options.DashboardPath = "CoreSwim");

app.MapControllers();

app.Run();