using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Recipient
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
                var recepient = serviceProvider.GetService<MessageListiner>();
                Task.Run(() => recepient.StartListening(token.Token), token.Token);

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
                    config.CreateMap<MessageModel, ServiceBMessagesStorage.Message>()
                        .ForMember(d => d.TimeOfReceipt,
                            o => o.MapFrom(s => DateTime.UtcNow));
                })
                .AddSingleton(new ServiceBMessagesStorage(dbConnectionString))
                .AddSingleton((s) => new MessageListiner(_serviceMessageIdentifier,
                    s.GetService<ServiceBMessagesStorage>(),
                    s.GetService<ILogger>(),
                    s.GetService<IMapper>()))
                .AddSingleton<ILogger>(s => s.GetService<ILoggerFactory>().CreateLogger<Program>())
                .BuildServiceProvider();
            return serviceProvider;
        }

    }
}
