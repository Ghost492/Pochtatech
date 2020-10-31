using System;
using Common;
using DataContext;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Tests
{
    class DbTests
    {
        private string connectionString = "Data source=172.18.115.36;Database=ServiceA;User ID=sa;Password=qwe123!@#;";

        [Test]
        public void ConnectionToDbTest()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
            }

        }
        [Test]
        public void GetMessageTest()
        {
            var storage = new ServiceAMessagesStorage(connectionString);
            var message = storage.GetNext();
            Assert.IsNotNull(message);
            Assert.IsNull(message.SendTime);
        }
    }
}
