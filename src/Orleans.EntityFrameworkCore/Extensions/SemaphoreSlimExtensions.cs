using System;
using System.Threading;
using System.Threading.Tasks;

namespace Orleans.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// </summary>
    internal static class SemaphoreSlimExtensions
    {
        /// <summary>
        /// Disposables the wait asynchronous.
        /// </summary>
        /// <param name="semaphore">The semaphore.</param>
        /// <param name="cancelToken">The cancel token.</param>
        /// <returns></returns>
        internal static async Task<IDisposable> DisposableWaitAsync(
            this SemaphoreSlim semaphore,
            CancellationToken cancelToken = default
        )
        {
            await semaphore
                .WaitAsync(cancelToken)
                .ConfigureAwait(false);

            return new ReleaseWrapper(semaphore);
        }

        private class ReleaseWrapper : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;
            private bool _isDisposed;

            public ReleaseWrapper(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                _semaphore.Release();
                _isDisposed = true;
            }
        }
    }
}