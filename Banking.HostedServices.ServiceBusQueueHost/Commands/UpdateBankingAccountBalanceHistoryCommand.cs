using System.Threading;
using System.Threading.Tasks;
using Banking.HostedServices.ServiceBusQueueHost.CommandHandlers;
using Hub.Shared.HostedServices.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Banking.Shared.Constants;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class UpdateBankingAccountBalanceHistoryCommand : ServiceBusQueueCommand, ICommandWithConsumers
{
    private readonly IUpdateBankingAccountBalanceHistoryCommandHandler _updateBankingAccountBalanceHistoryCommandHandler;
    private readonly IMessageSender _messageSender;

    public UpdateBankingAccountBalanceHistoryCommand(IUpdateBankingAccountBalanceHistoryCommandHandler updateBankingAccountBalanceHistoryCommandHandler,
        IMessageSender messageSender)
    {
        _updateBankingAccountBalanceHistoryCommandHandler = updateBankingAccountBalanceHistoryCommandHandler;
        _messageSender = messageSender;
    }
        
    public override async Task Execute(CancellationToken cancellationToken)
    {
        await _updateBankingAccountBalanceHistoryCommandHandler.UpdateAccountBalance();
    }

    public async Task NotifyConsumers()
    {
        await _messageSender.AddToQueue(QueueNames.BankingAccountBalanceHistoryUpdated);
    }

    public override string Trigger => QueueNames.UpdateBankingAccountBalances;
}