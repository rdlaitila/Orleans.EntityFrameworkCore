using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Runtime;

namespace Orleans.EntityFrameworkCore
{
    public class OrleansEFReminderTable : IReminderTable
    {
        private readonly OrleansEFContext _db;

        private readonly ClusterOptions _clusterOptions;

        public OrleansEFReminderTable(OrleansEFContext db, IOptions<ClusterOptions> clusterOptions)
        {
            _db = db;
            _clusterOptions = clusterOptions.Value;
        }

        public Task Init()
        {
            throw new System.NotImplementedException();
        }

        public Task<ReminderEntry> ReadRow(GrainReference grainRef, string reminderName)
        {
            throw new System.NotImplementedException();
        }

        public Task<ReminderTableData> ReadRows(GrainReference key)
        {
            throw new System.NotImplementedException();
        }

        public Task<ReminderTableData> ReadRows(uint begin, uint end)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemoveRow(GrainReference grainRef, string reminderName, string eTag)
        {
            throw new System.NotImplementedException();
        }

        public Task TestOnlyClearTable()
        {
            throw new System.NotImplementedException();
        }

        public Task<string> UpsertRow(ReminderEntry entry)
        {
            throw new System.NotImplementedException();
        }
    }
}