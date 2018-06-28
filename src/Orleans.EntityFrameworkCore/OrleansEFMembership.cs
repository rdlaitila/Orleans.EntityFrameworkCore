using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    [Table("orleans_ef_membership")]
    public class OrleansEFMembership
    {
        [Column("deployment_id")]
        public string DeploymentId {get; set;}

        [Column("address")]
        public string Address {get; set;}

        [Column("port")]
        public int Port {get; set;}

        [Column("generation")]
        public int Generation {get; set;}

        [Column("silo_name")]
        public string SiloName {get; set;}

        [Column("host_name")]
        public string HostName {get; set;}

        [Column("status")]
        public int Status {get; set;}

        [Column("proxy_port")]
        public int ProxyPort {get; set;}

        [Column("suspect_times")]
        public string SuspectTimes {get; set;}

        [Column("start_time")]
        public DateTime StartTime {get; set;}

        [Column("i_am_alive_time")]
        public DateTime IAmAliveTime {get; set;}

        [Column("fault_zone")]
        public int FaultZone {get; set;}

        [Column("update_zone")]
        public int UpdateZone {get; set;}

        //public virtual OrleansEFMembershipVersion Version {get; set;}
    }
}