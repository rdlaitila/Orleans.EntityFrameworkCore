using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests.App
{
    public interface IStorageGrain : IGrainWithGuidKey
    {
        Task<StorageState> GetState();

        Task IncrementCounter();
    }

    public class StorageGrain : Grain<StorageState>, IStorageGrain
    {
        public Task<StorageState> GetState()
        {
            return Task.FromResult(State);
        }

        public async Task IncrementCounter()
        {
            State.Counter++;
            await WriteStateAsync();
        }
    }
}