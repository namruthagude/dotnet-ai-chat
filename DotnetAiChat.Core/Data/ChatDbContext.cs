using DotnetAiChat.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetAiChat.Core.Data
{
    public  class ChatDbContext : DbContext
    {
        public DbSet<Conversation> conversations => Set<Conversation>();
        public DbSet<Message> messages => Set<Message>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DotNetAIChat",
        "chat.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            optionsBuilder.UseSqlite(
                $"Data Source={dbPath}",
                b => b.MigrationsAssembly("dotnet-ai-chat"));
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
