using Hub.Storage.Entities;
using Microsoft.EntityFrameworkCore;
using Sbanken.Data.Entities;

namespace Sbanken.Data
{
    public class SbankenDbContext : DbContext
    {
        public SbankenDbContext(DbContextOptions<SbankenDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.AddSettingEntity();
            builder.AddWorkerLogEntity();
            builder.AddBackgroundTaskConfigurationEntity();
            
            builder.Entity<Account> ()
                .ToTable(schema: "dbo", name: "Account");
            
            builder.Entity<AccountBalance> ()
                .ToTable(schema: "dbo", name: "AccountBalance");

            builder.Entity<Transaction>()
                .ToTable(schema: "dbo", name: "Transaction");

        }
    }
}