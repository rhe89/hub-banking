using System.Threading;
using System.Threading.Tasks;
using Banking.Services;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class CategorizeTransactionsCommand : ServiceBusQueueCommand
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<CategorizeTransactionsCommand> _logger;

    public CategorizeTransactionsCommand(
        ITransactionService transactionService,
        ILogger<CategorizeTransactionsCommand> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        var categorizedTransactions = await _transactionService.CategorizeTransactions();
        
        _logger.LogInformation("Categorized {Count} transactions", categorizedTransactions);
    }

    public override string Trigger => QueueNames.CategorizeTransactions;
}