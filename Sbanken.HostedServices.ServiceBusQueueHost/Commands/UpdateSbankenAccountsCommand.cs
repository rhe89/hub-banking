using System.Threading;
using System.Threading.Tasks;
using Hub.Shared.HostedServices.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers;
using Sbanken.Shared.Constants;

namespace Sbanken.HostedServices.ServiceBusQueueHost.Commands
{
    public class UpdateSbankenAccountsCommand : ServiceBusQueueCommand, ICommandWithConsumers
    {
        private readonly IUpdateSbankenAccountsCommandHandler _updateSbankenAccountsCommandHandler;
        private readonly IMessageSender _messageSender;

        public UpdateSbankenAccountsCommand(IUpdateSbankenAccountsCommandHandler updateSbankenAccountsCommandHandler,
            IMessageSender messageSender)
        {
            _updateSbankenAccountsCommandHandler = updateSbankenAccountsCommandHandler;
            _messageSender = messageSender;
        }
        
        public override async Task Execute(CancellationToken cancellationToken)
        {
            await _updateSbankenAccountsCommandHandler.UpdateAccounts();
        }

        public async Task NotifyConsumers()
        {
            await _messageSender.AddToQueue(QueueNames.SbankenAccountsUpdated);
            await _messageSender.AddToQueue(QueueNames.UpdateSbankenTransactions);
            await _messageSender.AddToQueue(QueueNames.UpdateSbankenAccountBalances);
        }

        public override string Trigger => QueueNames.UpdateSbankenAccounts;
    }
}