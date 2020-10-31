using System;
using System.Configuration;
using System.Linq;
using Common;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class MessagesDataContext : DbContext
    {
        private readonly string _connectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
        private DbSet<Message> Messages { get; set; }

        public MessagesDataContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApplicationException("Connection string not found.");
            }
            _connectionString = connectionString;
        }
        public Message GetNextMessage()
        {
            return Messages
                .OrderBy(x => x.MessageNumber)
                .FirstOrDefault(x => !x.SendTime.HasValue);
        }

        public void Create(Message message)
        {
            Messages.Add(message);
            SaveChanges();
        }

        public void Update(Message message)
        {
            Messages.Update(message);
            SaveChanges();
        }
    }
}
