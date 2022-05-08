using Hub.Shared.Storage.Repository;
using Hub.Shared.Web.BlazorServer;
using Microsoft.Extensions.DependencyInjection;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.Providers;
using Banking.Services;
using Hub.Shared.Storage.ServiceBus;
using MudBlazor.Services;

var builder = BlazorServerBuilder.CreateWebApplicationBuilder(args);

builder.Services.AddDatabase<BankingDbContext>(builder.Configuration, "SQL_DB_BANKING", "Banking.Data");
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<ITransactionProvider, TransactionProvider>();
builder.Services.AddSingleton<IPreferenceProvider, PreferenceProvider>();
builder.Services.AddSingleton<IPreferenceService, PreferenceService>();
builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAccountProvider, AccountProvider>();
builder.Services.AddTransient<IMessageSender, MessageSender>();
builder.Services.AddTransient<IRecurringTransactionProvider, RecurringTransactionProvider>();
builder.Services.AddTransient<IRecurringTransactionService, RecurringTransactionService>();
            
builder.Services.AddMudServices();
builder.Services.AddAutoMapper(c =>
{
    c.AddEntityMappingProfiles();
});

var app = builder.BuildApp();

app.Run();