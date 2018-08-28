using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    public abstract class OrleansEFEntity
    {
        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at.
        /// </summary>
        /// <value>
        /// The updated at.
        /// </value>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}