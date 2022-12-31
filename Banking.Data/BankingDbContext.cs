using Banking.Data.Entities;
using Hub.Shared.Settings;
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
            .ToTable(schema: "dbo", name: "Account")
                .HasMany(c => c.AccountBalance)
                .WithOne(e => e.Account)
                .IsRequired();
            
        builder.Entity<AccountBalance> ()
            .ToTable(schema: "dbo", name: "AccountBalance")
            .HasOne(e => e.Account)
            .WithMany(x => x.AccountBalance)
            .IsRequired();

        builder.Entity<AccumulatedAccountBalance>()
            .ToView(schema: "dbo", name: "AccumulatedAccountBalance");

        builder.Entity<Transaction>()
            .ToTable(schema: "dbo", name: "Transaction");
        
        builder.Entity<Setting>()
            .ToTable(schema: "dbo", name: "Setting");
        
        builder.Entity<ScheduledTransaction>()
            .ToTable(schema: "dbo", name: "RecurringTransaction");
        
        builder.Entity<CsvImport>()
            .ToTable(schema: "dbo", name: "CsvImport");
        
        builder.Entity<Bank>()
            .ToTable(schema: "dbo", name: "Bank");
        
        builder.Entity<TransactionCategory>()
            .ToTable(schema: "dbo", name: "TransactionCategory");
        
        builder.Entity<TransactionSubCategory>()
            .ToTable(schema: "dbo", name: "TransactionSubCategory");
        
        builder.Entity<MonthlyBudget>()
            .ToTable(schema: "dbo", name: "MonthlyBudget");
        
        builder.Entity<Preference>()
            .ToTable(schema: "dbo", name: "Preference");
    }
}