using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Orleans.EntityFrameworkCore.OrleansEFEntity" />
    [Table("orleans_ef_reminder")]
    public class OrleansEFReminder : OrleansEFEntity
    {
        /// <summary>
        /// Gets or sets the service identifier.
        /// </summary>
        /// <value>
        /// The service identifier.
        /// </value>
        [Column("service_id")]
        public string ServiceId { get; set; } = "";

        /// <summary>
        /// Gets or sets the grain identifier.
        /// </summary>
        /// <value>
        /// The grain identifier.
        /// </value>
        [Column("grain_id")]
        public string GrainId { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the reminder.
        /// </summary>
        /// <value>
        /// The name of the reminder.
        /// </value>
        [Column("reminder_name")]
        public string ReminderName { get; set; } = "";

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [Column("start_time")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        /// <value>
        /// The period.
        /// </value>
        [Column("period")]
        public int Period { get; set; }

        /// <summary>
        /// Gets or sets the grain hash.
        /// </summary>
        /// <value>
        /// The grain hash.
        /// </value>
        [Column("grain_hash")]
        public int GrainHash { get; set; }

        /// <summary>
        /// Gets or sets the e tag.
        /// </summary>
        /// <value>
        /// The e tag.
        /// </value>
        [Column("etag")]
        public string ETag { get; set; }
    }
}