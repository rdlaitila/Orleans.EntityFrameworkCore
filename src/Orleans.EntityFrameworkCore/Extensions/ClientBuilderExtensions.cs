using Microsoft.Extensions.DependencyInjection;
using Orleans.Messaging;

namespace Orleans.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// </summary>
    public static class ClientBuilderExtensions
    {
        /// <summary>
        /// Uses the entity framework clustering.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IClientBuilder UseEntityFrameworkClustering(this IClientBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IGatewayListProvider, OrleansEFGatewayListProvider>();
            });

            return builder;
        }
    }
}