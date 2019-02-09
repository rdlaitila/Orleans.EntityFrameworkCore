using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Orleans.EntityFrameworkCore.OrleansEFEntity" />
    [Table("orleans_ef_storage")]
    public class OrleansEFStorage : OrleansEFEntity
    {
        /// <summary>
        /// Row Id
        /// </summary>
        [Column("id"), Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        /// <value>
        /// The primary key.
        /// </value>
        [Column("primary_key"), Required]
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        [Column("type"), Required]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the binary data.
        /// </summary>
        /// <value>
        /// The binary data.
        /// </value>
        [Column("data")]
        [MaxLength]
        public string Data { get; set; }

        /// <summary>
        /// Gets or sets the e tag.
        /// </summary>
        /// <value>
        /// The e tag.
        /// </value>
        [Column("etag")]
        [MaxLength(100)]
        public string ETag { get; set; }
    }
}