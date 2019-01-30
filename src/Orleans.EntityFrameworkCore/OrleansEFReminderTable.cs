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
    /// Provides an IReminderTable implementation wrapping orleans operations
    /// translating them into entity framework calls
    /// </summary>
    public class OrleansEFReminderTable : IReminderTable
    {
        /// <summary>
        /// The database
        /// </summary>
        private readonly OrleansEFContext _db;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<OrleansEFReminderTable> _logger;

        /// <summary>
        /// The cluster options
        /// </summary>
        private readonly ClusterOptions _clusterOptions;

        /// <summary>
        /// The grain factory
        /// </summary>
        private readonly IGrainFactory _grainFactory;

        /// <summary>
        /// The grain reference converter
        /// </summary>
        private readonly IGrainReferenceConverter _grainReferenceConverter;

        /// <summary>
        /// Orleans appears to attempt access to IReminderTable in seperate threads
        /// which breaks access requirements of EF contexts thus we must
        /// lock when access to the context is attempted
        /// </summary>
        /// <returns></returns>
        private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="OrleansEFReminderTable"/> class.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="clusterOptions">The cluster options.</param>
        /// <param name="grainFactory">The grain factory.</param>
        /// <exception cref="ArgumentNullException">
        /// db
        /// or
        /// clusterOptions
        /// or
        /// grainFactory
        /// </exception>
        public OrleansEFReminderTable(
            IOptions<ClusterOptions> clusterOptions,
            IGrainFactory grainFactory,
            ILogger<OrleansEFReminderTable> logger,
            IGrainReferenceConverter grainReferenceConverter,
            OrleansEFContext db
        )
        {
            _clusterOptions = clusterOptions?.Value ??
                throw new ArgumentNullException(nameof(clusterOptions));

            _grainFactory = grainFactory ??
                throw new ArgumentNullException(nameof(grainFactory));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            _grainReferenceConverter = grainReferenceConverter ??
                throw new ArgumentNullException(nameof(grainReferenceConverter));

            _db = db ??
                throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Init()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads the row.
        /// </summary>
        /// <param name="grainRef">The grain reference.</param>
        /// <param name="reminderName">Name of the reminder.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<ReminderEntry> ReadRow(
            GrainReference grainRef,
            string reminderName
        )
        {
            try
            {
                return await _grainFactory
                    .GetGrain<IOrleansEFReminderGrain>(grainRef.ToKeyString())
                    .ReadRow(grainRef, reminderName);
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(ReadRow), e);
                throw;
            }
        }

        /// <summary>
        /// Reads the rows.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<ReminderTableData> ReadRows(
            GrainReference key
        )
        {
            try
            {
                return await _grainFactory
                    .GetGrain<IOrleansEFReminderGrain>(key.ToKeyString())
                    .ReadRows(key);
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(ReadRows), e);
                throw;
            }
        }

        /// <summary>
        /// Return all rows that have their GrainReference's.GetUniformHashCode() in the range (start, end]
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<ReminderTableData> ReadRows(
            uint begin,
            uint end
        )
        {
            try
            {
                var iBegin = (int)begin;
                var iEnd = (int)end;

                using (await _lock.DisposableWaitAsync())
                {
                    var rows = await _db
                        .Reminders
                        .Where(a =>
                            a.ServiceId == _clusterOptions.ServiceId &&
                            a.GrainHash >= iBegin &&
                            a.GrainHash <= iEnd
                        )
                        .ToListAsync();

                    return OrleansEFMapper.Map(
                        _grainReferenceConverter,
                        rows
                    );
                }
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(ReadRows), e);
                throw;
            }
        }

        /// <summary>
        /// Remove a row from the table.
        /// </summary>
        /// <param name="grainRef"></param>
        /// <param name="reminderName"></param>
        /// <param name="eTag"></param>
        /// <returns>
        /// true if a row with <paramref name="grainRef" /> and <paramref name="reminderName" /> existed and was removed successfully, false otherwise
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> RemoveRow(
            GrainReference grainRef,
            string reminderName,
            string eTag
        )
        {
            try
            {
                return await _grainFactory
                   .GetGrain<IOrleansEFReminderGrain>(grainRef.ToKeyString())
                   .RemoveRow(grainRef, reminderName, eTag);
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(RemoveRow), e);
                throw;
            }
        }

        /// <summary>
        /// Tests the only clear table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task TestOnlyClearTable()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Upserts the row.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<string> UpsertRow(
            ReminderEntry entry
        )
        {
            try
            {
                return await _grainFactory
                    .GetGrain<IOrleansEFReminderGrain>(entry.GrainRef.ToKeyString())
                    .UpsertRow(entry);
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(UpsertRow), e);
                throw;
            }
        }
    }
}