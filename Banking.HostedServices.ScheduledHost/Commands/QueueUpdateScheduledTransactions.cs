using System.Threading;
using System.Threading.Tasks;
using Hub.Shared.HostedServices.Commands;
using Hub.Shared.HostedServices.Schedule;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.HostedServices.ScheduledHost.Commands;

public class QueueUpdateScheduledTransactions : ScheduledCommand
{
    private readonly IMessageSender _messageSender;

    public QueueUpdateScheduledTransactions(
        ICommandConfigurationProvider commandConfigurationProvider,
        ICommandConfigurationFactory commandConfigurationFactory,
        IMessageSender messageSender) : base(commandConfigurationProvider, commandConfigurationFactory)
    {
        _messageSender = messageSender;
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        await _messageSender.AddToQueue(QueueNames.UpdateScheduledTransactions);
    }
}