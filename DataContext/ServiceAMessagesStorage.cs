using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class ServiceAMessagesStorage : DataContextBase<ServiceAMessagesStorage.Message>
    {
        public class Message
        {
            [Key]
            public int MessageNumber { get; set; }
            public DateTime? SendTime { get; set; }
            public string Text { get; set; }
            public int Hash { get; set; }
        }
        private DbSet<Message> Messages { get; set; }

        public Message GetNext()
        {
            return Messages
                .OrderBy(x => x.MessageNumber)
                .FirstOrDefault(x => !x.SendTime.HasValue);
        }

        public override void Update(Message data)
        {
            Messages.Update(data);
            SaveChanges();
        }

        public ServiceAMessagesStorage(string connectionString) : base(connectionString)
        {
        }
    }
}
