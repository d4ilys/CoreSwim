using Daily.CoreSwim.Actuators;
using Daily.CoreSwim.Dashboard;
using WebApplicationSimple;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy("daily",
    policyBuilder => { policyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); }));

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

app.UseCors("daily");

app.UseCoreSwimDashboard(options => options.DashboardPath = "CoreSwim");

app.MapControllers();

app.Run();