using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orleans;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// </summary>
    public static class SiloHostBuilderExtensions
    {
        private static bool partsConfigured;

        /// <summary>
        /// Adds the entity framework storage.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static ISiloHostBuilder AddEntityFrameworkStorage(this ISiloHostBuilder builder, string name = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)
        {
            ConfigureParts(builder);

            builder.ConfigureServices(services =>
            {
                services.TryAddSingleton<IGrainStorage>(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));
                services.AddSingletonNamedService<IGrainStorage, OrleansEFStorageProvider>(name);
            });

            return builder;
        }

        /// <summary>
        /// Uses the entity framework clustering.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static ISiloHostBuilder UseEntityFrameworkClustering(this ISiloHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IMembershipTable, OrleansEFMembershipTable>();
            });

            return builder;
        }

        /// <summary>
        /// Uses the entity framework reminders.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static ISiloHostBuilder UseEntityFrameworkReminders(this ISiloHostBuilder builder)
        {
            ConfigureParts(builder);

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IReminderTable, OrleansEFReminderTable>();
            });

            return builder;
        }

        /// <summary>
        /// Configures the parts.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void ConfigureParts(ISiloHostBuilder builder)
        {
            if (partsConfigured)
                return;

            builder.ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(OrleansEFContext).Assembly)
            );

            partsConfigured = true;
        }
    }
}