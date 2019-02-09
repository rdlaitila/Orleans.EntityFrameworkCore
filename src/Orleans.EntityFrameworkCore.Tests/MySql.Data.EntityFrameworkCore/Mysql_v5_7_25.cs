using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.EntityFrameworkCore.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.MySql.Data.EntityFrameworkCore
{
    [TestClass]
    public class Mysql_v5_7_25
    {
        [TestMethod]
        public async Task Test()
        {
            var containerName = GetType().FullName;

            var containerPort = 51006;

            var container = new TestUtil.DockerContainer(
                containerName,
                "mysql:5.7.25",
                new[]
                {
                    $"-p {containerPort}:3306",
                    "-e MYSQL_ROOT_PASSWORD=my-secret-pw"
                }
            );

            using (container)
            {
                container.Start();

                await TestUtil.WaitForDockerLogOutput(
                    containerName,
                    "Version: '5.7.25'  socket: '/var/run/mysqld/mysqld.sock'  port: 3306  MySQL Community Server (GPL)",
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
                    $"Uid=root;" +
                    $"Pwd=my-secret-pw";

                void Callback(IServiceCollection services)
                {
                    services.AddEntityFrameworkMySQL();
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseMySQL(dbContextConnString)
                    );
                }

                await TestSuite.Run(51000, Callback);
            }
        }
    }
}