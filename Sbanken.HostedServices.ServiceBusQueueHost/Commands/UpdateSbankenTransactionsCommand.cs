using System.Threading;
using System.Threading.Tasks;
using Hub.Shared.HostedServices.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers;
using Sbanken.Shared.Constants;

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

        public override string Trigger => QueueNames.UpdateSbankenTransactions;
    }
}