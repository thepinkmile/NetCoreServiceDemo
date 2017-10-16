using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MockMQ.Abstractions
{
    public interface IMessageBroker
    {
        #region Async

        Task<IMessage> GetMessageAsync(string queueName);

        Task<IMessage> GetMessageAsync(string queueName, Func<IDictionary<string, object>, bool> predicateFunc);

        Task AcceptMessageAsync(IMessage message);

        Task RejectMessageAsync(IMessage message);

        Task SendMessageAsync(string queueName, IMessage message);

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
