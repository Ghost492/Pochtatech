using System;
using Common;
using Microsoft.EntityFrameworkCore;

namespace DataContext
{
    public class DataContextBase<T> : DbContext, IDataStorage<T> where T : class
    {
        private readonly string _connectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public DataContextBase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ApplicationException("Connection string not found.");
            }
            _connectionString = connectionString;
        }

        public virtual T Get(object key)
        {
            throw new NotImplementedException();
        }

        public virtual void Create(T data)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(T data)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(T data)
        {
            throw new NotImplementedException();
        }
    }
}
