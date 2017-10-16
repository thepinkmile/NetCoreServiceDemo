using System.Collections.Generic;

namespace MockMQ.Abstractions
{
    public interface IMessage
    {
       IDictionary<string, object> Properties { get; }

        string Body { get; }

        string QueueName { get; set; }

        IMessageBroker MessageBroker { get; set; }
    }
}
