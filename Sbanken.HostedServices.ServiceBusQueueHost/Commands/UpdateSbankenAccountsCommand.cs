using System.Threading;
using System.Threading.Tasks;
using Hub.HostedServices.Commands.Core;
using Hub.HostedServices.ServiceBusQueue.Commands;
using Hub.ServiceBus.Core;
using Sbanken.Core.Constants;
using Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers;

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
            await _updateSbankenAccountsCommandHandler.FetchAndUpdateCurrentAccountBalances();
        }

        public async Task NotifyConsumers()
        {
            await _messageSender.AddToQueue(QueueNames.SbankenAccountsUpdated);
        }

        public override string QueueName => QueueNames.UpdateSbankenAccounts;
    }
}