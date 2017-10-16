using System;

namespace Demo2.Configuration
{
    public class AmqpOptions
    {
        public string Address { get; set; } = "amqps://guest:guest@locahost";

        public string SenderSubscriptionId { get; set; } = Guid.NewGuid().ToString("N");

        public string Topic { get; set; } = "Topic";

        public string ReceiverSubscriptionId { get; set; } = Guid.NewGuid().ToString("N");

        public string Subscription { get; set; } = "Subscription";
    }
}
