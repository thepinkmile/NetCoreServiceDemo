using System.Collections.Generic;
using MockMQ.Abstractions;

namespace MockMQ
{
    public class Message : IMessage
    {
        #region Constructors

        public Message()
        {
            Properties = new Dictionary<string, object>();
        }

        #endregion

        #region Properties

        public IDictionary<string, object> Properties { get; set; }

        public string Body { get; set; }

        public string QueueName { get; set; }

        public IMessageBroker MessageBroker { get; set; }

        #endregion

        #region EqualityComparison

        public override bool Equals(object obj)
        {
            if (obj is IMessage msg)
                return Body == msg.Body && Properties.Equals(msg.Properties);
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"Message [Properties: {Properties.Count}, Body Length: {Body.Length}]";
        }

        #endregion
    }
}
