using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daily.CoreSwim.Abstraction;

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
}