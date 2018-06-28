using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    [Table("orleans_ef_membership_version")]
    public class OrleansEFMembershipVersion
    {
        [Column("deployment_id")]
        public string DeploymentId {get; set;}

        [Column("timestamp")]
        public DateTime Timestamp {get; set;}

        [Column("version")]
        public uint Version {get; set;}
    }
}