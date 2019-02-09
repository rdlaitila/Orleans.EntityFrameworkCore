using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Serialization;
using System;
using System.Text;
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
        Task<IGrainState> ClearStateAsync(
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
        Task<IGrainState> WriteStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        );
    }

    internal class OrleansEFStorageGrain : Grain, IOrleansEFStorageGrain
    {
        private readonly ILogger<OrleansEFStorageGrain> _logger;

        private readonly OrleansEFContext _db;

        private readonly SerializationManager _serializationManager;

        public OrleansEFStorageGrain(
            ILogger<OrleansEFStorageGrain> logger,
            OrleansEFContext db,
            SerializationManager serializationManager
        )
        {
            _db = db ??
                throw new ArgumentNullException(nameof(db));

            _serializationManager = serializationManager ??
                throw new ArgumentNullException(nameof(serializationManager));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IGrainState> ClearStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(grainType))
                    throw new ArgumentNullException(nameof(grainType));

                grainReference = grainReference ??
                    throw new ArgumentNullException(nameof(grainReference));

                grainState = grainState ??
                    throw new ArgumentNullException(nameof(grainState));

                grainState.ETag =
                    null;

                grainState.State =
                    null;

                var dbGrain = await _db
                    .Storage
                    .FirstOrDefaultAsync(a =>
                        a.Type == grainType &&
                        a.PrimaryKey == grainReference.ToKeyString()
                    );

                if (dbGrain == null)
                    return grainState;

                _db.Storage.Remove(dbGrain);

                await _db.SaveChangesAsync();

                return grainState;
            }
            catch (Exception e)
            {
                _logger.LogError(0, e, nameof(ClearStateAsync));
                throw;
            }
        }

        public async Task<IGrainState> ReadStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(grainType))
                    throw new ArgumentNullException(nameof(grainType));

                grainReference = grainReference ??
                    throw new ArgumentNullException(nameof(grainReference));

                grainState = grainState ??
                    throw new ArgumentNullException(nameof(grainState));

                var dbGrain = await _db
                    .Storage
                    .FirstOrDefaultAsync(a =>
                        a.Type == grainType &&
                        a.PrimaryKey == grainReference.ToKeyString()
                    );

                if (dbGrain == null)
                {
                    grainState.ETag = null;
                    grainState.State = null;

                    return grainState;
                }

                grainState.ETag =
                    dbGrain.ETag;

                var dbGrainData = Convert
                    .FromBase64String(dbGrain.Data);

                grainState.State = _serializationManager
                    .DeserializeFromByteArray<object>(dbGrainData);

                return grainState;
            }
            catch (Exception e)
            {
                _logger.LogError(0, e, nameof(ReadStateAsync));
                throw;
            }
        }

        public async Task<IGrainState> WriteStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(grainType))
                    throw new ArgumentNullException(nameof(grainType));

                grainReference = grainReference ??
                    throw new ArgumentNullException(nameof(grainReference));

                grainState = grainState ??
                    throw new ArgumentNullException(nameof(grainState));

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
                        Id = Guid.NewGuid(),
                        Type = grainType,
                        PrimaryKey = grainReference.ToKeyString(),
                        ETag = grainState.ETag,
                    };

                    _db.Add(dbGrain);
                }

                var etagMismatch =
                    dbGrain.ETag != grainState.ETag;

                if (etagMismatch)
                    throw new OrleansEFStorageException.EtagMismatch(
                        $"etag mismatch. " +
                        $"grainType: {grainType} " +
                        $"grainId: {grainReference.ToKeyString()} " +
                        $"storedEtag: {dbGrain.ETag} " +
                        $"suppliedEtag: {grainState.ETag}"
                    );

                dbGrain.ETag =
                    Guid.NewGuid().ToString();

                var serializedData = _serializationManager
                    .SerializeToByteArray(grainState.State);

                dbGrain.Data = Convert
                    .ToBase64String(serializedData);

                await _db.SaveChangesAsync();

                grainState.ETag =
                    dbGrain.ETag;

                return grainState;
            }
            catch (Exception e)
            {
                _logger.LogError(0, e, nameof(WriteStateAsync));
                throw;
            }
        }
    }
}