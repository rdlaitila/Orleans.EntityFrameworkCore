using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Runtime;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// Provides an IGatewayListProvider implementation wrapping orleans operations
    /// translating them into entity framework calls
    /// </summary>
    public class OrleansEFGatewayListProvider : IGatewayListProvider
    {
        /// <summary>
        /// MaxStaleness
        /// </summary>
        /// <returns></returns>
        public TimeSpan MaxStaleness {get; internal set;}

        /// <summary>
        /// IsUpdatable
        /// </summary>
        public bool IsUpdatable => true;

        /// <summary>
        /// the OrleansEFContext
        /// </summary>
        private readonly OrleansEFContext _db;

        /// <summary>
        /// we need a logger as Orleans appears to swallow exceptions thrown in
        /// the IGatewayListProvider impl(s). Useful for debugging issues
        /// </summary>
        private readonly ILogger<OrleansEFGatewayListProvider> _logger;

        /// <summary>
        /// Cluster options as configured during ISiloHostBuilder setup
        /// </summary>
        private readonly ClusterOptions _clusterOptions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db"></param>
        /// <param name="clusterOptions"></param>
        /// <param name="logger"></param>
        public OrleansEFGatewayListProvider(
            OrleansEFContext db,
            IOptions<ClusterOptions> clusterOptions,
            IOptions<GatewayOptions> gatewayOptions,
            ILogger<OrleansEFGatewayListProvider> logger
        )
        {
            _db = db;
            _clusterOptions = clusterOptions.Value;
            _logger = logger;

            MaxStaleness = gatewayOptions.Value.GatewayListRefreshPeriod;
        }

        /// <summary>
        /// GetGateways
        /// </summary>
        /// <returns></returns>
        public async Task<IList<Uri>> GetGateways()
        {
            try
            {
                var siloData = await _db
                    .Memberships
                    .Where(membership =>
                        membership.Status == (int)SiloStatus.Active &&
                        membership.DeploymentId == _clusterOptions.ClusterId
                    )
                    .Select(membership => new {
                        membership.Address,
                        membership.ProxyPort,
                        membership.Generation
                    })
                    .ToListAsync();

                var gatewayUris = siloData.Select(a =>
                    OrleansEFMapper.Map(
                        a.Address,
                        (int)a.ProxyPort,
                        a.Generation
                    ).ToGatewayUri()
                ).ToList();

                return gatewayUris;
            }
            catch(Exception e)
            {
                _logger.Error(0, nameof(GetGateways), e);
                throw;
            }
        }

        /// <summary>
        /// InitializeGatewayListProvider
        /// </summary>
        /// <returns></returns>
        public Task InitializeGatewayListProvider()
        {
            return Task.CompletedTask;
        }
    }
}