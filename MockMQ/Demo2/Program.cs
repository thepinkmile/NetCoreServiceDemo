using Demo2.Configuration;
using Demo2.Services;
using Microsoft.Extensions.Logging;
using MockMQ;
using MockMQ.Abstractions;
using ServiceHost;

namespace Demo2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // create initial broker
            var broker = new MessageBroker();

            // put first message on the queue
            var testMessage = new Message{ Body = "Call siri" };
            broker.SendMessage("cortana", testMessage);

            var host = new ServiceHostBuilder()
                // Add Configuration sources
                .AddEnvironmentConfiguration()
                .AddJsonConfiguration("logging.json", false, true)
                .AddIniConfiguration("siri.ini", false, true)
                .AddXmlConfiguration("cortana.xml", false, true)
                .AddJsonConfiguration("alexa.json", false, true)
                .AddCommandLine(args)

                // Add configuration Types to DI
                .Configure<SiriOptions>("Siri")
                .Configure<AlexaOptions>("Alexa")
                .Configure<CortanaOptions>("Cortana")

                // Add Logging services
                .AddLogging("Loging", builder =>
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Error)
                        .AddFilter("System", LogLevel.Error)
                        .AddConsole()
                        .AddDebug();
                })

                // Add normal dependencies
                .TryAddSingleton<IMessageBroker>(broker)

                // Configure the services to run
                .IncludeService<SiriService>()
                .IncludeService<AlexaService>()
                .IncludeService<CortanaService>()

                // Build the ServiceHost
                .Build();

            // Start the ServiceHost
            host.Run();
        }
    }
}
