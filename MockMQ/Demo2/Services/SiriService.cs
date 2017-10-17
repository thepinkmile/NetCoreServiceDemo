using System;
using System.Globalization;
using System.Threading.Tasks;
using Demo2.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockMQ.Abstractions;
using ServiceHost;
using MockMQ;

namespace Demo2.Services
{
    public class SiriService : PollingServiceBase
    {
        #region Constructors

        public SiriService(IServiceProvider dependencies, IOptions<SiriOptions> options, ILogger<SiriService> logger)
            : base(dependencies)
        {
            _options = options;
            _logger = logger;
            _services = dependencies;
        }

        #endregion

        #region Properties

        private SiriOptions Options => _options?.Value ?? new SiriOptions();

        #endregion

        #region Methods

        public override async Task Execute()
        {
            _logger.LogDebug("Siri Executing...");

            try
            {
                using (var scopedServices = _services.CreateScope())
                {
                    var provider = scopedServices.ServiceProvider;

                    var broker = provider.GetRequiredService<IMessageBroker>();

                    // Retrieve Message
                    var message = await broker.GetMessageAsync(Options.QueueName, CancellationToken);
                    if (message == null) return;
                    await broker.AcceptMessageAsync(message);
                    _logger.LogInformation($"Message Received by Siri: '{message.Body}'");

                    // Send response
                    var delivery = message.Body.Replace("Call ", "", true, CultureInfo.InvariantCulture);
                    var response = new Message{ Body = $"Call {Options.SendToQueue}" };
                    _logger.LogDebug("Siri is sending response");
                    await broker.SendMessageAsync(delivery, response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error executing Siri Service");
            }
        }

        #endregion

        #region Fields

        private readonly IServiceProvider _services;

        private readonly IOptions<SiriOptions> _options;

        private readonly ILogger<SiriService> _logger;

        #endregion
    }
}
