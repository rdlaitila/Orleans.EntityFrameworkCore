using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.Microsoft.EntityFrameworkCore.Sqlite
{
    [TestClass]
    public class Sqlite
    {
        [TestMethod]
        public async Task Test()
        {
            var dbFileName = "orleans_ef_test.db";

            try
            {
                if (File.Exists(dbFileName))
                    File.Delete(dbFileName);

                var connectionString = "Data Source=orleans_ef_test.db";

                void callback(IServiceCollection services) =>
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseSqlite(connectionString)
                    );

                await TestSuite
                    .Run(1100, callback);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (File.Exists(dbFileName))
                    File.Delete(dbFileName);
            }
        }
    }
}