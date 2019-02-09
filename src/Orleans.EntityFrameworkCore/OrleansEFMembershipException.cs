using Orleans.Runtime;
using System;

namespace Orleans.EntityFrameworkCore
{
    public abstract class OrleansEFMembershipException : Exception
    {
        public OrleansEFMembershipException(string message) : base(message)
        {
        }

        public class RowNotFound : OrleansEFMembershipException
        {
            public RowNotFound(SiloAddress key) : base($"no rows with silo address {key.Endpoint.ToString()} found")

            {
            }
        }
    }
}