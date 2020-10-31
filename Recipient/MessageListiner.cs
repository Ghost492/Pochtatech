using System.Threading;
using AutoMapper;
using Common;
using DataContext;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Recipient
{
    public class MessageListiner
    {
        private readonly string _subjectString;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IDataStorage<ServiceBMessagesStorage.Message> _storage;

        public MessageListiner(string subjectString, IDataStorage<ServiceBMessagesStorage.Message> storage, ILogger logger, IMapper mapper)
        {
            _subjectString = subjectString;
            _logger = logger;
            _mapper = mapper;
            _storage = storage;
        }
        private void EventHandler(object? sender, EncodedMessageEventArgs args)
        {
            var messageModel = args.ReceivedObject as MessageModel;
            if (messageModel == null)
            {
                return;
            }

            var message = _mapper.Map<ServiceBMessagesStorage.Message>(messageModel);
            _storage.Create(message);
        }

        public void StartListening(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (var connection = new ConnectionFactory().CreateEncodedConnection())
                    {
                        connection.OnDeserialize = JsonSerializer<MessageModel>.Deserialize;
                        using (var s = connection.SubscribeAsync(_subjectString, EventHandler))
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