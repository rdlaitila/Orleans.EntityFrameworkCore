using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Tests
{
    public interface IStorageGrain : IGrainWithGuidKey
    {
        Task<string> GetString();

        Task SetString(string str);
    }

    public class StorageGrain : Grain<StorageGrainState>, IStorageGrain
    {
        public Task<string> GetString()
        {
            return Task.FromResult(State.String);
        }

        public async Task SetString(string str)
        {
            State.String = str;
            await WriteStateAsync();
            DeactivateOnIdle();
        }
    }

    public class StorageGrainState
    {
        public string String { get; set; }
    }
}