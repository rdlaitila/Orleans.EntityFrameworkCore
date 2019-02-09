using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.Npgsql.EntityFrameworkCore.PostgreSQL
{
    [TestClass]
    public class Postgres_v11_1
    {
        [TestMethod]
        public async Task Test()
        {
            var containerName = GetType().FullName;

            var containerPort = 44006;

            var container = new TestUtil.DockerContainer(
                containerName,
                "postgres:11.1",
                new[]
                {
                    $"-p {containerPort}:5432",
                    "-e POSTGRES_PASSWORD=mysecretpassword"
                }
            );

            using (container)
            {
                container.Start();

                await TestUtil.WaitForDockerLogOutput(
                    containerName,
                    "LOG:  database system is ready to accept connections",
                    DateTime.UtcNow.AddMinutes(2)
                );

                await TestUtil.WaitForTcpSocket(
                    "localhost",
                    containerPort,
                    DateTime.UtcNow.AddMinutes(1)
                );

                var dbContextConnString =
                    $"Server=localhost;" +
                    $"Port={containerPort};" +
                    $"Database=orleans_ef_test;" +
                    $"User Id=postgres;" +
                    $"Password=mysecretpassword";

                void Callback(IServiceCollection services)
                {
                    services.AddEntityFrameworkNpgsql();
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseNpgsql(dbContextConnString)
                    );
                }

                await TestSuite.Run(44000, Callback);
            }
        }
    }
}