using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceHost.Abstractions;

namespace ServiceHost
{
    public class ServiceHost : IServiceHost
    {
        #region Constructors

        public ServiceHost(IServiceProvider dependencies)
        {
            Dependencies = dependencies ?? throw new ArgumentNullException(nameof(dependencies));
            _logger = Dependencies.GetRequiredService<ILogger<ServiceHost>>();
            _runningServices = new List<Task>();
            _services = new List<IService>();
            _options = Dependencies.GetService<IOptions<ServiceHostOptions>>()?.Value ?? new ServiceHostOptions();
        }

        #endregion

        #region Properties

        public IServiceProvider Dependencies { get; }

        public bool IsRunning => _services.Any(s => s.IsRunning) || _runningServices.Any();

        #endregion

        #region Methods

        public void AddTaskService(Type serviceType)
        {
            if (!typeof(IService).IsAssignableFrom(serviceType))
                throw new ArgumentException("Type provided is not a valid IService type.", nameof(serviceType));

            _services.Add((IService)Dependencies.GetService(serviceType));
        }

        public void Run()
        {
            if (!IsRunning)
                Start();

            Task.WaitAll(_runningServices.ToArray());
        }

        public void Start()
        {
            if (IsRunning)
                throw new InvalidOperationException("Services are already running");

            Console.CancelKeyPress += CancelKeyPress;
            _cancellationTokenSource = new CancellationTokenSource();
            foreach (var service in _services)
            {
                _logger.LogDebug($"Starting service: {service.GetType().FullName}");
                try
                {
                    var task = service.StartAsync(_cancellationTokenSource.Token);
                    task.ConfigureAwait(true).GetAwaiter().OnCompleted(() => RemoveTask(task));
                    _runningServices.Append(task);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "A service failed to start");
                }
            }
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _logger.LogWarning("Stopping services.");
            Console.CancelKeyPress -= CancelKeyPress;
            _cancellationTokenSource.Cancel(false); // Signal all services to stop

            Task.WaitAll(_runningServices.ToArray(), _options.ShutdownTimeout);
            if (_services.Any(s => s.IsRunning))
            {
                _logger.LogWarning("Some services failed to complete within the specified timeout.");
                foreach (var service in _services.Where(s => s.IsRunning))
                {
                    _logger.LogInformation($"Forcing stop of service: {service.GetType().FullName}");
                    try
                    {
                        if (service.IsRunning)
                            service.StopAsync().Wait();
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "A service failed to stop");
                    }
                }
                Task.WaitAll(_runningServices.ToArray());
                _logger.LogInformation("All stray services have now completed");
            }
            _runningServices.Clear();

            _cancellationTokenSource = null;
            if (IsRunning)
                _logger.LogError("No running services registered but services still show IsRunning=true");
        }

        #endregion

        #region Internal

        private void RemoveTask(Task task)
        {
            if (_runningServices.Contains(task))
                _runningServices.Remove(task);
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Stop();
        }

        #endregion

        #region Fields

        private readonly IList<IService> _services;

        private readonly ILogger<ServiceHost> _logger;

        private readonly IList<Task> _runningServices;

        private readonly ServiceHostOptions _options;

        private CancellationTokenSource _cancellationTokenSource;

        #endregion
    }
}
