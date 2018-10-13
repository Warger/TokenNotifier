using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TokenNotifier.Models;

namespace TokenNotifier.Data
{
    public class DbCryptoContext : DbContext
    {
        public DbCryptoContext(DbContextOptions options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Notification>().ToTable("Notification");
            modelBuilder.Entity<Wallet>().ToTable("Wallet");
            modelBuilder.Entity<Token>().ToTable("Token");
        }
    }
}
