using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orleans.EntityFrameworkCore.Migrations;
using System;
using System.Threading.Tasks;
using Orleans.EntityFrameworkCore.Tests.App;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]

namespace Orleans.EntityFrameworkCore.Tests
{
    public static class TestSuite
    {
        public static async Task Run(
            int siloPortStart,
            Action<IServiceCollection> configureServicesCallback
        )
        {
            var ctx = new TestSuiteContext
            {
                SiloPortStart = siloPortStart,
                ConfigureServicesCallback = configureServicesCallback,
            };

            await StartCluster(ctx);

            var storageGrain = ctx
                .SiloClient
                .GetGrain<IStorageGrain>(Guid.Empty);

            var storageState = await storageGrain
                .GetState();

            Assert.AreEqual(0, storageState.Counter);

            for (var a = 0; a < 100; a++)
            {
                await storageGrain
                    .IncrementCounter();
            }

            storageState = await storageGrain
                .GetState();

            Assert.AreEqual(100, storageState.Counter);

            var reminderGrain = ctx
                .SiloClient
                .GetGrain<IReminderGrain>(Guid.Empty);

            var reminderState = await reminderGrain
                .GetState();

            Assert.AreEqual(0, reminderState.NumReminderCalled);

            await reminderGrain
                .SetReminder();

            await Task.Delay(TimeSpan.FromSeconds(10));

            reminderState = await reminderGrain
                .GetState();

            Assert.AreEqual(1, reminderState.NumReminderCalled);

            await StopCluster(ctx);

            await StartCluster(ctx);

            await Task.Delay(TimeSpan.FromSeconds(61));

            reminderGrain = ctx
                .SiloClient
                .GetGrain<IReminderGrain>(Guid.Empty);

            reminderState = await reminderGrain
                .GetState();

            Assert.AreEqual(2, reminderState.NumReminderCalled);

            await StopCluster(ctx);
        }

        private static async Task StartCluster(TestSuiteContext context)
        {
            context.SiloHost1 = TestUtil.BuildSiloHost(
                context.SiloPortStart,
                context.SiloPortStart + 1,
                context.ConfigureServicesCallback
            );

            context.SiloHost2 = TestUtil.BuildSiloHost(
                context.SiloPortStart + 2,
                context.SiloPortStart + 3,
                context.ConfigureServicesCallback
            );

            context.SiloHost3 = TestUtil.BuildSiloHost(
                context.SiloPortStart + 4,
                context.SiloPortStart + 5,
                context.ConfigureServicesCallback
            );

            using (var scope = context.SiloHost1.Services.CreateScope())
            {
                var orleansEfContext = scope
                    .ServiceProvider
                    .GetService<OrleansEFContext>();

                var runMigrations =
                    orleansEfContext.Database.ProviderName != ProviderNames.InMemory;

                if (runMigrations)
                {
                    await orleansEfContext
                        .Database
                        .MigrateAsync();
                }
            }

            await Task.WhenAll(
                context.SiloHost1.StartAsync(),
                context.SiloHost2.StartAsync(),
                context.SiloHost3.StartAsync()
            );

            context.SiloClient = TestUtil
                .BuildClient(context.ConfigureServicesCallback);

            await context.SiloClient
                .Connect(exception => Task.FromResult(true));
        }

        public static async Task StopCluster(TestSuiteContext context)
        {
            context.SiloClient.Dispose();

            await context.SiloHost1.StopAsync();
            await context.SiloHost2.StopAsync();
            await context.SiloHost3.StopAsync();

            context.SiloHost1.Dispose();
            context.SiloHost2.Dispose();
            context.SiloHost3.Dispose();
        }
    }
}