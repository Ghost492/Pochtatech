using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Logging;

namespace Sender
{
    public class Program
    {
        private static readonly string _serviceMessageIdentifier = "serviceMessageIdentifier";
        private static IConfigurationRoot _configuration;
        private static ILogger _logger;

        static void Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            _logger = new ConsoleLogger();
            try
            {
                var token = new CancellationTokenSource();
                Task.Run(() => Run(token.Token), token.Token);
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

        private static void Run(CancellationToken token)
        {
           
           var messagesDataContext = new ServiceAMessagesStorage(_configuration.GetValue<string>("dbConnectionString"));
            using var sender = new Sender<Message>(_serviceMessageIdentifier, _logger);
            while (!token.IsCancellationRequested)
            {
                var message = messagesDataContext.GetNextMessage();
                if (message != null)
                {
                    message.SendTime = DateTime.UtcNow;
                    while (!sender.SendData(message))
                    {
                        if(token.IsCancellationRequested) return;
                        Thread.Sleep(1000);
                    }
                    messagesDataContext.Update(message);
                }
                Thread.Sleep(60*1000);
            }
        }
    }
}
