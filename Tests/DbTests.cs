using System;
using Common;
using DataContext;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Tests
{
    class DbTests
    {
        private ServiceAMessagesStorage _messagesDataContext { get; set; }
        [SetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            _messagesDataContext = new ServiceAMessagesStorage(configuration.GetValue<string>("dbConnectionString"));
        }

        [Test]
        public void GenerateMessages()
        {
            for (var i = 1; i < 100000; i++)
            {
                _messagesDataContext.Create(GenerateMessage(i));
            }

        }

        private Message GenerateMessage(int i)
        {
            return new Message()
            {
                MessageNumber = i,
                Text = $"random text {Guid.NewGuid()}",
                Hash = Guid.NewGuid().GetHashCode()
            };
        }
    }
}
