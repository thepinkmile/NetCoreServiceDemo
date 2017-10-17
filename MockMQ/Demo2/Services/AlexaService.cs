using System;
using System.Globalization;
using System.Threading.Tasks;
using Demo2.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockMQ;
using MockMQ.Abstractions;
using ServiceHost;

namespace Demo2.Services
{
    public class AlexaService : PollingServiceBase
    {
        #region Constructors

        public AlexaService(IServiceProvider dependencies, IOptions<AlexaOptions> options, ILogger<AlexaService> logger)
            : base(dependencies)
        {
            _options = options;
            _logger = logger;
            _services = dependencies;
        }

        #endregion

        #region Properties

        private AlexaOptions Options => _options?.Value ?? new AlexaOptions();

        #endregion

        #region Methods

        public override async Task Execute()
        {
            _logger.LogDebug("Alexa Executing...");

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
                    _logger.LogInformation($"Message Received by Alexa: '{message.Body}'");

                    // Send response
                    var delivery = message.Body.Replace("Call ", "", true, CultureInfo.InvariantCulture);
                    var response = new Message { Body = $"Call {Options.SendToQueue}" };
                    _logger.LogDebug("Alexa is sending response");
                    await broker.SendMessageAsync(delivery, response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error executing Alexa Service");
            }
        }

        #endregion

        #region Fields

        private readonly IServiceProvider _services;

        private readonly IOptions<AlexaOptions> _options;

        private readonly ILogger<AlexaService> _logger;

        #endregion
    }
}
