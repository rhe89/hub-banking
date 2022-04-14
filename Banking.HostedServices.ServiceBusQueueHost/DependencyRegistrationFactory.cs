using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Hub.Shared.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Banking.Data;
using Banking.Data.AutoMapper;
using Banking.HostedServices.ServiceBusQueueHost.CommandHandlers;
using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;
using Banking.Integration;

namespace Banking.HostedServices.ServiceBusQueueHost;

public class DependencyRegistrationFactory : DependencyRegistrationFactory<BankingDbContext>
{
    protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddHubHttpClient<ISbankenConnector, SbankenConnector>();
        serviceCollection.AddTransient<IMessageSender, MessageSender>();
        serviceCollection.AddTransient<IUpdateBankingAccountsCommandHandler, UpdateBankingAccountsCommandHandler>();
        serviceCollection.AddTransient<IUpdateBankingTransactionsCommandHandler, UpdateBankingTransactionsCommandHandler>();
        serviceCollection.AddTransient<IUpdateBankingAccountBalanceHistoryCommandHandler, UpdateBankingAccountBalanceHistoryCommandHandler>();
            
        serviceCollection.AddAutoMapper(c =>
        {
            c.AddEntityMappingProfiles();
        });
    }

    protected override void AddQueueListenerServices(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddTransient<UpdateBankingTransactionsCommand>();
        serviceCollection.AddTransient<UpdateBankingAccountsCommand>();
        serviceCollection.AddTransient<UpdateBankingAccountBalanceHistoryCommand>();
            
        serviceCollection.AddHostedService<UpdateBankingAccountsQueueListener>();
        serviceCollection.AddHostedService<UpdateBankingTransactionsQueueListener>();
        serviceCollection.AddHostedService<UpdateBankingAccountsBalanceHistoryQueueListener>();
    }
}