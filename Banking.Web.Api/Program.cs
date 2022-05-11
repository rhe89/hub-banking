using Hub.Shared.Web.Api;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApiBuilder.CreateWebApplicationBuilder<BankingDbContext>(args, "SQL_DB_BANKING");

builder.Services.TryAddTransient<IAccountProvider, AccountProvider>();
builder.Services.TryAddTransient<ITransactionProvider, TransactionProvider>();
builder.Services.TryAddTransient<IAccountBalanceProvider, AccountBalanceProvider>();

builder.Services.AddAutoMapper(c => { c.AddEntityMappingProfiles(); });

builder
    .BuildApp()
    .Run();
