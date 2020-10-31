using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Recipient
{
    public class Program
    {
        private static readonly string _serviceMessageIdentifier = "serviceMessageIdentifier";
        private static IConfigurationRoot _configuration;
        private static ConsoleLogger _logger;

        static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            _logger = new ConsoleLogger();
            var messagesDataContext = new ServiceAMessagesStorage(_configuration.GetValue<string>("dbConnectionString"));
            try
            {
                var token = new CancellationTokenSource();
                var recepient = new Recipient(_serviceMessageIdentifier, _logger, messagesDataContext);
                Task.Run(() => recepient.StartListening(token.Token), token.Token);
                Console.ReadLine();
                token.Cancel();

            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            Console.WriteLine("Application will be closed. Please wait 10 seconds...");
            Thread.Sleep(10 * 1000);
        }


    }

    public class Recipient
    {
        private readonly string _subjectString;
        private readonly ILogger _logger;
        private readonly ServiceAMessagesStorage _messagesDataContext;

        public Recipient(string subjectString, ILogger logger, ServiceAMessagesStorage messagesDataContext)
        {
            _subjectString = subjectString;
            _logger = logger;
            _messagesDataContext = messagesDataContext;
        }

        private void Handler(object? sender, EncodedMessageEventArgs args)
        {

        }

        public void StartListening(CancellationToken cancellationToken)
        {
            EventHandler<EncodedMessageEventArgs> eventHandler = (sender, args) =>
            {
                var message = args.ReceivedObject as Message;
                if (message == null)
                {
                    return;
                }
                _messagesDataContext.Create(message);
            };
            using (var connection = new ConnectionFactory().CreateEncodedConnection())
            {
                connection.OnDeserialize = JsonSerializer<Message>.Deserialize;
                using (var s = connection.SubscribeAsync(_subjectString, eventHandler))
                {
                    WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle });
                }
            }

        }
    }
}
