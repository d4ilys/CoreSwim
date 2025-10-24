using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Daily.CoreSwim.Actuators;
using FreeSql;
using Microsoft.Extensions.DependencyInjection;

namespace Daily.CoreSwim.Dashboard.MySql
{
    public static class CoreSwimDashboardBuilderExtension
    {
        public static CoreSwimDashboardBuilder UseMySql(this CoreSwimDashboardBuilder builder,
            Action<CoreSwimDashboardMySqlOptions> options)
        {
            var optionsInternal = new CoreSwimDashboardMySqlOptions();

            options(optionsInternal);

            builder.Service.AddSingleton(optionsInternal);

            builder.Service.AddSingleton(_ =>
                new FreeSqlBuilder().UseConnectionString(DataType.MySql,
                        $"Data Source={optionsInternal.Host};Port={optionsInternal.Port};User ID={optionsInternal.User};Password={optionsInternal.Password}; Initial Catalog={optionsInternal.DatabaseName};Charset=utf8; SslMode=none;")
                    .UseAdoConnectionPool(true)
                    .Build()
            );

            builder.Service.AddHostedService<CoreSwimMySqlPersistenceHostedService>();

            return builder;
        }
    }
}