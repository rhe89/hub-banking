using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Banking.Data;
using Banking.Data.AutoMapper;

namespace Banking.HostedServices.ServiceBusQueueHost;

public class DependencyRegistrationFactory : DependencyRegistrationFactory<BankingDbContext>
{
    protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddTransient<IMessageSender, MessageSender>();
            
        serviceCollection.AddAutoMapper(c =>
        {
            c.AddEntityMappingProfiles();
        });
    }

    protected override void AddQueueListenerServices(IServiceCollection serviceCollection, IConfiguration configuration)
    {
    }
}