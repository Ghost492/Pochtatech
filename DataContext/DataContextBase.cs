using System;
using System.Configuration;
using System.Linq;
using Common;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class DataContextBase<T> : DbContext where T : class
    {
        private readonly string _connectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }
        protected DbSet<T> Messages { get; set; }

        public DataContextBase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApplicationException("Connection string not found.");
            }
            _connectionString = connectionString;
        }

        public void Create(T message)
        {
            Messages.Add(message);
            SaveChanges();
        }

        public void Update(T message)
        {
            Messages.Update(message);
            SaveChanges();
        }
    }
}
