using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Runtime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans.EntityFrameworkCore.Extensions;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// Provides an IMembershipTable implementation wrapping orleans operations
    /// translating them into entity framework calls
    /// </summary>
    public class OrleansEFMembershipTable : IMembershipTable
    {
        /// <summary>
        /// Cluster options as configured during ISiloHostBuilder setup
        /// </summary>
        private readonly ClusterOptions _clusterOptions;

        /// <summary>
        /// we need a logger as Orleans appears to swallow exceptions thrown in
        /// the IMembershipTable impl(s). Useful for debugging issues
        /// </summary>
        private readonly ILogger<OrleansEFMembershipTable> _logger;

        /// <summary>
        /// Needed to get instances of OrleansEFContext
        /// </summary>
        private readonly IServiceProvider _services;

        private readonly OrleansEFContext _orleansEfContext;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Constructor fed from DI
        /// </summary>
        /// <param name="db"></param>
        /// <param name="clusterOptions"></param>
        /// <param name="logger"></param>
        public OrleansEFMembershipTable(
            IOptions<ClusterOptions> clusterOptions,
            ILogger<OrleansEFMembershipTable> logger,
            IServiceProvider services,
            OrleansEFContext orleansEfContext
        )
        {
            _clusterOptions = clusterOptions?.Value ??
                throw new ArgumentNullException(nameof(clusterOptions));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            _services = services ??
                throw new ArgumentNullException(nameof(services));

            _orleansEfContext = orleansEfContext ??
                throw new ArgumentNullException(nameof(orleansEfContext));
        }

        /// <summary>
        /// DeleteMembershipTableEntries
        /// </summary>
        /// <param name="clusterId"></param>
        /// <returns></returns>
        public Task DeleteMembershipTableEntries(string clusterId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clusterId))
                    throw new ArgumentNullException(nameof(clusterId));

                throw new System.NotImplementedException();
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(DeleteMembershipTableEntries), e);
                throw;
            }
        }

        /// <summary>
        /// delete old silo entries
        /// </summary>
        /// <param name="beforeDate"></param>
        /// <returns></returns>
        public async Task CleanupDefunctSiloEntries(DateTimeOffset beforeDate)
        {
            using (await _semaphore.DisposableWaitAsync())
            {
                var deadSilos = await _orleansEfContext
                    .Memberships
                    .Where(a =>
                        a.DeploymentId == _clusterOptions.ClusterId &&
                        a.IAmAliveTime < beforeDate.UtcDateTime
                    )
                    .ToListAsync();

                deadSilos.ForEach(a =>
                    _logger.Info(0, $"cleaning up suspected dead silo. address={a.Address}:{a.Port} hostname={a.HostName} last_i_am_alive={a.IAmAliveTime}")
                );

                _orleansEfContext.Memberships.RemoveRange(deadSilos);

                await _orleansEfContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// InitializeMembershipTable
        /// </summary>
        /// <param name="tryInitTableVersion"></param>
        /// <returns></returns>
        public Task InitializeMembershipTable(bool tryInitTableVersion)
        {
            try
            {
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(InitializeMembershipTable), e);
                throw;
            }
        }

        /// <summary>
        /// InsertRow
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="tableVersion"></param>
        /// <returns></returns>
        public async Task<bool> InsertRow(MembershipEntry entry, TableVersion tableVersion)
        {
            try
            {
                if (entry == null)
                    throw new ArgumentNullException(nameof(entry));

                if (tableVersion == null)
                    throw new ArgumentNullException(nameof(tableVersion));

                using (await _semaphore.DisposableWaitAsync())
                {
                    var newRow = OrleansEFMapper.Map(entry);

                    newRow.Id = Guid.NewGuid();
                    newRow.DeploymentId = _clusterOptions.ClusterId;
                    newRow.Generation = entry.SiloAddress.Generation;
                    newRow.Address = entry.SiloAddress.Endpoint.Address.ToString();
                    newRow.Port = entry.SiloAddress.Endpoint.Port;

                    _orleansEfContext.Memberships.Add(newRow);

                    await _orleansEfContext.SaveChangesAsync();

                    _logger.Info(
                        0, "{0}: {1}", nameof(InsertRow),
                        $"inserted silo with address {entry.SiloAddress.Endpoint.ToString()} to membership table"
                    );

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(InsertRow), e);
                throw;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// ReadAll
        /// </summary>
        /// <returns></returns>
        public async Task<MembershipTableData> ReadAll()
        {
            try
            {
                using (await _semaphore.DisposableWaitAsync())
                {
                    var rows = await _orleansEfContext
                        .Memberships
                        .AsNoTracking()
                        .Where(a =>
                            a.DeploymentId == _clusterOptions.ClusterId
                        )
                        .ToListAsync();

                    return OrleansEFMapper.Map(rows);
                }
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(ReadAll), e);
                throw;
            }
        }

        /// <summary>
        /// ReadRow
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<MembershipTableData> ReadRow(SiloAddress key)
        {
            try
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                using (await _semaphore.DisposableWaitAsync())
                {
                    var rows = await _orleansEfContext
                        .Memberships
                        .AsNoTracking()
                        .Where(a =>
                            a.DeploymentId == _clusterOptions.ClusterId &&
                            a.Address == key.Endpoint.Address.ToString() &&
                            a.Port == (uint)key.Endpoint.Port &&
                            a.Generation == key.Generation
                        )
                        .ToListAsync();

                    if (rows.Count == 0)
                        _logger.Warn(
                            0, "{0}: {1}", nameof(ReadRow),
                            $"no rows with silo address {key.Endpoint.ToString()} found"
                        );

                    return OrleansEFMapper.Map(rows);
                }
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(ReadRow), e);
                throw;
            }
        }

        /// <summary>
        /// UpdateIAmAlive
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task UpdateIAmAlive(
            MembershipEntry entry
        )
        {
            try
            {
                if (entry == null)
                    throw new ArgumentNullException(nameof(entry));

                using (await _semaphore.DisposableWaitAsync())
                {
                    var row = await _orleansEfContext
                        .Memberships
                        .FirstOrDefaultAsync(a =>
                            a.DeploymentId == _clusterOptions.ClusterId &&
                            a.Address == entry.SiloAddress.Endpoint.Address.ToString() &&
                            a.Port == (uint)entry.SiloAddress.Endpoint.Port &&
                            a.Generation == entry.SiloAddress.Generation
                        );

                    if (row == null)
                        throw new OrleansEFMembershipException.RowNotFound(
                            entry.SiloAddress
                        );

                    row.IAmAliveTime = entry.IAmAliveTime;

                    await _orleansEfContext.SaveChangesAsync();

                    _logger.Info(
                        0, "{0}: {1}", nameof(UpdateIAmAlive),
                        $"updated silo {entry.SiloAddress.Endpoint.ToString()} IAmAlive timestamp with {entry.IAmAliveTime}"
                    );
                }
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(UpdateIAmAlive), e);
                throw;
            }
        }

        /// <summary>
        /// UpdateRow
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="etag"></param>
        /// <param name="tableVersion"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRow(
            MembershipEntry entry,
            string etag,
            TableVersion tableVersion
        )
        {
            try
            {
                if (entry == null)
                    throw new ArgumentNullException(nameof(entry));

                if (string.IsNullOrWhiteSpace(etag))
                    throw new ArgumentNullException(nameof(etag));

                if (tableVersion == null)
                    throw new ArgumentNullException(nameof(tableVersion));

                using (await _semaphore.DisposableWaitAsync())
                {
                    var row = await _orleansEfContext
                        .Memberships
                        .FirstOrDefaultAsync(a =>
                            a.DeploymentId == _clusterOptions.ClusterId &&
                            a.Address == entry.SiloAddress.Endpoint.Address.ToString() &&
                            a.Port == (uint)entry.SiloAddress.Endpoint.Port &&
                            a.Generation == entry.SiloAddress.Generation
                        );

                    if (row == null)
                        throw new OrleansEFMembershipException.RowNotFound(
                            entry.SiloAddress
                        );

                    OrleansEFMapper.Map(entry, row);

                    await _orleansEfContext.SaveChangesAsync();

                    _logger.Info(
                        0, "{0}: {1}", nameof(UpdateRow),
                        $"updated silo {entry.SiloAddress.Endpoint.ToString()}"
                    );

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(UpdateRow), e);
                throw;
            }
        }
    }
}