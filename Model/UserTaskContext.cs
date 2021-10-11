using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace api_producer.Model
{
    public class UserTaskContext : DbContext
    {
        public UserTaskContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Task> UserTask { get; set; }
    }
}