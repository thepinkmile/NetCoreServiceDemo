using System;
using System.Threading;
using Demo2.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceHost;

namespace Demo2.Services
{
    public class TestService : PollingServiceBase
    {
        #region Constructors

        public TestService(IServiceProvider dependencies, IOptions<AmqpOptions> options, ILogger<TestService> logger)
            : base(dependencies)
        {
            _options = options?.Value ?? new AmqpOptions();
            _logger = logger;
        }

        #endregion

        #region Methods

        public override void Execute()
        {
            _logger.LogDebug($"AMQP Address: {_options.Address}");
            _logger.LogDebug($"AMQP Topic: {_options.Topic}");

            Thread.Sleep(1000);
            // TODO: Add MessageSender Here
        }

        #endregion

        #region Fields

        private readonly AmqpOptions _options;

        private readonly ILogger<TestService> _logger;

        #endregion
    }
}
