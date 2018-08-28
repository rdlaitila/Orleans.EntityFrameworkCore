using System;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    /// <seealso cref="System.Exception" />
    public abstract class OrleansEFReminderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrleansEFStorageException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OrleansEFReminderException(string message) : base(message)
        {
        }

        /// <summary>
        /// </summary>
        /// <seealso cref="Orleans.EntityFrameworkCore.OrleansEFStorageException" />
        public class EtagMismatch : OrleansEFStorageException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EtagMismatch"/> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public EtagMismatch(string message) : base(message)
            {
            }
        }
    }
}