using Hub.Shared.Storage.Repository;
using Hub.Shared.Web.BlazorServer;
using Microsoft.Extensions.DependencyInjection;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.Providers;
using Banking.Services;
using Banking.Web.WebApp;
using Banking.Web.WebApp.Services.Table;
using Banking.Web.WebApp.Shared;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.ServiceBus;
using MudBlazor.Services;

var builder = BlazorServerBuilder.CreateWebApplicationBuilder(args);

builder.Services.AddDatabase<BankingDbContext>(builder.Configuration, "SQL_DB_BANKING");
builder.Services.AddTransient<IMessageSender, MessageSender>();
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<ITransactionProvider, TransactionProvider>();

builder.Services.AddTransient<IAccountService, AccountService>();
builder.Services.AddTransient<IAccountProvider, AccountProvider>();
builder.Services.AddTransient<IAccountBalanceProvider, AccountBalanceProvider>();
builder.Services.AddTransient<IAccumulatedAccountBalanceProvider, AccumulatedAccountBalanceProvider>();

builder.Services.AddTransient<IScheduledTransactionProvider, ScheduledTransactionProvider>();
builder.Services.AddTransient<IScheduledTransactionService, ScheduledTransactionService>();

builder.Services.AddTransient<IBankProvider, BankProvider>();
builder.Services.AddTransient<IBankService, BankService>();

builder.Services.AddTransient<ITransactionCategoryProvider, TransactionCategoryProvider>();
builder.Services.AddTransient<ITransactionCategoryService, TransactionCategoryService>();

builder.Services.AddTransient<IMonthlyBudgetProvider, MonthlyBudgetProvider>();

builder.Services.AddTransient<TransactionsTableService>();
builder.Services.AddTransient<ScheduledTransactionsTableService>();
builder.Services.AddTransient<TransactionCategoriesTableService>();
builder.Services.AddTransient<TransactionSubCategoriesTableService>();
builder.Services.AddTransient<AccountsTableService>();
builder.Services.AddTransient<BanksTableService>();
builder.Services.AddTransient<AccountTypesTableService>();

builder.Services.AddTransient<TableService<TransactionQuery>, TransactionsTableService>();
builder.Services.AddTransient<TableService<ScheduledTransactionQuery>, ScheduledTransactionsTableService>();
builder.Services.AddTransient<TableService<TransactionCategoryQuery>, TransactionCategoriesTableService>();
builder.Services.AddTransient<TableService<TransactionSubCategoryQuery>, TransactionSubCategoriesTableService>();
builder.Services.AddTransient<TableService<AccountQuery>, AccountsTableService>();
builder.Services.AddTransient<TableService<BankQuery>, BanksTableService>();
builder.Services.AddTransient<TableService<AccountTypesQuery>, AccountTypesTableService>();

builder.Services.AddSingleton<State>();

builder.Services.AddScoped<UIHelpers>();

builder.Services.AddMudServices();
builder.Services.AddAutoMapper(c =>
{
    c.AddEntityMappingProfiles();
});

builder
    .BuildApp()
    .Run();