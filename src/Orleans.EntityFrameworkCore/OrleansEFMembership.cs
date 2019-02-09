using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Orleans.EntityFrameworkCore.OrleansEFEntity" />
    [Table("orleans_ef_membership")]
    public class OrleansEFMembership : OrleansEFEntity
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the deployment identifier.
        /// </summary>
        /// <value>
        /// The deployment identifier.
        /// </value>
        [Column("deployment_id")]
        public string DeploymentId { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [Column("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        [Column("port")]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the generation.
        /// </summary>
        /// <value>
        /// The generation.
        /// </value>
        [Column("generation")]
        public int Generation { get; set; }

        /// <summary>
        /// Gets or sets the name of the silo.
        /// </summary>
        /// <value>
        /// The name of the silo.
        /// </value>
        [Column("silo_name")]
        public string SiloName { get; set; }

        /// <summary>
        /// Gets or sets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        [Column("host_name")]
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [Column("status")]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the proxy port.
        /// </summary>
        /// <value>
        /// The proxy port.
        /// </value>
        [Column("proxy_port")]
        public int ProxyPort { get; set; }

        /// <summary>
        /// Gets or sets the suspect times.
        /// </summary>
        /// <value>
        /// The suspect times.
        /// </value>
        [Column("suspect_times")]
        public string SuspectTimes { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        [Column("start_time")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the i am alive time.
        /// </summary>
        /// <value>
        /// The i am alive time.
        /// </value>
        [Column("i_am_alive_time")]
        public DateTime IAmAliveTime { get; set; }

        /// <summary>
        /// Gets or sets the fault zone.
        /// </summary>
        /// <value>
        /// The fault zone.
        /// </value>
        [Column("fault_zone")]
        public int FaultZone { get; set; }

        /// <summary>
        /// Gets or sets the update zone.
        /// </summary>
        /// <value>
        /// The update zone.
        /// </value>
        [Column("update_zone")]
        public int UpdateZone { get; set; }
    }
}