using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceHost.Abstractions
{
    /// <inheritdoc />
    /// <summary>
    /// A service abstraction.
    /// </summary>
    public interface IService : IDisposable
    {
        /// <summary>
        /// The services configured dependencies.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Flag to show service is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Start the service.
        /// </summary>
        /// <param name="cancellationToken">Used to abort program start.</param>
        /// <returns></returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to gracefully stop the service.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
