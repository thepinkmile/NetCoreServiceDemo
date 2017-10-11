using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using Amqp;
using Amqp.Framing;
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
                //.AddIniFile("amqp.ini", true, true)
                //.AddCommandLine(args)
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

            var amqpConnection = configuration["AMQP:Address"];

            var connection = new Connection(new Address(amqpConnection));
            var session = new Session(connection);

            // sending messages
            var senderId = configuration["AMQP:SenderSubscriptionId"];
            var topicId = configuration["AMQP:Topic"];
            var sender = new SenderLink(session, senderId, topicId);
            for (var i = 0; i < 10; ++i)
            {
                logger.LogDebug($"Sending Message {i}");
                var message = new Message($"Received Message {i}")
                {
                    Properties = new Properties { MessageId = Guid.NewGuid().ToString("N") },
                    ApplicationProperties =
                        new ApplicationProperties { ["Message.Type.FullName"] = typeof(string).FullName }
                };

                sender.Send(message);
            }
            sender.Close();

            // receiving messages
            var receiverId = configuration["AMQP:ReceiverSubscriptionId"];
            var subscription = configuration["AMQP:Subscription"];
            var consumer = new ReceiverLink(session, receiverId, $"{topicId}/Subscriptions/{subscription}");
            consumer.Start(5, (receiver, message) => OnMessageCallback(receiver, message, logger));

            // Wait for key to close app
            Console.Read();
        }

        private static void OnMessageCallback(IReceiverLink receiver, Message message, ILogger logger)
        {
            try
            {
                var type = message.ApplicationProperties["Message.Type.FullName"];

                logger.LogDebug($"Type of Message: {type}");
                //Console.WriteLine($"Type of Message: {type}");
                // TODO: we could use this to correctly convert the body back to the correct type

                string body;
                if (message.Body is byte[] bytes)
                {
                    using (var reader = XmlDictionaryReader.CreateBinaryReader(
                        new MemoryStream(bytes), null, XmlDictionaryReaderQuotas.Max))
                    {
                        var doc = new XmlDocument();
                        doc.Load(reader);
                        body = doc.InnerText;
                    }
                }
                else
                {
                    body = message.Body.ToString();
                }

                logger.LogDebug($"Message Body: {body}");
                //Console.WriteLine(body);

                receiver.Accept(message);
            }
            catch (Exception ex)
            {
                receiver.Reject(message);
                logger.LogError(ex, "Oh NO!!!, something is broken");
                //Console.WriteLine(ex);
            }
        }
    }
}
