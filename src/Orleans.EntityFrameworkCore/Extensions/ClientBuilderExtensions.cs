using Microsoft.Extensions.DependencyInjection;
using Orleans.Messaging;

namespace Orleans.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// </summary>
    public static class ClientBuilderExtensions
    {
        private static bool partsConfigured;

        /// <summary>
        /// Uses the entity framework clustering.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IClientBuilder UseEntityFrameworkClustering(this IClientBuilder builder)
        {
            ConfigureParts(builder);

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IGatewayListProvider, OrleansEFGatewayListProvider>();
            });

            return builder;
        }

        /// <summary>
        /// Configures the parts.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void ConfigureParts(IClientBuilder builder)
        {
            if (partsConfigured)
                return;

            builder.ConfigureApplicationParts(parts =>
                parts.AddApplicationPart(typeof(IOrleansEFStorageGrain).Assembly).WithReferences()
            );

            partsConfigured = true;
        }
    }
}