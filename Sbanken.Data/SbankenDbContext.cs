using Hub.Storage.Repository.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Sbanken.Core.Entities;

namespace Sbanken.Data
{
    public class SbankenDbContext : HostedServiceDbContext
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