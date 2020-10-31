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
            var options = ConnectionFactory.GetDefaultOptions();
            options.AllowReconnect = true;
            options.MaxReconnect = Options.ReconnectForever;
            _connection = new ConnectionFactory().CreateEncodedConnection(options);
            _connection.OnSerialize = JsonSerializer<T>.Serialize;
        }

        public bool SendData(T data)
        {
            try
            {
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