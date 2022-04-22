using System.Threading;
using System.Threading.Tasks;
using Banking.HostedServices.ServiceBusQueueHost.CommandHandlers;
using Hub.Shared.HostedServices.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class UpdateBankingAccountsCommand : ServiceBusQueueCommand, ICommandWithConsumers
{
    private readonly IUpdateBankingAccountsCommandHandler _updateBankingAccountsCommandHandler;
    private readonly IMessageSender _messageSender;

    public UpdateBankingAccountsCommand(IUpdateBankingAccountsCommandHandler updateBankingAccountsCommandHandler,
        IMessageSender messageSender)
    {
        _updateBankingAccountsCommandHandler = updateBankingAccountsCommandHandler;
        _messageSender = messageSender;
    }
        
    public override async Task Execute(CancellationToken cancellationToken)
    {
        await _updateBankingAccountsCommandHandler.UpdateAccounts();
    }

    public async Task NotifyConsumers()
    {
        await _messageSender.AddToQueue(QueueNames.BankingAccountsUpdated);
        await _messageSender.AddToQueue(QueueNames.UpdateBankingTransactions);
        await _messageSender.AddToQueue(QueueNames.UpdateBankingAccountBalances);
    }

    public override string Trigger => QueueNames.UpdateBankingAccounts;
}