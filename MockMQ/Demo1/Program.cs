using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockMQ;
using MockMQ.Abstractions;

namespace Demo1
{
    public class Program
    {
        [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddIniFile("msgsettings.ini", true, true)
                .AddCommandLine(args)
                .Build();

            var serviceCollection = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConsole()
                        .AddDebug();
                })
                .AddSingleton<IMessageBroker, MessageBroker>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application Logging Initialized");

            // get config values
            var queueName = configuration["Broker:QueueName"];
            var senderName = configuration["Broker:SenderName"];
            var receiverName = configuration["Broker:ReceiverName"];

            // retrieve DI MessageBroker
            var broker = serviceProvider.GetRequiredService<IMessageBroker>();

            // Send some messages
            var message = new Message
            {
                Properties =
                {
                    {"Type", typeof(string)},
                    {"Encoding", Encoding.UTF8 }
                },
                Body = $"Message for {receiverName} sent by {senderName}"
            };
            logger.LogInformation("Sending Message...");
            broker.SendMessage(queueName, message);

            // read message
            logger.LogInformation("Receiving Message...");
            var retrievedMessage = broker
                .GetMessageAsync(queueName, p => p.ContainsKey("Type") && p["Type"].Equals(typeof(string)))
                .Result;
            logger.LogInformation($"Message Body: {retrievedMessage.Body}");

            // blocking request for same message
            logger.LogInformation("Making second request for same message...");
            var attemptToRead2 = Task.Run(() => broker.GetMessageAsync(queueName));
            logger.LogDebug($"Completed: {attemptToRead2.IsCompleted}");
            for (var i = 0; i < 10; ++i)
            {
                Thread.Sleep(100);
                logger.LogInformation($"Completed: {attemptToRead2.IsCompleted}");
            }
            attemptToRead2.ContinueWith(task =>
            {
                // This should fire after the original recipt has been rejected
                var msg = task.Result;
                logger.LogWarning($"Message Received using ContinueWith(...): {msg.Body}");
                broker.AcceptMessage(msg);
            });

            // reject original message recipt
            logger.LogWarning("Rejecting initial handler");
            broker.RejectMessage(retrievedMessage);

            Thread.Sleep(5000);

            // Wait for key to close app
            logger.LogError("Application completed. Press key to exit.");
            Console.Read();
        }
    }
}
