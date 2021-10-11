using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace api_producer.Model
{
    public class TaskRecordDbContext : DbContext
    {
        public TaskRecordDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<TaskRecord> UserTask { get; set; }
    }
}