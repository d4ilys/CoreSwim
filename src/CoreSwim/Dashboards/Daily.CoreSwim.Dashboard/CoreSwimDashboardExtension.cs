using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Daily.CoreSwim.Dashboard
{
    public static class CoreSwimDashboardExtension
    {
        public static WebApplication UseCoreSwimDashboard(this WebApplication app,
            Action<CoreSwimDashboardOptions> options)
        {
            var optionsInternal = new CoreSwimDashboardOptions();

            options(optionsInternal);

            app.MapPost($"{optionsInternal.DashboardPath}/getJobs", async context =>
            {
                var coreSwim = context.RequestServices.GetRequiredService<ICoreSwim>();
                var (data, total) = await coreSwim.Config.Persistence.GetJobsAsync(0, 1000);
                var resultData = new List<ActuatorDescriptionResponseBody>();
                foreach (var description in data)
                {
                    var body = new ActuatorDescriptionResponseBody
                    {
                        JobId = description.JobId,
                        JobStatus = description.JobStatus,
                        Description = description.Description,
                        RunOnStart = description.RunOnStart,
                        StartTime = description.StartTime,
                        LastRunTime = description.LastRunTime,
                        NextRunTime = description.NextRunTime,
                        NumberOfRuns = description.NumberOfRuns,
                        MaxNumberOfRuns = description.MaxNumberOfRuns,
                        NumberOfErrors = description.NumberOfErrors,
                        RepeatInterval = description.RepeatInterval,
                        MaxNumberOfErrors = description.MaxNumberOfErrors,
                    };
                    var iobStatusText = Enum.GetName(description.JobStatus);
                    body.JobStatusText = iobStatusText;
                    resultData.Add(body);
                }

                var json = new { data = resultData, total };

                var jsonText = JsonSerializer.Serialize(json, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new DateTimeConverter() }
                });

                context.Response.ContentType = "application/json;charset=UTF-8";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(jsonText);
            });

            app.MapGet($"{optionsInternal.DashboardPath}/getJobExecuteRecord", async context =>
            {
                var stringValues = context.Request.Query["jobId"];
                //base64解码
                var jobId = stringValues.FirstOrDefault();
                jobId = Encoding.UTF8.GetString(Convert.FromBase64String(jobId!));
                var coreSwim = context.RequestServices.GetRequiredService<ICoreSwim>();
                var (records, recordsTotal) =
                    await coreSwim.Config.Persistence.GetJobsExecutionRecordsAsync(jobId, 0, 10);

                var json = new { data = records, total = recordsTotal };

                var jsonText = JsonSerializer.Serialize(json, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new DateTimeConverter() }
                });

                context.Response.ContentType = "application/json;charset=UTF-8";
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(jsonText);
            });

            app.MapGet($"{optionsInternal.DashboardPath}/pulseOnJob", async context =>
            {
                var stringValues = context.Request.Query["jobId"];
                //base64解码
                var jobId = stringValues.FirstOrDefault();
                jobId = Encoding.UTF8.GetString(Convert.FromBase64String(jobId!));
                var coreSwim = context.RequestServices.GetRequiredService<ICoreSwim>();
                var pulseOnJob = coreSwim.PulseOnJob(jobId);
                await context.Response.WriteAsJsonAsync(new
                {
                    success = pulseOnJob
                });
            });

            app.MapGet($"{optionsInternal.DashboardPath}/pauseJob", async context =>
            {
                var stringValues = context.Request.Query["jobId"];
                //base64解码
                var jobId = stringValues.FirstOrDefault();
                jobId = Encoding.UTF8.GetString(Convert.FromBase64String(jobId!));
                var coreSwim = context.RequestServices.GetRequiredService<ICoreSwim>();
                var pulseOnJob = coreSwim.PauseJob(jobId);
                await context.Response.WriteAsJsonAsync(new
                {
                    success = pulseOnJob
                });
            });

            app.MapGet($"{optionsInternal.DashboardPath}/executeImmediately", async context =>
            {
                var stringValues = context.Request.Query["jobId"];
                //base64解码
                var jobId = stringValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(jobId))
                {
                    var coreSwim = context.RequestServices.GetRequiredService<ICoreSwim>();
                    jobId = Encoding.UTF8.GetString(Convert.FromBase64String(jobId));
                    var actuator = await coreSwim.Config.ActuatorStore.GetJobAsync(jobId);
                    if (actuator!.JobStatus != ActuatorStatus.Ready)
                    {
                        await context.Response.WriteAsJsonAsync(new { success = false, message = "任务触发失败 该任务状态异常" });
                        return;
                    }

                    await coreSwim.ExecuteJobAsync(jobId, CancellationToken.None);
                }

                await context.Response.WriteAsJsonAsync(new { success = true, message = "任务触发成功" });
            });

            app.UseMiddleware<CoreSwimDashboardMiddleware>(optionsInternal);

            return app;
        }

        public static CoreSwimDashboardBuilder AddCoreSwim(this IServiceCollection services, Action<ICoreSwim> options)
        {
            var func = options;
            services.AddSingleton(provider =>
            {
                ICoreSwim coreSwim = new CoreSwim();
                func(coreSwim);
                coreSwim.Config.Logger = new AspNetCoreLogger(provider.GetService<ILogger<ICoreSwimLogger>>()!);
                return coreSwim;
            });

            services.AddHostedService<CoreSwimHostedService>();

            return new CoreSwimDashboardBuilder(services);
        }

        public static CoreSwimDashboardBuilder AddCoreSwimDashboard(this IServiceCollection services)
        {
            return new CoreSwimDashboardBuilder(services);
        }
    }
}