using System;
using System.Diagnostics.CodeAnalysis;
using Amqp;
using Amqp.Framing;
using Amqp.Sasl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo1
{
    public class Program
    {
        [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddIniFile("amqp.ini", true, true)
                .AddCommandLine(args)
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole()
                        .AddDebug();
                });
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application Logging Initialized");

            // get config values
            var amqpConnection = configuration["AMQP:Address"];
            var senderId = configuration["AMQP:SenderSubscriptionId"];
            var topicId = configuration["AMQP:Topic"];
            var receiverId = configuration["AMQP:ReceiverSubscriptionId"];
            var subscription = configuration["AMQP:Subscription"];

            // setup amqp session
            var connection = new Connection(new Address(amqpConnection), SaslProfile.Anonymous, null, null);
            var session = new Session(connection);

            // sending messages
            var sender = new SenderLink(session, senderId, topicId);
            logger.LogDebug("Sending Message...");
            var message = new Message("Hello World")
            {
                Properties = new Properties
                {
                    MessageId = Guid.NewGuid().ToString("N")
                }
            };
            sender.Send(message);
            sender.Close();

            // receiving messages
            logger.LogDebug("Receiving Message...");
            var consumer = new ReceiverLink(session, $"{receiverId}{{create: always}}", $"{topicId}/Subscriptions/{subscription}");
            var msg = consumer.Receive();
            consumer.Accept(msg);
            consumer.Close();

            var type = msg.ApplicationProperties["Message.Type.FullName"];
            logger.LogDebug($"Type of Message: {type}");
            logger.LogDebug($"Message Body: {msg.Body}");

            session.Close();
            connection.Close();

            // Wait for key to close app
            Console.Read();
        }
    }
}
