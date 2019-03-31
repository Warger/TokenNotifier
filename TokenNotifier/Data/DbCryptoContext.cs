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
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<TokenNotifier.Models.Exchange> Exchange { get; set; }
        public DbSet<WatchedToken> WatchedTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Notification>().ToTable("Notification");
            modelBuilder.Entity<Wallet>().ToTable("Wallet");
            modelBuilder.Entity<Token>().ToTable("Token");
            modelBuilder.Entity<Transfer>().ToTable("Transfer");
            modelBuilder.Entity<Exchange>().ToTable("Exchange");
            modelBuilder.Entity<WatchedToken>().ToTable("WatchedToken");
            modelBuilder.Entity<TwitterAccount>().ToTable("TwitterAccount");
        }


        public DbSet<TokenNotifier.Models.TwitterAccount> TwitterAccount { get; set; }
    }
}
