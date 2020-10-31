using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class ServiceBMessagesStorage : DataContextBase<ServiceBMessagesStorage.Message>
    {
        public class Message
        {
            [Key]
            public int MessageNumber { get; set; }
            public DateTime? SendTime { get; set; }
            public string Text { get; set; }
            public int Hash { get; set; }
            public DateTime? TimeOfReceipt { get; set; }
        }
        private DbSet<Message> Messages { get; set; }

        public override void Create(Message data)
        {
            Messages.Add(data);
            SaveChanges();
        }

        public ServiceBMessagesStorage(string connectionString) : base(connectionString)
        {
        }
    }
}
