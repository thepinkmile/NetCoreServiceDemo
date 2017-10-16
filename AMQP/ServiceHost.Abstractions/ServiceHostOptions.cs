using System;

namespace ServiceHost.Abstractions
{
    public class ServiceHostOptions
    {
        public TimeSpan ShutdownTimeout { get; set; }
    }
}
