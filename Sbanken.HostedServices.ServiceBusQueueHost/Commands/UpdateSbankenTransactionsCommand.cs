using System.Threading;
using System.Threading.Tasks;
using Hub.HostedServices.Commands.Core;
using Hub.HostedServices.ServiceBusQueue.Commands;
using Hub.ServiceBus.Core;
using Sbanken.Core.Constants;
using Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers;

namespace Sbanken.HostedServices.ServiceBusQueueHost.Commands
{
    public class UpdateSbankenTransactionsCommand : ServiceBusQueueCommand, ICommandWithConsumers
    {
        private readonly IUpdateSbankenTransactionsCommandHandler _updateSbankenTransactionsCommandHandler;
        private readonly IMessageSender _messageSender;

        public UpdateSbankenTransactionsCommand(IUpdateSbankenTransactionsCommandHandler updateSbankenTransactionsCommandHandler,
            IMessageSender messageSender)
        {
            _updateSbankenTransactionsCommandHandler = updateSbankenTransactionsCommandHandler;
            _messageSender = messageSender;
        }

        public override async Task Execute(CancellationToken cancellationToken)
        {
            await _updateSbankenTransactionsCommandHandler.UpdateTransactions();
        }

        public async Task NotifyConsumers()
        {
            await _messageSender.AddToQueue(QueueNames.SbankenTransactionsUpdated);
        }

        public override string QueueName => QueueNames.UpdateSbankenTransactions;
    }
}