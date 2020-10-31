using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using Common;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class ServiceBMessagesStorage : DataContextBase<MessageModel>
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

        public void Create(Message messageModel)
        {
            Messages.Add(messageModel);
            SaveChanges();
        }

        public ServiceBMessagesStorage(string connectionString) : base(connectionString)
        {
        }
    }
}
