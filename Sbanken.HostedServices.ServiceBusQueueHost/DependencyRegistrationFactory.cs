using AutoMapper;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Hub.Shared.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.Data;
using Sbanken.Data.AutoMapper;
using Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers;
using Sbanken.HostedServices.ServiceBusQueueHost.Commands;
using Sbanken.HostedServices.ServiceBusQueueHost.QueueListenerServices;
using Sbanken.Integration;

namespace Sbanken.HostedServices.ServiceBusQueueHost
{
    public class DependencyRegistrationFactory : DependencyRegistrationFactory<SbankenDbContext>
    {
        protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddHubHttpClient<ISbankenConnector, SbankenConnector>();
            serviceCollection.AddTransient<IMessageSender, MessageSender>();
            serviceCollection.AddTransient<IUpdateSbankenAccountsCommandHandler, UpdateSbankenAccountsCommandHandler>();
            serviceCollection.AddTransient<IUpdateSbankenTransactionsCommandHandler, UpdateSbankenTransactionsCommandHandler>();
            serviceCollection.AddTransient<IUpdateSbankenAccountBalanceHistoryCommandHandler, UpdateSbankenAccountBalanceHistoryCommandHandler>();
            
            serviceCollection.AddAutoMapper(c =>
            {
                c.AddSbankenProfiles();
            });
        }

        protected override void AddQueueListenerServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddTransient<UpdateSbankenTransactionsCommand>();
            serviceCollection.AddTransient<UpdateSbankenAccountsCommand>();
            serviceCollection.AddTransient<UpdateSbankenAccountBalanceHistoryCommand>();
            
            serviceCollection.AddHostedService<UpdateSbankenAccountsQueueListener>();
            serviceCollection.AddHostedService<UpdateSbankenTransactionsQueueListener>();
            serviceCollection.AddHostedService<UpdateSbankenAccountsBalanceHistoryQueueListener>();
        }
    }
}