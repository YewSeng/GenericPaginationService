using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using System;
using System.Collections.Generic;

namespace PaginationDemo.Models
{
    public class ExternalPatronDbContext : DbContext
    {

        public ExternalPatronDbContext()
        {

        }

        public ExternalPatronDbContext(DbContextOptions<ExternalPatronDbContext> options) : base(options)
        {

        }

        public virtual DbSet<ExternalPatron> ExternalPatrons { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional configuration if needed
        }
    }
}
