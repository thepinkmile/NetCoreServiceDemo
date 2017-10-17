using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MockMQ.Abstractions
{
    public interface IMessageBroker
    {
        #region Async

        Task<IMessage> GetMessageAsync(string queueName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IMessage> GetMessageAsync(string queueName, Func<IDictionary<string, object>, bool> predicateFunc, CancellationToken cancellationToken = default(CancellationToken));

        Task AcceptMessageAsync(IMessage message, CancellationToken cancellationToken = default(CancellationToken));

        Task RejectMessageAsync(IMessage message, CancellationToken cancellationToken = default(CancellationToken));

        Task SendMessageAsync(string queueName, IMessage message, CancellationToken cancellationToken = default(CancellationToken));

        #endregion

        #region Sync

        IMessage GetMessage(string queueName);

        IMessage GetMessage(string queueName, Func<IDictionary<string, object>, bool> predicateFunc);

        void AcceptMessage(IMessage message);

        void RejectMessage(IMessage message);

        void SendMessage(string queueName, IMessage message);

        #endregion
    }
}
