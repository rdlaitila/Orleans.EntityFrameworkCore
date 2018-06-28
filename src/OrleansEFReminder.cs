using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    [Table("orleans_ef_reminders")]
    public class OrleansEFReminder
    {
        [Column("service_id")]
        public string ServiceId {get; set;}

        [Column("grain_id")]
        public string GrainId {get; set;}

        [Column("reminder_name")]
        public string ReminderName {get; set;}

        [Column("start_time")]
        public DateTime StartTime {get; set;}

        [Column("period")]
        public int Period {get; set;}

        [Column("grain_hash")]
        public int GrainHash {get; set;}

        [Column("version")]
        public int Version {get; set;}
    }
}