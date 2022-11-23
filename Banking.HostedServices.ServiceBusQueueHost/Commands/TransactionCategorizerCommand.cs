using System.Threading;
using System.Threading.Tasks;
using Banking.Services;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class TransactionCategorizerCommand : ServiceBusQueueCommand
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionCategorizerCommand> _logger;

    public TransactionCategorizerCommand(
        ITransactionService transactionService,
        ILogger<TransactionCategorizerCommand> logger)
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