using System.Threading;
using System.Threading.Tasks;
using Banking.HostedServices.ServiceBusQueueHost.CommandHandlers;
using Hub.Shared.HostedServices.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class UpdateBankingTransactionsCommand : ServiceBusQueueCommand, ICommandWithConsumers
{
    private readonly IUpdateBankingTransactionsCommandHandler _updateBankingTransactionsCommandHandler;
    private readonly IMessageSender _messageSender;

    public UpdateBankingTransactionsCommand(IUpdateBankingTransactionsCommandHandler updateBankingTransactionsCommandHandler,
        IMessageSender messageSender)
    {
        _updateBankingTransactionsCommandHandler = updateBankingTransactionsCommandHandler;
        _messageSender = messageSender;
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        await _updateBankingTransactionsCommandHandler.UpdateTransactions();
    }

    public async Task NotifyConsumers()
    {
        await _messageSender.AddToQueue(QueueNames.BankingTransactionsUpdated);
    }

    public override string Trigger => QueueNames.UpdateBankingTransactions;
}