using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Services;

public interface ITransactionService
{
    Task<bool> AddTransaction(TransactionDto transaction);
    Task<bool> UpdateTransaction(TransactionDto transaction);
}
    
public class TransactionService : ITransactionService
{
    private readonly IHubDbRepository _hubDbRepository;

    public TransactionService(IHubDbRepository hubDbRepository)
    {
        _hubDbRepository = hubDbRepository;
    }

    public async Task<bool> AddTransaction(TransactionDto transaction)
    {
        await _hubDbRepository.AddAsync<Transaction, TransactionDto>(transaction);

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

        return true;
    }
}