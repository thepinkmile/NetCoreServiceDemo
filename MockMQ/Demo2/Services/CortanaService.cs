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
    public class CortanaService : PollingServiceBase
    {
        #region Constructors

        public CortanaService(IServiceProvider dependencies, IOptions<CortanaOptions> options, ILogger<CortanaService> logger)
            : base(dependencies)
        {
            _options = options;
            _logger = logger;
            _services = dependencies;
        }

        #endregion

        #region Properties

        private CortanaOptions Options => _options?.Value ?? new CortanaOptions();

        #endregion

        #region Methods

        public override async Task Execute()
        {
            _logger.LogInformation("Cortana Executing...");

            try
            {
                using (var scopedServices = _services.CreateScope())
                {
                    var provider = scopedServices.ServiceProvider;

                    var broker = provider.GetRequiredService<IMessageBroker>();

                    // Retrieve Message
                    var message = await broker.GetMessageAsync(Options.QueueName, CancellationToken);
                    if (message == null) return;
                    _logger.LogInformation($"Message Received by Cortana: '{message.Body}'");

                    // Send response
                    var delivery = message.Body.Replace("Call ", "", true, CultureInfo.InvariantCulture);
                    var response = new Message { Body = $"Call {Options.SendToQueue}" };
                    _logger.LogInformation("Sending response");
                    await broker.SendMessageAsync(delivery, response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error executing Cortana Service");
            }
        }

        #endregion

        #region Fields

        private readonly IServiceProvider _services;

        private readonly IOptions<CortanaOptions> _options;

        private readonly ILogger<CortanaService> _logger;

        #endregion
    }
}
