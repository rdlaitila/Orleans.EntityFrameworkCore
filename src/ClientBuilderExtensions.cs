using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Messaging;

namespace Orleans.EntityFrameworkCore
{
    public static class ClientBuilderExtensions
    {
        public static IClientBuilder UseEntityFrameworkClustering(this IClientBuilder builder)
        {
            builder.ConfigureServices(services => {
                services.AddSingleton<IGatewayListProvider, OrleansEFGatewayListProvider>();
            });

            return builder;
        }
    }
}