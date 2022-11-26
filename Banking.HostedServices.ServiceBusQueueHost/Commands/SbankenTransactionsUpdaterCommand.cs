using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Integration;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.HostedServices.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.Repository.Core;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class SbankenTransactionsUpdaterCommand : ServiceBusQueueCommand, ICommandWithConsumers
{
    private readonly IHubDbRepository _dbRepository;
    private readonly ISbankenConnector _sbankenConnector;
    private readonly IBankProvider _bankProvider;
    private readonly IAccountProvider _accountProvider;
    private readonly ITransactionProvider _transactionProvider;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<SbankenTransactionsUpdaterCommand> _logger;

    public SbankenTransactionsUpdaterCommand(
        ISbankenConnector sbankenConnector, 
        IBankProvider bankProvider,
        IAccountProvider accountProvider,
        ITransactionProvider transactionProvider,
        IMessageSender messageSender,
        ILogger<SbankenTransactionsUpdaterCommand> logger,
        IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
        _logger = logger;
        _bankProvider = bankProvider;
        _accountProvider = accountProvider;
        _transactionProvider = transactionProvider;
        _sbankenConnector = sbankenConnector;
        _messageSender = messageSender;
    }
    
    public override async Task Execute(CancellationToken cancellationToken)
    {
        var sbankenBank = (await _bankProvider.GetBanks(new BankQuery { Name = "Sbanken" })).Single();

        var transactionsInDb = await _transactionProvider.GetTransactions(new TransactionQuery
        {
            BankId = sbankenBank.Id,
            IncludeExcludedTransactions = true,
            IncludeTransactionsFromSharedAccounts = true
        });

        var accounts = await _accountProvider.GetAccounts(new AccountQuery
        {
            BankId = sbankenBank.Id,
            IncludeDiscontinuedAccounts = true,
            IncludeExternalAccounts = true,
            IncludeSharedAccounts = true
        });

        if (!accounts.Any())
        {
            return;
        }

        var startDate = new DateTime(2009, 04, 03);
        var endDate = startDate.AddDays(30);

        while (endDate <= DateTime.Now)
        {
            await AddTransactions(startDate, endDate, accounts, transactionsInDb);

            startDate = endDate.AddDays(1);
            endDate = startDate.AddDays(30);
        }
    }

    private async Task AddTransactions(DateTime startDate, DateTime endDate, IList<AccountDto> accounts, IList<TransactionDto> transactionsInDb)
    {
        _logger.LogInformation("Getting transactions from {Start} to {End}", startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd:MM:yyyy"));
        
        var transactionsFromSbanken = await _sbankenConnector.GetTransactions(startDate, endDate);

        transactionsFromSbanken = transactionsFromSbanken.Where(x => x.AccountingDate.Date >= startDate).ToList();

        _logger.LogInformation("Found {Count} transactions", transactionsFromSbanken.Count);

        var transactionsAdded = 0;

        foreach (var transactionFromSbanken in transactionsFromSbanken)
        {
            var accountId = accounts.FirstOrDefault(x => x.Name == transactionFromSbanken.AccountName)?.Id;

            if (accountId == null)
            {
                _logger.LogWarning("Found no account id for account with name {Account}", transactionFromSbanken.AccountName);
                continue;
            }

            if (transactionsInDb.Any(x => x.TransactionId == transactionFromSbanken.TransactionId))
            {
                continue;
            }

            var transaction = new TransactionDto
            {
                Amount = transactionFromSbanken.Amount,
                Description = transactionFromSbanken.Text,
                TransactionType = transactionFromSbanken.TransactionTypeCode,
                TransactionDate = transactionFromSbanken.AccountingDate,
                AccountId = accountId.Value,
                TransactionId = transactionFromSbanken.TransactionId
            };
            
            transactionsInDb.Add(transaction);

            _dbRepository.QueueAdd<Transaction, TransactionDto>(transaction);

            transactionsAdded++;
        }

        await _dbRepository.ExecuteQueueAsync();

        _logger.LogInformation("Added {Count} transactions to DB", transactionsAdded);
    }

    public async Task NotifyConsumers()
    {
        await _messageSender.AddToQueue(QueueNames.BankingTransactionsUpdated);
    }

    public override string Trigger => QueueNames.UpdateSbankenTransactions;
}