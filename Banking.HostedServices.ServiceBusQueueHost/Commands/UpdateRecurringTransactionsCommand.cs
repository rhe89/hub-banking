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
        var todaysDate = DateTime.Now.Date;
        
        if (DateTime.Compare(recurringTransaction.NextTransactionDate.Date, todaysDate) > 0)
        {
            _logger.LogInformation("Transaction {Description} is not due until {Date}", recurringTransaction.Description, recurringTransaction.NextTransactionDate.ToString("dd.MM.yyyy"));
            return;
        }

        var transactionCreated = await _transactionService.AddTransaction(new TransactionDto
        {
            AccountId = recurringTransaction.AccountId,
            TransactionDate = recurringTransaction.NextTransactionDate.Date,
            Description = recurringTransaction.Description,
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
            Occurrence.Daily => recurringTransaction.NextTransactionDate.Date.AddDays(1),
            Occurrence.Weekly => recurringTransaction.NextTransactionDate.Date.AddDays(7),
            Occurrence.BiWeekly => recurringTransaction.NextTransactionDate.Date.AddDays(14),
            Occurrence.Monthly => recurringTransaction.NextTransactionDate.Date.AddMonths(1),
            Occurrence.BiMonthly => recurringTransaction.NextTransactionDate.Date.AddMonths(2),
            Occurrence.Annually => recurringTransaction.NextTransactionDate.Date.AddYears(1),
            Occurrence.BiAnnually => recurringTransaction.NextTransactionDate.Date.AddYears(2),
            Occurrence.Quarterly => recurringTransaction.NextTransactionDate.Date.AddMonths(3),
            Occurrence.Semiannually => recurringTransaction.NextTransactionDate.Date.AddMonths(6),
            _ => throw new ArgumentOutOfRangeException(nameof(recurringTransaction.Occurrence))
        };

        await _dbRepository.UpdateAsync<RecurringTransaction, RecurringTransactionDto>(recurringTransaction);
        
        _logger.LogInformation("Created transaction {Description} with amount {Amount}. Next transaction is due on {NexTransactionDate}", recurringTransaction.Description, recurringTransaction.Amount, recurringTransaction.NextTransactionDate.ToString("dd.MM.yyyy"));
    }

    public override string Trigger => QueueNames.UpdateRecurringTransactions;
}