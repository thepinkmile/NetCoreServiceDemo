using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceHost.Abstractions;

namespace ServiceHost
{
    public abstract class ServiceBase : IService
    {
        #region Constructors

        protected ServiceBase(IServiceProvider dependencies)
        {
            Services = dependencies ?? throw new ArgumentException("IServiceProvider was not provided.", nameof(dependencies));
        }

        #endregion

        #region Properties

        public IServiceProvider Services { get; }

        public bool IsRunning { get; protected set; }

        protected CancellationToken CancellationToken => _cts?.Token ?? default(CancellationToken);

        #endregion

        #region Methods

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (IsRunning) return Task.CompletedTask;

            cancellationToken.ThrowIfCancellationRequested();
            cancellationToken.Register(() => StopAsync().Wait(CancellationToken.None));
            _cts = new CancellationTokenSource();
            return Task.CompletedTask;
        }

        public virtual Task StopAsync()
        {
            if (IsRunning) _cts?.Cancel(false);
            return Task.CompletedTask;
        }

        #endregion

        #region IDispose

        public virtual void Dispose()
        {
            _cts?.Dispose();
        }

        #endregion

        #region Fields

        private CancellationTokenSource _cts;

        #endregion
    }
}
