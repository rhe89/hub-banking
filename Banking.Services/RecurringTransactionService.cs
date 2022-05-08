using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Services;

public interface IRecurringTransactionService
{
    Task AddRecurringTransaction(RecurringTransactionDto recurringTransaction);
    Task UpdateRecurringTransaction(RecurringTransactionDto recurringTransaction);
    Task DeleteRecurringTransaction(RecurringTransactionDto recurringTransaction);
}

public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly IHubDbRepository _dbRepository;

    public RecurringTransactionService(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task AddRecurringTransaction(RecurringTransactionDto recurringTransaction)
    {
        await _dbRepository.AddAsync<RecurringTransaction, RecurringTransactionDto>(recurringTransaction);
    }
    
    public async Task UpdateRecurringTransaction(RecurringTransactionDto recurringTransaction)
    {
        var recurringTransactionInDb =
            await _dbRepository.SingleAsync<RecurringTransaction, RecurringTransactionDto>(r => r.Id == recurringTransaction.Id);

        recurringTransactionInDb.AccountId = recurringTransaction.AccountId;
        recurringTransactionInDb.Description = recurringTransaction.Description;
        recurringTransactionInDb.Occurrence = recurringTransaction.Occurrence;
        recurringTransactionInDb.NextTransactionDate = recurringTransaction.NextTransactionDate;
        recurringTransactionInDb.Amount = recurringTransaction.Amount;

        await _dbRepository.UpdateAsync<RecurringTransaction, RecurringTransactionDto>(recurringTransactionInDb);
    }

    public async Task DeleteRecurringTransaction(RecurringTransactionDto recurringTransaction)
    {
        await _dbRepository.RemoveAsync<RecurringTransaction, RecurringTransactionDto>(recurringTransaction);
    }
}