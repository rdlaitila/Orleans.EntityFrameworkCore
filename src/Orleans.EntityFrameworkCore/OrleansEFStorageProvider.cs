using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;
using System;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore
{
    /// <summary>
    /// Provides an IStorageProvider implementation wrapping orleans operations
    /// translating them into entity framework calls
    /// </summary>
    internal class OrleansEFStorageProvider : IStorageProvider
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<OrleansEFStorageProvider> _logger;

        /// <summary>
        /// The grain factory
        /// </summary>
        private readonly IGrainFactory _grainFactory;

        /// <summary>
        /// The serialization manager
        /// </summary>
        private readonly SerializationManager _serializationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrleansEFStorageProvider"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="grainFactory">The grain factory.</param>
        /// <param name="serializationManager">The serialization manager.</param>
        /// <exception cref="ArgumentNullException">
        /// logger
        /// or
        /// grainFactory
        /// or
        /// serializationManager
        /// </exception>
        public OrleansEFStorageProvider(
            ILogger<OrleansEFStorageProvider> logger,
            IGrainFactory grainFactory,
            SerializationManager serializationManager
        )
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            _grainFactory = grainFactory ??
                throw new ArgumentNullException(nameof(grainFactory));

            _serializationManager = serializationManager ??
                throw new ArgumentNullException(nameof(serializationManager));
        }

        /// <summary>
        /// Clears the state asynchronous.
        /// </summary>
        /// <param name="grainType">Type of the grain.</param>
        /// <param name="grainReference">The grain reference.</param>
        /// <param name="grainState">State of the grain.</param>
        /// <returns></returns>
        public async Task ClearStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            try
            {
                await _grainFactory
                   .GetGrain<IOrleansEFStorageGrain>(grainReference.ToKeyString())
                   .ClearStateAsync(grainType, grainReference, grainState);
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(ClearStateAsync), e);
                throw;
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        /// <returns></returns>
        public Task Close()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="providerRuntime">The provider runtime.</param>
        /// <param name="config">The configuration.</param>
        /// <returns></returns>
        public Task Init(
            string name,
            IProviderRuntime providerRuntime,
            IProviderConfiguration config
        )
        {
            Name = name;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads the state asynchronous.
        /// </summary>
        /// <param name="grainType">Type of the grain.</param>
        /// <param name="grainReference">The grain reference.</param>
        /// <param name="grainState">State of the grain.</param>
        /// <returns></returns>
        public async Task ReadStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            try
            {
                var state = await _grainFactory
                   .GetGrain<IOrleansEFStorageGrain>(grainReference.ToKeyString())
                   .ReadStateAsync(grainType, grainReference, grainState);

                grainState.State = state.State;
                grainState.ETag = state.ETag;
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(ReadStateAsync), e);
                throw;
            }
        }

        /// <summary>
        /// Writes the state asynchronous.
        /// </summary>
        /// <param name="grainType">Type of the grain.</param>
        /// <param name="grainReference">The grain reference.</param>
        /// <param name="grainState">State of the grain.</param>
        /// <returns></returns>
        public async Task WriteStateAsync(
            string grainType,
            GrainReference grainReference,
            IGrainState grainState
        )
        {
            try
            {
                await _grainFactory
                    .GetGrain<IOrleansEFStorageGrain>(grainReference.ToKeyString())
                    .WriteStateAsync(grainType, grainReference, grainState);

                return;
            }
            catch (Exception e)
            {
                _logger.Error(0, nameof(WriteStateAsync), e);
                throw;
            }
        }
    }
}