using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;
using Banking.Providers;
using Banking.Services;

namespace Banking.HostedServices.ServiceBusQueueHost;

public class DependencyRegistrationFactory : DependencyRegistrationFactory<BankingDbContext>
{
    protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddAutoMapper(c =>
        {
            c.AddEntityMappingProfiles();
        });
    }

    protected override void AddQueueListenerServices(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddTransient<UpdateRecurringTransactionsCommand>();
        serviceCollection.AddSingleton<IRecurringTransactionProvider, RecurringTransactionProvider>();
        serviceCollection.AddSingleton<ITransactionService, TransactionService>();
        serviceCollection.AddSingleton<IAccountService, AccountService>();
        serviceCollection.AddSingleton<IMessageSender, MessageSender>();
        serviceCollection.AddHostedService<UpdateRecurringTransactionsQueueListener>();
    }
}