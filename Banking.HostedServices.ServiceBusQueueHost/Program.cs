using System;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Microsoft.Extensions.Hosting;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;
using Banking.Integration;
using Banking.Providers;
using Banking.Services;
using Hub.Shared.Settings;
using Hub.Shared.Storage.ServiceBus;
using Hub.Shared.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BulderBankTransactionsImporter = Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices.BulderBankTransactionsImporter;
using SbankenTransactionsImporter = Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices.SbankenTransactionsImporter;

ServiceBusHostBuilder
    .CreateHostBuilder<BankingDbContext>(args, "SQL_DB_BANKING")
    .ConfigureServices((context, serviceCollection) =>
    {
        serviceCollection.AddSingleton<IBulderBankTransactionsImporter, Banking.Integration.BulderBankTransactionsImporter>();
        serviceCollection.AddSingleton<ISbankenTransactionsImporter, Banking.Integration.SbankenTransactionsImporter>();
        serviceCollection.AddSingleton<IScheduledTransactionService, ScheduledTransactionService>();
        serviceCollection.AddSingleton<IScheduledTransactionProvider, ScheduledTransactionProvider>();
        serviceCollection.AddSingleton<ITransactionService, TransactionService>();
        serviceCollection.AddSingleton<ITransactionProvider, TransactionProvider>();
        serviceCollection.AddSingleton<IAccountService, AccountService>();
        serviceCollection.AddSingleton<IAccountProvider, AccountProvider>();
        serviceCollection.AddSingleton<IAccountBalanceProvider, AccountBalanceProvider>();
        serviceCollection.AddSingleton<IAccumulatedAccountBalanceProvider, AccumulatedAccountBalanceProvider>();
        serviceCollection.AddSingleton<IBankService, BankService>();
        serviceCollection.AddSingleton<IBankProvider, BankProvider>();
        serviceCollection.AddSingleton<ITransactionCategoryProvider, TransactionCategoryProvider>();
        serviceCollection.AddSingleton<ITransactionCategoryService, TransactionCategoryService>();
        serviceCollection.AddSingleton<IMessageSender, MessageSender>();
        serviceCollection.AddSingleton<ISettingProvider, SettingProvider>();
        serviceCollection.AddSingleton<SpreadsheetApiConnector, SpreadsheetApiConnector>();
        serviceCollection.AddSingleton<IIncomeImporter, IncomeImporter>();
        serviceCollection.AddSingleton<IDebtImporter, DebtImporter>();
        serviceCollection.AddSingleton<IMonthlyBudgetProvider, MonthlyBudgetProvider>();
        serviceCollection.AddSingleton<IMonthlyBudgetService, MonthlyBudgetService>();
        serviceCollection.AddSingleton<IPreferenceProvider, PreferenceProvider>();
        serviceCollection.AddHubHttpClient<ISpreadsheetApiConnector, SpreadsheetApiConnector>(
            client => client.BaseAddress = new Uri(context.Configuration.GetValue<string>(ApiEndpoints.Spreadsheet)));

        serviceCollection.AddSingleton<BulderBankTransactionsImporterCommand>();
        serviceCollection.AddHostedService<BulderBankTransactionsImporter>();
        
        serviceCollection.AddSingleton<TransactionCategorizerCommand>();
        serviceCollection.AddHostedService<TransactionCategorizer>();
        
        serviceCollection.AddSingleton<CreditCardPaymentCalculatorCommand>();
        serviceCollection.AddHostedService<CreditCardPaymentCalculator>();
        
        serviceCollection.AddSingleton<AccountBalancesForNewMonthUpdaterCommand>();
        serviceCollection.AddHostedService<AccountBalancesForNewMonthUpdater>();
        
        serviceCollection.AddSingleton<SbankenTransactionsImporterCommand>();
        serviceCollection.AddHostedService<SbankenTransactionsImporter>();
        
        serviceCollection.AddSingleton<MonthlyBudgetCalculatorCommand>();
        serviceCollection.AddHostedService<MonthlyBudgetCalculator>();
        
        serviceCollection.AddSingleton<ScheduledTransactionsUpdaterCommand>();
        serviceCollection.AddHostedService<ScheduledTransactionsUpdater>();
        
        serviceCollection.AddAutoMapper(c =>
        {
            c.AddEntityMappingProfiles();
        });
    })
    .Build()
    .Run();