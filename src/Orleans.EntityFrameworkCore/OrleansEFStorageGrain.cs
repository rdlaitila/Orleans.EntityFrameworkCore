using Microsoft.EntityFrameworkCore;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Serialization;
using System;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Orleans.IGrainWithStringKey" />
    internal interface IOrleansEFStorageGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Clears the state asynchronous.
        /// </summary>
        /// <param name="grainType">Type of the grain.</param>
        /// <param name="grainReference">The grain reference.</param>
        /// <param name="grainState">State of the grain.</param>
        /// <returns></returns>
        Task ClearStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        );

        /// <summary>
        /// Reads the state asynchronous.
        /// </summary>
        /// <param name="grainType">Type of the grain.</param>
        /// <param name="grainReference">The grain reference.</param>
        /// <param name="grainState">State of the grain.</param>
        /// <returns></returns>
        Task<IGrainState> ReadStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        );

        /// <summary>
        /// Writes the state asynchronous.
        /// </summary>
        /// <param name="grainType">Type of the grain.</param>
        /// <param name="grainReference">The grain reference.</param>
        /// <param name="grainState">State of the grain.</param>
        /// <returns></returns>
        Task WriteStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        );
    }

    [StatelessWorker(1)]
    internal class OrleansEFStorageGrain : Grain, IOrleansEFStorageGrain
    {
        private readonly OrleansEFContext _db;

        private readonly SerializationManager _serializationManager;

        public OrleansEFStorageGrain(
            OrleansEFContext db,
            SerializationManager serializationManager
        )
        {
            _db = db;
            _serializationManager = serializationManager;
        }

        public async Task ClearStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            var dbGrain = await _db
                .Storage
                .FirstOrDefaultAsync(a =>
                    a.Type == grainType &&
                    a.PrimaryKey == grainReference.ToKeyString()
                );

            if (dbGrain == null)
                return;

            _db.Storage.Remove(dbGrain);

            await _db.SaveChangesAsync();

            grainState.ETag = null;
        }

        public async Task<IGrainState> ReadStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            var dbGrain = await _db
                .Storage
                .FirstOrDefaultAsync(a =>
                    a.Type == grainType &&
                    a.PrimaryKey == grainReference.ToKeyString()
                );

            if (dbGrain == null)
                return grainState;

            grainState.ETag = dbGrain.ETag;
            grainState.State = _serializationManager
                .DeserializeFromByteArray<object>(dbGrain.BinaryData);

            return grainState;
        }

        public async Task WriteStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            var dbGrain = await _db
                .Storage
                .FirstOrDefaultAsync(a =>
                    a.Type == grainType &&
                    a.PrimaryKey == grainReference.ToKeyString()
                );

            if (dbGrain == null)
            {
                dbGrain = new OrleansEFStorage
                {
                    Type = grainType,
                    PrimaryKey = grainReference.ToKeyString(),
                };

                _db.Add(dbGrain);
            }

            var etagMismatch =
                !string.IsNullOrWhiteSpace(dbGrain.ETag) &&
                dbGrain.ETag != grainState.ETag;

            if (etagMismatch)
                throw new OrleansEFStorageException.EtagMismatch(
                    $"etag mismatch. " +
                    $"grainType: {grainType} " +
                    $"grainId: {grainReference.ToKeyString()} " +
                    $"storedEtag: {dbGrain.ETag} " +
                    $"suppliedEtag: {grainState.ETag}"
                );

            dbGrain.ETag = Guid.NewGuid().ToString();
            dbGrain.BinaryData = _serializationManager
                .SerializeToByteArray(grainState.State);

            await _db.SaveChangesAsync();
        }
    }
}