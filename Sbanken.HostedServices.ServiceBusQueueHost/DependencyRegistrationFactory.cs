using AutoMapper;
using Hub.HostedServices.ServiceBusQueue;
using Hub.ServiceBus;
using Hub.ServiceBus.Core;
using Hub.Web.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.Core.Integration;
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
            
            serviceCollection.AddAutoMapper(c =>
            {
                c.AddSbankenProfiles();
            });
        }

        protected override void AddQueueListenerServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddTransient<UpdateSbankenTransactionsCommand>();
            serviceCollection.AddTransient<UpdateSbankenAccountsCommand>();
            
            serviceCollection.AddHostedService<UpdateSbankenAccountsQueueListener>();
            serviceCollection.AddHostedService<UpdateSbankenTransactionsQueueListener>();
        }
    }
}