using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ServiceHost.Abstractions;

namespace ServiceHost
{
    public abstract class PollingServiceBase : ServiceBase
    {
        #region Constructor

        protected PollingServiceBase(IServiceProvider dependencies)
            : base(dependencies)
        {
            _host = dependencies.GetRequiredService<IServiceHost>();
        }

        #endregion

        #region ServiceBase Methods

        public sealed override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            IsRunning = true;
            _task = Task.Run(() => Run(), CancellationToken);
            _task.GetAwaiter().OnCompleted(() => StopAsync().Wait(CancellationToken.None));
            _host.RegisterServiceTask(_task);
        }

        public sealed override async Task StopAsync()
        {
            await base.StopAsync();
            _host.UnregisterServiceTask(_task);
            IsRunning = false;
        }

        #endregion

        #region PollingMethods

        public void Run()
        {
            try
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    Execute().Wait();
                }
            }
            finally
            {
                StopAsync().Wait();
            }
        }

        public abstract Task Execute();

        #endregion

        #region Fields

        private Task _task;

        private IServiceHost _host;

        #endregion
    }
}
