using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daily.CoreSwim.Abstraction;
using Daily.CoreSwim.Actuators;

namespace Daily.CoreSwim.Dashboard.MySql
{
    internal class CleanUpExpiredDataJob(IFreeSql db, CoreSwimDashboardMySqlOptions options) : IJob
    {
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //ActuatorExecutionRecordTable 只保留指定天数的数据
            var dateTime = DateTime.Now.AddDays(-options.RetainDataDays).Date;
            return db.Delete<ActuatorExecutionRecordTable>()
                .Where(x => x.StartTime < dateTime).ExecuteAffrowsAsync(cancellationToken);
        }
    }

    internal class SyncOnlineStatus(IFreeSql db, ICoreSwim swim) : IJob
    {
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var allJob = await db.Select<ActuatorDescriptionTable>().ToListAsync(cancellationToken);
            var allLocalJob = await swim.Config.ActuatorStore.GetLocalAllJobAsync();
            foreach (var job in allJob.Where(job => !allLocalJob.ContainsKey(job.JobId)))
            {
                await db.Update<ActuatorDescriptionTable>().Set(a => a.JobStatus, ActuatorStatus.Offline)
                    .Where(a => a.JobId == job.JobId)
                    .ExecuteAffrowsAsync(cancellationToken);

                swim.RemoveJob(job.JobId);
            }
        }
    }
}