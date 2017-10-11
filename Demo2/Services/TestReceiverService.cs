using System;
using System.Threading;
using Demo2.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceHost;

namespace Demo2.Services
{
    public class TestReceiverService : PollingServiceBase
    {
        #region Constructors

        public TestReceiverService(IServiceProvider dependencies, IOptions<AmqpOptions> options, ILogger<TestReceiverService> logger)
            : base(dependencies)
        {
            _options = options?.Value ?? new AmqpOptions();
            _logger = logger;
        }

        #endregion

        #region Methods

        public override void Execute()
        {
            _logger.LogInformation("Executing...");
            _logger.LogInformation($"AMQP SubscriptionId: {_options.SenderSubscriptionId}");

            Thread.Sleep(1000);
            // TODO: Add MessageSender Here
        }

        #endregion

        #region Fields

        private readonly AmqpOptions _options;

        private readonly ILogger<TestReceiverService> _logger;

        #endregion
    }
}
