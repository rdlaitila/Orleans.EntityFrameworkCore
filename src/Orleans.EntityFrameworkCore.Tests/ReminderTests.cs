using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.EntityFrameworkCore.Extensions;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests
{
    [TestClass]
    public class ReminderTests
    {
        private static OrleansEFContext _context;
        private static InMemoryDatabaseRoot _contextRoot;
        private static DbContextOptionsBuilder<OrleansEFContext> _contextOptionsBuilder;
        private static ISiloHost _defaultHost;
        private static IClusterClient _defaultClient;

        [ClassInitialize]
        public static void _ClassInitialize(TestContext ctx)
        {
            _contextRoot = new InMemoryDatabaseRoot();
            _contextOptionsBuilder = new DbContextOptionsBuilder<OrleansEFContext>();
            _contextOptionsBuilder.UseInMemoryDatabase("ef_context", _contextRoot);
            _context = new OrleansEFContext(_contextOptionsBuilder.Options);

            _defaultHost = BuildHost(_contextRoot);
            _defaultClient = BuildClient();

            _defaultHost
                .StartAsync()
                .GetAwaiter()
                .GetResult();

            _defaultClient
                .Connect()
                .GetAwaiter()
                .GetResult();
        }

        [ClassCleanup]
        public static void _ClassCleanup()
        {
            _defaultHost
                .StopAsync()
                .GetAwaiter()
                .GetResult();
        }

        [TestMethod]
        public async Task SiloHost_successfully_persists_and_runs_reminder()
        {
            var grain = _defaultClient
                .GetGrain<IReminderGrain>(Guid.Empty);

            await grain.WakeUp();

            var reminder = await _context.Reminders.FirstOrDefaultAsync();

            Assert.IsNotNull(reminder);

            await Task.Delay(TimeSpan.FromSeconds(62));

            Assert.AreEqual(true, await grain.ReminderCalled());
        }

        private static ISiloHost BuildHost(
            InMemoryDatabaseRoot dbRoot
        )
        {
            return new SiloHostBuilder()
                .ConfigureEndpoints(IPAddress.Loopback, 31313, 11313)
                .ConfigureServices(services =>
                {
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseInMemoryDatabase("ef_context", dbRoot)
                    );
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(IReminderGrain).Assembly)
                )
                .UseLocalhostClustering()
                .UseEntityFrameworkReminders()
                .Build();
        }

        private static IClusterClient BuildClient()
        {
            return new ClientBuilder()
                .UseLocalhostClustering()
                .Build();
        }
    }
}