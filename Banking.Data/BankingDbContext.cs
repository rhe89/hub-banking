using Banking.Data.Entities;
using Hub.Shared.Storage.Repository;
using Microsoft.EntityFrameworkCore;

namespace Banking.Data;

public class BankingDbContext : HubDbContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
            
        builder.Entity<Account> ()
            .ToTable(schema: "dbo", name: "Account");
            
        builder.Entity<AccountBalance> ()
            .ToTable(schema: "dbo", name: "AccountBalance");

        builder.Entity<Transaction>()
            .ToTable(schema: "dbo", name: "Transaction");
        
        builder.Entity<Preference>()
            .ToTable(schema: "dbo", name: "Preference");

    }
}