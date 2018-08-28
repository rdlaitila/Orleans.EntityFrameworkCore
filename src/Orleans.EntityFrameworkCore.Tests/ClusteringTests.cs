using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.Configuration;
using Orleans.EntityFrameworkCore.Extensions;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests
{
    [TestClass]
    public class ClusteringTests
    {
        private static OrleansEFContext _context;
        private static InMemoryDatabaseRoot _contextRoot;
        private static DbContextOptionsBuilder<OrleansEFContext> _contextOptionsBuilder;
        private static ISiloHost _defaultHost;

        [ClassInitialize]
        public static void _ClassInitialize(TestContext ctx)
        {
            _contextRoot = new InMemoryDatabaseRoot();
            _contextOptionsBuilder = new DbContextOptionsBuilder<OrleansEFContext>();
            _contextOptionsBuilder.UseInMemoryDatabase("ef_context", _contextRoot);
            _context = new OrleansEFContext(_contextOptionsBuilder.Options);

            _defaultHost = BuildHost(_contextRoot);
            _defaultHost
                .StartAsync()
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
        public async Task SiloHost_correctly_adds_membership_entry()
        {
            var member = await _context.Memberships.FirstOrDefaultAsync();

            Assert.IsNotNull(member);
            Assert.AreEqual("127.0.0.1", member.Address);
            Assert.AreEqual("TestCluster1", member.DeploymentId);
            Assert.AreEqual(0, member.FaultZone);
            Assert.AreEqual(System.Environment.MachineName, member.HostName);

            Assert.AreEqual(DateTime.UtcNow.Year, member.IAmAliveTime.Year);
            Assert.AreEqual(DateTime.UtcNow.Month, member.IAmAliveTime.Month);
            Assert.AreEqual(DateTime.UtcNow.Day, member.IAmAliveTime.Day);
            Assert.AreEqual(DateTime.UtcNow.Hour, member.IAmAliveTime.Hour);

            Assert.AreEqual(31313, member.Port);
            Assert.AreEqual(11313, member.ProxyPort);
            Assert.IsTrue(member.SiloName.StartsWith("Silo_"));

            Assert.AreEqual(DateTime.UtcNow.Year, member.StartTime.Year);
            Assert.AreEqual(DateTime.UtcNow.Month, member.StartTime.Month);
            Assert.AreEqual(DateTime.UtcNow.Day, member.StartTime.Day);
            Assert.AreEqual(DateTime.UtcNow.Hour, member.StartTime.Hour);

            Assert.AreEqual(3, member.Status);
            Assert.AreEqual("", member.SuspectTimes);
            Assert.AreEqual(0, member.UpdateZone);

            Assert.AreEqual(DateTime.UtcNow.Year, member.CreatedAt.Year);
            Assert.AreEqual(DateTime.UtcNow.Month, member.CreatedAt.Month);
            Assert.AreEqual(DateTime.UtcNow.Day, member.CreatedAt.Day);
            Assert.AreEqual(DateTime.UtcNow.Hour, member.CreatedAt.Hour);

            Assert.AreEqual(DateTime.UtcNow.Year, member.UpdatedAt.Year);
            Assert.AreEqual(DateTime.UtcNow.Month, member.UpdatedAt.Month);
            Assert.AreEqual(DateTime.UtcNow.Day, member.UpdatedAt.Day);
            Assert.AreEqual(DateTime.UtcNow.Hour, member.UpdatedAt.Hour);
        }

        private static ISiloHost BuildHost(
            InMemoryDatabaseRoot dbRoot,
            string clusterId = "TestCluster1",
            string serviceId = "TestCluster",
            int siloPort = 31313,
            int gatewayPort = 11313
        )
        {
            return new SiloHostBuilder()
                .ConfigureEndpoints(IPAddress.Loopback, siloPort, gatewayPort)
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterId;
                    options.ServiceId = serviceId;
                })
                .ConfigureServices(services =>
                {
                    services.AddDbContext<OrleansEFContext>(options =>
                        options.UseInMemoryDatabase("ef_context", dbRoot)
                    );
                })
                .UseEntityFrameworkClustering()
                .Build();
        }
    }
}