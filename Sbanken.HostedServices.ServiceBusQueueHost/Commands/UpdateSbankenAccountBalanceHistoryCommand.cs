using System.Threading;
using System.Threading.Tasks;
using Hub.HostedServices.Commands.Core;
using Hub.HostedServices.ServiceBusQueue.Commands;
using Hub.ServiceBus.Core;
using Sbanken.Core.Constants;
using Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers;

namespace Sbanken.HostedServices.ServiceBusQueueHost.Commands
{
    public class UpdateSbankenAccountBalanceHistoryCommand : ServiceBusQueueCommand, ICommandWithConsumers
    {
        private readonly IUpdateSbankenAccountBalanceHistoryCommandHandler _updateSbankenAccountBalanceHistoryCommandHandler;
        private readonly IMessageSender _messageSender;

        public UpdateSbankenAccountBalanceHistoryCommand(IUpdateSbankenAccountBalanceHistoryCommandHandler updateSbankenAccountBalanceHistoryCommandHandler,
            IMessageSender messageSender)
        {
            _updateSbankenAccountBalanceHistoryCommandHandler = updateSbankenAccountBalanceHistoryCommandHandler;
            _messageSender = messageSender;
        }
        
        public override async Task Execute(CancellationToken cancellationToken)
        {
            await _updateSbankenAccountBalanceHistoryCommandHandler.UpdateAccountBalance();
        }

        public async Task NotifyConsumers()
        {
            await _messageSender.AddToQueue(QueueNames.SbankenAccountBalanceHistoryUpdated);
        }

        public override string QueueName => QueueNames.SbankenAccountsUpdated;
    }
}