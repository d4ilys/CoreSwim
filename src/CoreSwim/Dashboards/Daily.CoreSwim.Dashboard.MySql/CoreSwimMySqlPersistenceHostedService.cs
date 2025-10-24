using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daily.CoreSwim.Actuators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Daily.CoreSwim.Dashboard.MySql
{
    internal class CoreSwimMySqlPersistenceHostedService : IHostedService
    {
        public CoreSwimMySqlPersistenceHostedService(IServiceProvider provider, IFreeSql freeSql)
        {
            var coreSwim = provider.GetService<ICoreSwim>();

            if (coreSwim == null)
            {
                throw new Exception("请先调用AddCoreSwim方法");
            }

            coreSwim.Config.Persistence = new CoreSwimMySqlPersistence(coreSwim, freeSql);

            //同步数据表
            var actuatorExecutionRecordTable =
                freeSql.CodeFirst.GetTableByEntity(typeof(ActuatorExecutionRecordTable));
            var actuatorDescriptionTable = freeSql.CodeFirst.GetTableByEntity(typeof(ActuatorDescriptionTable));

            if (!freeSql.DbFirst.ExistsTable(actuatorExecutionRecordTable.DbName))
            {
                freeSql.CodeFirst.SyncStructure(typeof(ActuatorExecutionRecordTable));
            }

            if (!freeSql.DbFirst.ExistsTable(actuatorDescriptionTable.DbName))
            {
                freeSql.CodeFirst.SyncStructure(typeof(ActuatorDescriptionTable));
            }

            coreSwim.AddJob<CleanUpExpiredDataJob>("SystemCleanUpExpiredData",
                CoreSwimActuator.Daily().SetDescription("清理过期数据 <系统内置> "));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}