using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using DataContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Recipient
{
    public class Program
    {
        private static readonly string _serviceMessageIdentifier = "serviceMessageIdentifier";
        private static ILogger _logger;

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
                        .ForMember(d=>d.TimeOfReceipt,
                            o=> o.MapFrom(s=>DateTime.UtcNow));
                })
                .AddSingleton(new ServiceBMessagesStorage(dbConnectionString))
                .AddSingleton((s) => new Recipient(_serviceMessageIdentifier,
                    s.GetService<ServiceBMessagesStorage>(),
                    s.GetService<ILogger>(),
                    s.GetService<IMapper>()))
                .AddSingleton<ILogger>(s => s.GetService<ILoggerFactory>().CreateLogger<Program>())
                .BuildServiceProvider();
            return serviceProvider;
        }

        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            _logger = serviceProvider.GetService<ILogger>();
            try
            {
                var token = new CancellationTokenSource();
                var recepient = serviceProvider.GetService<Recipient>();
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
        private readonly IMapper _mapper;
        private readonly ServiceBMessagesStorage _storage;

        public Recipient(string subjectString, ServiceBMessagesStorage storage, ILogger logger, IMapper mapper)
        {
            _subjectString = subjectString;
            _logger = logger;
            _mapper = mapper;
            _storage = storage;
        }

        public void StartListening(CancellationToken cancellationToken)
        {
            EventHandler<EncodedMessageEventArgs> eventHandler = (sender, args) =>
            {
                var messageModel = args.ReceivedObject as MessageModel;
                if (messageModel == null)
                {
                    return;
                }
                var message = _mapper.Map<ServiceBMessagesStorage.Message>(messageModel);
                _storage.Create(message);
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var connection = new ConnectionFactory().CreateEncodedConnection())
                    {
                        connection.OnDeserialize = JsonSerializer<MessageModel>.Deserialize;
                        using (var s = connection.SubscribeAsync(_subjectString, eventHandler))
                        {
                            WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle });
                        }
                    }
                }
                catch (NATSNoServersException e)
                {
                    _logger.LogError(e.Message);
                }
            }
        }
    }
}
