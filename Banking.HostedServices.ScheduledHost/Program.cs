using Hub.Shared.HostedServices.Schedule;
using Microsoft.Extensions.Hosting;
using Banking.Data;
using Banking.HostedServices.ScheduledHost.Commands;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.DependencyInjection;

ScheduledHostBuilder
    .CreateHostBuilder<BankingDbContext>(args, "SQL_DB_BANKING")
    .ConfigureServices(serviceCollection =>
    {
        serviceCollection.AddSingleton<IScheduledCommand, QueueUpdateScheduledTransactionsCommand>();
        serviceCollection.AddSingleton<IScheduledCommand, QueueUpdateAccountBalancesForNewMonthCommand>();
        serviceCollection.AddSingleton<IMessageSender, MessageSender>();
    })
    .Build()
    .Run();
