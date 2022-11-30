using Hub.Shared.Web.Api;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApiBuilder.CreateWebApplicationBuilder<BankingDbContext>(args, "SQL_DB_BANKING");

builder.Services.TryAddSingleton<IAccountProvider, AccountProvider>();
builder.Services.TryAddSingleton<IAccountBalanceProvider, AccountBalanceProvider>();
builder.Services.TryAddSingleton<IAccumulatedAccountBalanceProvider, AccumulatedAccountBalanceProvider>();
builder.Services.TryAddSingleton<ITransactionProvider, TransactionProvider>();
builder.Services.TryAddSingleton<IScheduledTransactionProvider, ScheduledTransactionProvider>();
builder.Services.TryAddSingleton<IBankProvider, BankProvider>();
builder.Services.TryAddSingleton<ITransactionCategoryProvider, TransactionCategoryProvider>();

builder.Services.AddAutoMapper(c => { c.AddEntityMappingProfiles(); });

builder
    .BuildApp()
    .Run();
