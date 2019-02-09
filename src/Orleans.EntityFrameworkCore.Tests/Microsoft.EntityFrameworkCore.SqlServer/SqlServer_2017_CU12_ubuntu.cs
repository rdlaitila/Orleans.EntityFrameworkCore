using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.Microsoft.EntityFrameworkCore.SqlServer
{
    [TestClass]
    public class SqlServer_2017_CU12_ubuntu
    {
        [TestMethod]
        public async Task Test()
        {
            var containerName = GetType().FullName;

            var containerPort = 3006;

            var databasePassword = "!@SuperStrongPassword1234";

            var container = new TestUtil.DockerContainer(
                containerName,
                "mcr.microsoft.com/mssql/server:2017-CU12-ubuntu",
                new[]
                {
                    $"-p {containerPort}:1433",
                    $"-e ACCEPT_EULA=Y",
                    $"-e SA_PASSWORD={databasePassword}"
                }
            );

            using (container)
            {
                container.Start();

                await TestUtil.WaitForDockerLogOutput(
                    containerName,
                    "Server is listening on [ 'any' <ipv4> 1433].",
                    DateTime.UtcNow.AddMinutes(2)
                );

                await TestUtil.WaitForTcpSocket(
                    "localhost",
                    containerPort,
                    DateTime.UtcNow.AddMinutes(1)
                );

                await Task.Delay(TimeSpan.FromSeconds(5));

                var dbContextConnString =
                    $"Server=localhost,{containerPort};" +
                    $"Database=orleans_ef_test;" +
                    $"User Id=sa;" +
                    $"Password={databasePassword};";

                void Callback(IServiceCollection services) =>
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseSqlServer(dbContextConnString)
                    );

                await TestSuite.Run(3000, Callback);
            }
        }
    }
}