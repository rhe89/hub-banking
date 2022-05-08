using System;
using System.Threading;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Banking.Services;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.SearchParameters;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.Repository.Core;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class UpdateRecurringTransactionsCommand : ServiceBusQueueCommand
{
    private readonly IRecurringTransactionProvider _recurringTransactionProvider;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<UpdateRecurringTransactionsCommand> _logger;
    private readonly IHubDbRepository _dbRepository;

    public UpdateRecurringTransactionsCommand(
        IRecurringTransactionProvider recurringTransactionProvider,
        ITransactionService transactionService,
        ILogger<UpdateRecurringTransactionsCommand> logger,
        IHubDbRepository dbRepository)
    {
        _recurringTransactionProvider = recurringTransactionProvider;
        _transactionService = transactionService;
        _logger = logger;
        _dbRepository = dbRepository;
    }
        
    public override async Task Execute(CancellationToken cancellationToken)
    {
        var recurringTransactions = await _recurringTransactionProvider.GetRecurringTransactions(new RecurringTransactionSearchParameters());

        foreach (var recurringTransaction in recurringTransactions)
        {
            await UpdateRecurringTransaction(recurringTransaction);
        }
    }

    private async Task UpdateRecurringTransaction(RecurringTransactionDto recurringTransaction)
    {
        var today = DateTime.Now.Date;
        
        if (recurringTransaction.NextTransactionDate < today)
        {
            return;
        }

        var transactionCreated = await _transactionService.AddTransaction(new TransactionDto
        {
            AccountId = recurringTransaction.AccountId,
            TransactionDate = recurringTransaction.NextTransactionDate,
            Amount = recurringTransaction.Amount,
            TransactionId = $"{recurringTransaction.AccountId}-{recurringTransaction.NextTransactionDate}-{recurringTransaction.Description}-{recurringTransaction.Amount}"
        });

        if (!transactionCreated)
        {
            _logger.LogError("Transaction with text {Text} and due date {DueDate} could not be created", recurringTransaction.Description, recurringTransaction.NextTransactionDate);
            return;
        }

        recurringTransaction.LatestTransactionCreated = recurringTransaction.NextTransactionDate;
        recurringTransaction.NextTransactionDate = recurringTransaction.Occurrence switch
        {
            Occurrence.Daily => today.AddDays(1),
            Occurrence.Weekly => today.AddDays(7),
            Occurrence.BiWeekly => today.AddDays(14),
            Occurrence.Monthly => today.AddMonths(1),
            Occurrence.BiMonthly => today.AddMonths(2),
            Occurrence.Annually => today.AddYears(1),
            Occurrence.BiAnnually => today.AddYears(2),
            Occurrence.Quarterly => today.AddMonths(3),
            Occurrence.Semiannually => today.AddMonths(6),
            _ => throw new ArgumentOutOfRangeException(nameof(recurringTransaction.Occurrence))
        };

        await _dbRepository.UpdateAsync<RecurringTransaction, RecurringTransactionDto>(recurringTransaction);
    }

    public override string Trigger => QueueNames.UpdateRecurringTransactions;
}