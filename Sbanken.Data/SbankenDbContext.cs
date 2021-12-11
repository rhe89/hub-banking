using Hub.Shared.Storage.Repository;
using Microsoft.EntityFrameworkCore;
using Sbanken.Data.Entities;

namespace Sbanken.Data
{
    public class SbankenDbContext : HubDbContext
    {
        public SbankenDbContext(DbContextOptions<SbankenDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Account> ()
                .ToTable(schema: "dbo", name: "Account");
            
            builder.Entity<AccountBalance> ()
                .ToTable(schema: "dbo", name: "AccountBalance");

            builder.Entity<Transaction>()
                .ToTable(schema: "dbo", name: "Transaction");

        }
    }
}