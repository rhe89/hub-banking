using Hub.Shared.HostedServices.ServiceBusQueue;
using Microsoft.Extensions.Hosting;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;
using Banking.Providers;
using Banking.Services;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

ServiceBusHostBuilder
    .CreateHostBuilder<BankingDbContext>(args, "SQL_DB_BANKING")
    .ConfigureServices(serviceCollection =>
    {
        serviceCollection.AddTransient<UpdateRecurringTransactionsCommand>();
        serviceCollection.AddSingleton<IRecurringTransactionProvider, RecurringTransactionProvider>();
        serviceCollection.AddSingleton<ITransactionService, TransactionService>();
        serviceCollection.AddSingleton<IAccountService, AccountService>();
        serviceCollection.AddSingleton<IMessageSender, MessageSender>();
        serviceCollection.AddHostedService<UpdateRecurringTransactionsQueueListener>();
        serviceCollection.AddAutoMapper(c =>
        {
            c.AddEntityMappingProfiles();
        });
    })
    .Build()
    .Run();