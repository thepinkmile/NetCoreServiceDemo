using System;
using System.Threading.Tasks;
using Amqp;
using Demo2.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceHost;

namespace Demo2.Services
{
    public class AlexaService : PollingServiceBase
    {
        #region Constructors

        public AlexaService(IServiceProvider dependencies, IOptions<AlexaOptions> options, ILogger<AlexaService> logger)
            : base(dependencies)
        {
            var options1 = options?.Value ?? new AlexaOptions();
            _logger = logger;

            var address = new Address(options1.Address);
            _amqpConnection = new Connection(address);
            _amqpSession = new Session(_amqpConnection);
            _amqpReceiverLink = new ReceiverLink(_amqpSession, options1.ReceiverSubscriptionId, options1.Subscription);
            _amqpSenderLink = new SenderLink(_amqpSession, options1.SenderSubscriptionId, options1.Topic);
        }

        #endregion

        #region Methods

        public override async Task Execute()
        {
            _logger.LogInformation("Alexa Executing...");

            var message = await _amqpReceiverLink.ReceiveAsync();
            CancellationToken.ThrowIfCancellationRequested();
            // TODO: add message validation here
            _amqpReceiverLink.Accept(message);
            _logger.LogWarning($"Message Received by Alexa: '{message.Body}'");

            CancellationToken.ThrowIfCancellationRequested();
            var reply = new Message("Hey Siri, Call me...");
            await _amqpSenderLink.SendAsync(reply);
        }

        #endregion

        #region IDispose

        public override void Dispose()
        {
            if (!_amqpReceiverLink.IsClosed) _amqpReceiverLink.Close();
            if (!_amqpSenderLink.IsClosed) _amqpSenderLink.Close();
            if (!_amqpSession.IsClosed) _amqpSession.Close();
            if (!_amqpConnection.IsClosed) _amqpConnection.Close();
            base.Dispose();
        }

        #endregion

        #region Fields

        private readonly ILogger<AlexaService> _logger;

        private readonly Session _amqpSession;

        private readonly Connection _amqpConnection;

        private readonly ReceiverLink _amqpReceiverLink;

        private readonly SenderLink _amqpSenderLink;

        #endregion
    }
}
