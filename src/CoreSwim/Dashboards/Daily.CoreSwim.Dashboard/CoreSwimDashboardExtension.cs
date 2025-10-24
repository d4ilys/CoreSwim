using System.Text;
using System.Text.Json;
using Daily.CoreSwim.Abstraction;
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
                    var (records, recordsTotal) =
                        await coreSwim.Config.Persistence.GetJobsExecutionRecordsAsync(description.JobId, 0, 10);
                    var body = new ActuatorDescriptionResponseBody
                    {
                        JobId = description.JobId,
                        JobOnline = description.JobOnline,
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
                        ExecutionRecords = records
                    };
                    resultData.Add(body);
                }

                var json = new { data = resultData, total };
                await context.Response.WriteAsJsonAsync(json);
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
                    await coreSwim.ExecuteJobAsync(jobId, CancellationToken.None);
                }

                await context.Response.WriteAsJsonAsync(new { success = true });
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