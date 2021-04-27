using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyProject_KPiEP
{
    public class DataContext : DbContext
    {
        public DbSet<NotesEntity> Notes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("DataSource = myDb.db");
        }
    }
}
