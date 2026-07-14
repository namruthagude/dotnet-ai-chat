using dotnet_ai_chat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_ai_chat.Data
{
    public  class ChatDbContext : DbContext
    {
        public DbSet<Conversation> conversations => Set<Conversation>();
        public DbSet<Message> messages => Set<Message>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           optionsBuilder.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "chat.db")}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conversation>().
                HasMany(c => c.Messages)
                .WithOne(m => m.Conversation)
                .HasForeignKey(c => c.ConversationId);
        }
    }
}
