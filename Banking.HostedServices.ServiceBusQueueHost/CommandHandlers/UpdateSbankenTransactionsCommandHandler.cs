using System;
using System.Linq;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Banking;
using Hub.Shared.Storage.Repository.Core;
using Microsoft.Extensions.Logging;
using Banking.Data.Entities;
using Banking.Integration;

namespace Banking.HostedServices.ServiceBusQueueHost.CommandHandlers;

public interface IUpdateBankingTransactionsCommandHandler
{
    Task UpdateTransactions();
}
    
public class UpdateBankingTransactionsCommandHandler : IUpdateBankingTransactionsCommandHandler
{
    private readonly IHubDbRepository _dbRepository;
    private readonly ISbankenConnector _sbankenConnector;
    private readonly ILogger<UpdateBankingTransactionsCommandHandler> _logger;

    public UpdateBankingTransactionsCommandHandler(
        ISbankenConnector sbankenConnector, 
        ILogger<UpdateBankingTransactionsCommandHandler> logger, 
        IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
        _logger = logger;
        _sbankenConnector = sbankenConnector;
    }

    public async Task UpdateTransactions()
    {
        var transactionsInDb = _dbRepository.All<Transaction, TransactionDto>()
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ToList();

        var accounts = _dbRepository.All<Account, AccountDto>()
            .ToList();

        if (!accounts.Any())
        {
            return;
        }

        var startDate = DateTime.Now.AddDays(-30);

        var transactionsFromSbanken = await _sbankenConnector.GetTransactions(startDate, null);

        transactionsFromSbanken = transactionsFromSbanken.Where(x => x.AccountingDate.Date >= startDate).ToList();

        _logger.LogInformation("Found {Count} transactions", transactionsFromSbanken.Count);
            
        var transactionsAdded = 0;

        foreach (var transactionFromSbanken in transactionsFromSbanken)
        {
            var accountId = accounts.FirstOrDefault(x => x.Name == transactionFromSbanken.AccountName)?.Id;

            if (accountId == null)
            {
                    
                continue;
            }
                
            if (transactionsInDb.Any(x => 
                    x.TransactionId == transactionFromSbanken.TransactionId ||
                    x.Description == transactionFromSbanken.Text && 
                    x.TransactionDate == transactionFromSbanken.AccountingDate))
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

            _dbRepository.QueueAdd<Transaction, TransactionDto>(transaction);
                
            transactionsAdded++;
        }
            
        await _dbRepository.ExecuteQueueAsync();
            
        _logger.LogInformation("Added {Count} transactions to DB", transactionsAdded);
    }
}