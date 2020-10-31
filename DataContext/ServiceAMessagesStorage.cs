﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using Common;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class ServiceAMessagesStorage : DataContextBase<MessageModel>
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
     
        public Message GetNextMessage()
        {
            return Messages
                .OrderBy(x => x.MessageNumber)
                .FirstOrDefault(x => !x.SendTime.HasValue);
        }

        public void Update(Message messageModel)
        {
            Messages.Update(messageModel);
            SaveChanges();
        }

        public ServiceAMessagesStorage(string connectionString) : base(connectionString)
        {
        }
    }
}
