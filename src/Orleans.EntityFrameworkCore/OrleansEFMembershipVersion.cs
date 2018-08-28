using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    [Table("orleans_ef_membership_version")]
    public class OrleansEFMembershipVersion
    {
        /// <summary>
        /// Gets or sets the deployment identifier.
        /// </summary>
        /// <value>
        /// The deployment identifier.
        /// </value>
        [Column("deployment_id")]
        public string DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        [Column("version")]
        public uint Version { get; set; }
    }
}