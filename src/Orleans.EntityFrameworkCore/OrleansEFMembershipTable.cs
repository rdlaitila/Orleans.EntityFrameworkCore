using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.EntityFrameworkCore.Extensions;
using Orleans.Runtime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// Provides an IMembershipTable implementation wrapping orleans operations
    /// translating them into entity framework calls
    /// </summary>
    public class OrleansEFMembershipTable : IMembershipTable
    {
        /// <summary>
        /// The OrleansEFContext
        /// </summary>
        private readonly OrleansEFContext _db;

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
        /// Orleans appears to attempt access to IMembershpTable in seperate threads
        /// which breaks access requirements of EF contexts thus we must
        /// lock when access to the context is attempted
        /// </summary>
        /// <returns></returns>
        private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Constructor fed from DI
        /// </summary>
        /// <param name="db"></param>
        /// <param name="clusterOptions"></param>
        /// <param name="logger"></param>
        public OrleansEFMembershipTable(
            OrleansEFContext db,
            IOptions<ClusterOptions> clusterOptions,
            ILogger<OrleansEFMembershipTable> logger
        )
        {
            _db = db ??
                throw new ArgumentNullException(nameof(db));

            _clusterOptions = clusterOptions?.Value ??
                throw new ArgumentNullException(nameof(clusterOptions));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
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

                using (await _lock.DisposableWaitAsync())
                {
                    var newRow = OrleansEFMapper.Map(entry);

                    newRow.DeploymentId = _clusterOptions.ClusterId;
                    newRow.Generation = entry.SiloAddress.Generation;
                    newRow.Address = entry.SiloAddress.Endpoint.Address.ToString();
                    newRow.Port = entry.SiloAddress.Endpoint.Port;

                    _db.Memberships.Add(newRow);

                    await _db.SaveChangesAsync();

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

        /// <summary>
        /// ReadAll
        /// </summary>
        /// <returns></returns>
        public async Task<MembershipTableData> ReadAll()
        {
            try
            {
                var rows = await _db
                    .Memberships
                    .AsNoTracking()
                    .Where(a =>
                        a.DeploymentId == _clusterOptions.ClusterId
                    )
                    .ToListAsync();

                return OrleansEFMapper.Map(rows);
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

                var rows = await _db
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
        public async Task UpdateIAmAlive(MembershipEntry entry)
        {
            try
            {
                using (await _lock.DisposableWaitAsync())
                {
                    if (entry == null)
                        throw new ArgumentNullException(nameof(entry));

                    var row = await _db.Memberships.FirstOrDefaultAsync(a =>
                        a.DeploymentId == _clusterOptions.ClusterId &&
                        a.Address == entry.SiloAddress.Endpoint.Address.ToString() &&
                        a.Port == (uint)entry.SiloAddress.Endpoint.Port &&
                        a.Generation == entry.SiloAddress.Generation
                    );

                    if (row == null)
                        throw new UpdateIAmAliveException.RowNotFound(entry.SiloAddress);

                    row.IAmAliveTime = entry.IAmAliveTime;

                    await _db.SaveChangesAsync();

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
        /// custom exceptions for UpdateIAmAlive
        /// </summary>
        public abstract class UpdateIAmAliveException : Exception
        {
            public UpdateIAmAliveException(string message) : base(message)
            {
            }

            public class RowNotFound : UpdateIAmAliveException
            {
                public RowNotFound(SiloAddress key) : base($"no rows with silo address {key.Endpoint.ToString()} found")
                {
                }
            }
        }

        /// <summary>
        /// UpdateRow
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="etag"></param>
        /// <param name="tableVersion"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRow(MembershipEntry entry, string etag, TableVersion tableVersion)
        {
            try
            {
                using (await _lock.DisposableWaitAsync())
                {
                    if (entry == null)
                        throw new ArgumentNullException(nameof(entry));

                    if (string.IsNullOrWhiteSpace(etag))
                        throw new ArgumentNullException(nameof(etag));

                    if (tableVersion == null)
                        throw new ArgumentNullException(nameof(tableVersion));

                    var row = await _db.Memberships.FirstOrDefaultAsync(a =>
                        a.DeploymentId == _clusterOptions.ClusterId &&
                        a.Address == entry.SiloAddress.Endpoint.Address.ToString() &&
                        a.Port == (uint)entry.SiloAddress.Endpoint.Port &&
                        a.Generation == entry.SiloAddress.Generation
                    );

                    if (row == null)
                        throw new UpdateRowException.RowNotFound(
                            entry.SiloAddress
                        );

                    OrleansEFMapper.Map(entry, row);

                    await _db.SaveChangesAsync();

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

        /// <summary>
        /// custom exceptions for UpdateRow
        /// </summary>
        public abstract class UpdateRowException : Exception
        {
            public UpdateRowException(string message) : base(message)
            {
            }

            public class RowNotFound : UpdateRowException
            {
                public RowNotFound(SiloAddress key) : base($"no rows with silo address {key.Endpoint.ToString()} found")
                {
                }
            }
        }
    }
}