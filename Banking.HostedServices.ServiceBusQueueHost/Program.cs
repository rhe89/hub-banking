using Hub.Shared.HostedServices.ServiceBusQueue;
using Microsoft.Extensions.Hosting;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;
using Banking.Integration;
using Banking.Providers;
using Banking.Services;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

ServiceBusHostBuilder
    .CreateHostBuilder<BankingDbContext>(args, "SQL_DB_BANKING")
    .ConfigureServices(serviceCollection =>
    {
        serviceCollection.AddSingleton<ICsvTransactionsImporter, CsvTransactionsImporter>();
        serviceCollection.AddSingleton<IScheduledTransactionService, ScheduledTransactionService>();
        serviceCollection.AddSingleton<IScheduledTransactionProvider, ScheduledTransactionProvider>();
        serviceCollection.AddSingleton<ITransactionService, TransactionService>();
        serviceCollection.AddSingleton<ITransactionProvider, TransactionProvider>();
        serviceCollection.AddSingleton<IAccountService, AccountService>();
        serviceCollection.AddSingleton<IAccountProvider, AccountProvider>();
        serviceCollection.AddSingleton<IAccountBalanceProvider, AccountBalanceProvider>();
        serviceCollection.AddSingleton<IBankService, BankService>();
        serviceCollection.AddSingleton<IBankProvider, BankProvider>();
        serviceCollection.AddSingleton<ITransactionCategoryProvider, TransactionCategoryProvider>();
        serviceCollection.AddSingleton<ITransactionCategoryService, TransactionCategoryService>();
        serviceCollection.AddSingleton<IMessageSender, MessageSender>();

        serviceCollection.AddSingleton<ImportTransactionsCsvCommand>();
        serviceCollection.AddHostedService<ImportTransactionsCsvQueueListener>();
        
        serviceCollection.AddSingleton<CategorizeTransactionsCommand>();
        serviceCollection.AddHostedService<CategorizeTransactionsQueueListener>();
        
        serviceCollection.AddAutoMapper(c =>
        {
            c.AddEntityMappingProfiles();
        });
    })
    .Build()
    .Run();