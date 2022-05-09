using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.Services;

public interface ITransactionService
{
    Task<bool> AddTransaction(TransactionDto transaction);
    Task<bool> UpdateTransaction(TransactionDto transaction);
    Task DeleteTransaction(TransactionDto transaction);
}
    
public class TransactionService : ITransactionService
{
    private readonly IAccountService _accountService;
    private readonly IHubDbRepository _hubDbRepository;
    private readonly IMessageSender _messageSender;

    public TransactionService(
        IAccountService accountService,
        IHubDbRepository hubDbRepository, 
        IMessageSender messageSender)
    {
        _accountService = accountService;
        _hubDbRepository = hubDbRepository;
        _messageSender = messageSender;
    }

    public async Task<bool> AddTransaction(TransactionDto transaction)
    {
        await _hubDbRepository.AddAsync<Transaction, TransactionDto>(transaction);

        await _messageSender.AddToQueue(QueueNames.BankingTransactionsUpdated);

        await _accountService.UpdateAccountBalance(transaction.AccountId, transaction.Amount);
        
        return true;
    }
        
    public async Task<bool> UpdateTransaction(TransactionDto transaction)
    {
        var transactionInDb = await _hubDbRepository.SingleAsync<Transaction, TransactionDto>(t => t.Id == transaction.Id);
        
        transactionInDb.AccountId = transaction.AccountId;
        transactionInDb.Description = transaction.Description;
        transactionInDb.TransactionDate = transaction.TransactionDate;
        transactionInDb.Amount = transaction.Amount;

        await _hubDbRepository.UpdateAsync<Transaction, TransactionDto>(transactionInDb);

        await _messageSender.AddToQueue(QueueNames.BankingTransactionsUpdated);

        return true;
    }

    public async Task DeleteTransaction(TransactionDto transaction)
    {
        await _hubDbRepository.RemoveAsync<Transaction, TransactionDto>(transaction);
    }
}