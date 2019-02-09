using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.Npgsql.EntityFrameworkCore.PostgreSQL
{
    [TestClass]
    public class Cockroachdb_v2_1_4
    {
        [TestMethod]
        public async Task Test()
        {
            var containerName = GetType().FullName;

            var containerPort = 2006;

            var container = new TestUtil.DockerContainer(
                containerName,
                "cockroachdb/cockroach:v2.1.4",
                new[]
                {
                    $"-p {containerPort}:26257",
                    "-e MYSQL_ROOT_PASSWORD=my-secret-pw"
                },
                "start --insecure"
            );

            using (container)
            {
                container.Start();

                await TestUtil.WaitForDockerLogOutput(
                    containerName,
                    "nodeID:              1",
                    DateTime.UtcNow.AddMinutes(2)
                );

                await TestUtil.WaitForTcpSocket(
                    "localhost",
                    containerPort,
                    DateTime.UtcNow.AddMinutes(1)
                );

                var dbContextConnString =
                    $"Host=localhost;" +
                    $"Port={containerPort};" +
                    $"Database=orleans_ef_test;" +
                    $"Username=root;" +
                    $"SSL Mode=Disable;";

                void Callback(IServiceCollection services)
                {
                    services.AddEntityFrameworkNpgsql();
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseNpgsql(dbContextConnString)
                    );
                }

                await TestSuite.Run(2000, Callback);
            }
        }
    }
}