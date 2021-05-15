using Hub.HostedServices.Schedule;
using Hub.HostedServices.Schedule.Commands;
using Hub.ServiceBus;
using Hub.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sbanken.Data;
using Sbanken.HostedServices.ScheduledHost.Commands;

namespace Sbanken.HostedServices.ScheduledHost
{
    public class DependencyRegistrationFactory : DependencyRegistrationFactory<SbankenDbContext>
    {
        protected override void AddDomainDependencies(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.TryAddSingleton<IMessageSender, MessageSender>();
            serviceCollection.AddSingleton<IScheduledCommand, QueueUpdateSbankenAccountsCommand>();
            serviceCollection.AddSingleton<IScheduledCommand, QueueUpdateSbankenTransactionsCommand>();
        }
    }
}