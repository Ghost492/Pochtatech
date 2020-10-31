using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Common;
using DataContext;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Recipient;
using Sender;

namespace Tests
{
    public class RecipientTests
    {
        private Mock<ILogger> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>(MockBehavior.Strict);
        }

        [Test]
        public void RecipientShouldSaveMessage()
        {
            var messageModel = new MessageModel()
            {
                Hash = 123,
                MessageNumber = 1,
                Text = "text"
            };
            var message = new ServiceBMessagesStorage.Message()
            {
                Hash = 123,
                MessageNumber = 1,
                Text = "text",
                SendTime = DateTime.UtcNow,
                TimeOfReceipt = DateTime.UtcNow.AddMilliseconds(100)
            };
            var mockStorage = new Mock<IDataStorage<ServiceBMessagesStorage.Message>>();
            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(m => m.Map<ServiceBMessagesStorage.Message>(It.IsAny<MessageModel>()))
                .Returns(message);

            var subjectString = "testConnection";
            var listener = new MessageListiner(subjectString, mockStorage.Object, _mockLogger.Object, mockMapper.Object);
            var token = new CancellationTokenSource();
            var sender = new DataSender<MessageModel>(subjectString, _mockLogger.Object);

            Task.Run(() => listener.StartListening(token.Token), token.Token);
            sender.SendData(messageModel);
            Thread.Sleep(2 * 1000);

            mockStorage.Verify(m => m.Create(message), Times.Once);
        }
    }
}
