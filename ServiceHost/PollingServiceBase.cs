using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceHost
{
    public abstract class PollingServiceBase : ServiceBase
    {
        #region Constructor

        protected PollingServiceBase(IServiceProvider dependencies)
            : base(dependencies)
        {
        }

        #endregion

        #region ServiceBase Methods
        
        public sealed override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            _task = Task.Run(() => Run(), CancellationToken);
        }

        public sealed override async Task StopAsync()
        {
            await base.StopAsync();
            await _task;
        }

        #endregion

        #region PollingMethods

        public void Run()
        {
            IsRunning = true;
            try
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    Execute();
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        public abstract void Execute();

        #endregion

        #region Fields

        private Task _task;

        #endregion
    }
}
