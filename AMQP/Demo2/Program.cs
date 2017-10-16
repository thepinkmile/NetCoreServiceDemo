using Demo2.Configuration;
using Demo2.Services;
using Microsoft.Extensions.Logging;
using ServiceHost;

namespace Demo2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new ServiceHostBuilder()
                // Add Configuration sources
                .AddEnvironmentConfiguration()
                .AddIniConfiguration("amqp.ini", false, true)
                .AddJsonConfiguration("appsettings.json", false, true)
                .AddCommandLine(args)

                // Add configuration Types to DI
                .Configure<AmqpOptions>("SiriAmqp")
                .Configure<AlexaOptions>("AlexaAmqp")

                // Add Logging services
                .AddLogging("Loging", builder =>
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Error)
                        .AddFilter("System", LogLevel.Error)
                        .AddConsole()
                        .AddDebug();
                })

                // Configure the services to run
                .IncludeService<SiriService>()
                .IncludeService<AlexaService>()

                // Build the ServiceHost
                .Build()

                // Start the ServiceHost
                .Run();
        }
    }
}
