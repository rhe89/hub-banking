using Hub.Shared.HostedServices.Schedule;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Banking.Data;

namespace Banking.HostedServices.ScheduledHost;

public class DependencyRegistrationFactory : DependencyRegistrationFactory<BankingDbContext>
{
    protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.TryAddSingleton<IMessageSender, MessageSender>();
    }
}