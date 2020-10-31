using Common;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Sender;

namespace Tests
{
    class SenderTests
    {
        private Mock<ILogger> _moqLogger;

        [SetUp]
        public void Setup()
        {
            _moqLogger = new Mock<ILogger>(MockBehavior.Strict);
        }

        [Test]
        public void CanSendMessage()
        {
            var message = new MessageModel()
            {
                Hash = 123,
                MessageNumber = 1,
                Text = "text"
            };
            var sender = new DataSender<MessageModel>("testConnection", _moqLogger.Object);
            sender.SendData(message);
        }
    }
}
