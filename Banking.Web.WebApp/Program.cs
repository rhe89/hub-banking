using Hub.Shared.Storage.Repository;
using Hub.Shared.Web.BlazorServer;
using Microsoft.Extensions.DependencyInjection;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.Providers;
using Banking.Services;

var builder = BlazorServerBuilder.CreateWebApplicationBuilder(args);

builder.Services.AddDatabase<BankingDbContext>(builder.Configuration, "SQL_DB_BANKING", "Banking.Data");
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<ITransactionProvider, TransactionProvider>();
builder.Services.AddTransient<IPreferenceProvider, PreferenceProvider>();
builder.Services.AddTransient<IPreferenceService, PreferenceService>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAccountProvider, AccountProvider>();
            
builder.Services.AddAutoMapper(c =>
{
    c.AddEntityMappingProfiles();
});

var app = builder.BuildApp();

app.Run();