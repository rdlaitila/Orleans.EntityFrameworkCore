using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.Microsoft.EntityFrameworkCore.InMemory
{
    [TestClass]
    public class InMemory
    {
        [TestMethod]
        public async Task Test()
        {
            try
            {
                var dbRoot = new InMemoryDatabaseRoot();

                void Callback(IServiceCollection services) =>
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseInMemoryDatabase(
                            databaseName: "orleans_ef_test",
                            databaseRoot: dbRoot
                        )
                    );

                await TestSuite.Run(1000, Callback);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}