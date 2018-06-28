using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;

namespace Orleans.EntityFrameworkCore
{
    public static class SiloHostBuilderExtensions
    {
        public static ISiloHostBuilder UseEntityFrameworkClustering(this ISiloHostBuilder builder)
        {
            builder.ConfigureServices(services => {
                services.AddSingleton<IMembershipTable, OrleansEFMembershipTable>();
            });

            return builder;
        }

        public static ISiloHostBuilder UseEntityFrameworkReminders(this ISiloHostBuilder builder)
        {
            builder.ConfigureServices(services => {
                services.AddSingleton<IReminderTable, OrleansEFReminderTable>();
            });

            return builder;
        }
    }
}