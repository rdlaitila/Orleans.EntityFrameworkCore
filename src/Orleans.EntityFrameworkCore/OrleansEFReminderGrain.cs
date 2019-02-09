using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Orleans.Concurrency;
using Orleans.Configuration;
using Orleans.Runtime;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Orleans.IGrainWithStringKey" />
    internal interface IOrleansEFReminderGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Reads the row.
        /// </summary>
        /// <param name="grainRef">The grain reference.</param>
        /// <param name="reminderName">Name of the reminder.</param>
        /// <returns></returns>
        Task<ReminderEntry> ReadRow(
            GrainReference grainRef,
            string reminderName
        );

        /// <summary>
        /// Removes the row.
        /// </summary>
        /// <param name="grainRef">The grain reference.</param>
        /// <param name="reminderName">Name of the reminder.</param>
        /// <param name="eTag">The e tag.</param>
        /// <returns></returns>
        Task<bool> RemoveRow(
            GrainReference grainRef,
            string reminderName,
            string eTag
        );

        /// <summary>
        /// Reads the rows.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        Task<ReminderTableData> ReadRows(
            GrainReference key
        );

        /// <summary>
        /// Upserts the row.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        Task<string> UpsertRow(
            ReminderEntry entry
        );
    }

    internal class OrleansEFReminderGrain : Grain, IOrleansEFReminderGrain
    {
        private readonly OrleansEFContext _db;

        private readonly IOptions<ClusterOptions> _clusterOptions;

        private readonly IGrainReferenceConverter _grainReferenceConverter;

        public OrleansEFReminderGrain(
            IOptions<ClusterOptions> clusterOptions,
            IGrainReferenceConverter grainReferenceConverter,
            OrleansEFContext db
        )
        {
            _clusterOptions = clusterOptions ??
                throw new ArgumentNullException(nameof(clusterOptions));

            _grainReferenceConverter = grainReferenceConverter ??
                throw new ArgumentNullException(nameof(grainReferenceConverter));

            _db = db ??
                throw new ArgumentNullException(nameof(db));
        }

        public async Task<ReminderEntry> ReadRow(
            GrainReference grainRef,
            string reminderName
        )
        {
            var row = await _db
                .Reminders
                .FirstOrDefaultAsync(a =>
                    a.ServiceId == _clusterOptions.Value.ServiceId &&
                    a.GrainId == grainRef.ToKeyString() &&
                    a.ReminderName == reminderName
                );

            if (row == null)
                return null;

            return OrleansEFMapper.Map(
                _grainReferenceConverter,
                row
            );
        }

        public async Task<ReminderTableData> ReadRows(
            GrainReference key
        )
        {
            var rows = await _db
                .Reminders
                .Where(a =>
                    a.ServiceId == _clusterOptions.Value.ServiceId &&
                    a.GrainId == key.ToKeyString()
                )
                .ToListAsync();

            return OrleansEFMapper.Map(
                _grainReferenceConverter,
                rows
            );
        }

        public async Task<bool> RemoveRow(
            GrainReference grainRef,
            string reminderName,
            string eTag
        )
        {
            var row = await _db
                .Reminders
                .FirstOrDefaultAsync(a =>
                    a.ServiceId == _clusterOptions.Value.ServiceId &&
                    a.GrainId == grainRef.ToKeyString() &&
                    a.ReminderName == reminderName
                );

            if (row == null)
                return false;

            if (row.ETag != eTag)
                throw new OrleansEFReminderException.EtagMismatch(
                    $"etag mismatch. " +
                    $"grainId: {grainRef.ToKeyString()} " +
                    $"reminderName: {reminderName}"
                );

            _db.Reminders.Remove(row);

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<string> UpsertRow(
            ReminderEntry entry
        )
        {
            var row = await _db
                .Reminders
                .FirstOrDefaultAsync(a =>
                    a.ServiceId == _clusterOptions.Value.ServiceId &&
                    a.GrainId == entry.GrainRef.ToKeyString() &&
                    a.ReminderName == entry.ReminderName
                );

            var nullRow =
                row == null;

            if (nullRow)
            {
                row = new OrleansEFReminder
                {
                    Id = Guid.NewGuid(),
                    ETag = entry.ETag,
                };

                _db.Reminders.Add(row);
            }

            if (entry.ETag != row.ETag)
                throw new OrleansEFReminderException.EtagMismatch(
                    $"etag mismatch. " +
                    $"grainId: {entry.GrainRef.ToKeyString()} " +
                    $"reminderName: {entry.ReminderName}"
                );

            var serviceId = _clusterOptions
                .Value
                .ServiceId;

            row = OrleansEFMapper
                .Map(entry, row, serviceId: serviceId);

            row.ETag = Guid
                .NewGuid()
                .ToString();

            await _db.SaveChangesAsync();

            return row.ETag;
        }
    }
}