using System;
using Common;
using Microsoft.Extensions.Logging;
using NATS.Client;

namespace Sender
{
    public class Sender<T> : IDisposable

    {
        private readonly string _subjectString;
        private readonly ILogger _logger;
        private readonly IEncodedConnection _connection;

        public Sender(string subjectString, ILogger logger)
        {
            _subjectString = subjectString;
            _logger = logger;
            _connection = new ConnectionFactory().CreateEncodedConnection();
            _connection.OnSerialize = JsonSerializer<T>.Serialize;
            _connection.Opts.AllowReconnect = true;
        }

        public bool SendData(T data)
        {
            try
            {
                Console.WriteLine("Send Message");
                _connection.Publish(_subjectString, data);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }

            return true;
        }
        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}