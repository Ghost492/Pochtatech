using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Sender
{
    public class Program
    {
        private static readonly string _serviceMessageIdentifier = "serviceMessageIdentifier";
        private static ILogger _logger;

        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            _logger = serviceProvider.GetService<ILogger>();

            try
            {
                var token = new CancellationTokenSource();
                Task.Run(() => StartService(token.Token, serviceProvider), token.Token);
                Console.ReadLine();
                token.Cancel();

            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
            Console.WriteLine("Application will be closed. Please, wait 10 seconds...");
            Thread.Sleep(10 * 1000);
        }
        private static ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            var dbConnectionString = configuration.GetValue<string>("dbConnectionString");
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddAutoMapper(config =>
                {
                    config.CreateMap<ServiceAMessagesStorage.Message, MessageModel>();
                })
                .AddSingleton(new ServiceAMessagesStorage(dbConnectionString))
                .AddSingleton((s) => new DataSender<MessageModel>(_serviceMessageIdentifier, s.GetService<ILogger>()))
                .AddSingleton<ILogger>(s => s.GetService<ILoggerFactory>().CreateLogger<Program>())
                .BuildServiceProvider();
            return serviceProvider;
        }

        private static void StartService(CancellationToken token, ServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILogger>();
            var storage = serviceProvider.GetService<ServiceAMessagesStorage>();
            var mapper = serviceProvider.GetService<IMapper>();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var sender = serviceProvider.GetService<DataSender<MessageModel>>();
                    while (!token.IsCancellationRequested)
                    {
                        var message = storage.GetNext();
                        if (message != null)
                        {
                            message.SendTime = DateTime.UtcNow;
                            var messageModel = mapper.Map<MessageModel>(message);
                            while (!sender.SendData(messageModel))
                            {
                                if (token.IsCancellationRequested) return;
                                Thread.Sleep(1000);
                            }
                            storage.Update(message);
                        }
                        Thread.Sleep(1000);
                    }
                }
                catch (NATSNoServersException e)
                {
                    logger.LogError(e.Message);
                }
            }
        }
    }
}
