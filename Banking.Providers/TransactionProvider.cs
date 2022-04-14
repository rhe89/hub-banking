using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Banking;
using Hub.Shared.Storage.Repository.Core;
using Microsoft.EntityFrameworkCore;
using Banking.Data.Entities;

namespace Banking.Providers;

public interface ITransactionProvider
{
    Task<IList<TransactionDto>> GetTransactions(int? ageInDays, string description, string accountName);
    Task<IList<TransactionDto>> GetTransactions(int month, int year, string accountName);
    Task<IList<int>> GetTransactionYears(string accountName);
    Task<TransactionDto> GetTransaction(long transactionId);
}
    
public class TransactionProvider : ITransactionProvider
{
    private readonly IHubDbRepository _dbRepository;

    public TransactionProvider(IHubDbRepository dbRepository) 
    {
        _dbRepository = dbRepository;
    }
    
    public async Task<IList<TransactionDto>> GetTransactions(int? ageInDays, string description, string accountName)
    {
        Expression<Func<Transaction, bool>> predicate = transaction =>
            (ageInDays == null || transaction.TransactionDate > DateTime.Now.AddDays(-ageInDays.Value)) && 
            (string.IsNullOrEmpty(description) || transaction.Text.ToLower().Contains(description.ToLower())) &&
            (string.IsNullOrEmpty(accountName) || transaction.Account.Name.ToLower().Contains(accountName.ToLower()));

        var transactions = await _dbRepository
            .WhereAsync<Transaction, TransactionDto>(predicate,
                source => source.Include(x => x.Account));

        return transactions
            .OrderByDescending(x => x.TransactionDate)
            .ToList();        
    }
    
    public async Task<IList<int>> GetTransactionYears(string accountName)
    {
        var transactions = await _dbRepository
            .WhereAsync<Transaction, TransactionDto>(transaction => transaction.Account.Name.ToLower().Contains(accountName.ToLower()));

        return transactions
            .Select(x => x.TransactionDate.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToList();        
    }

    public async Task<IList<TransactionDto>> GetTransactions(int month, int year, string accountName)
    {
        Expression<Func<Transaction, bool>> predicate = transaction =>
            (transaction.TransactionDate.Month == month) && 
            (transaction.TransactionDate.Year == year) &&
            (transaction.Account.Name.ToLower().Contains(accountName.ToLower()));

        var transactions = await _dbRepository
            .WhereAsync<Transaction, TransactionDto>(predicate,
                source => source.Include(x => x.Account));

        return transactions
            .OrderByDescending(x => x.TransactionDate)
            .ToList();        
    }

    public async Task<TransactionDto> GetTransaction(long transactionId)
    {
        Expression<Func<Transaction, bool>> predicate = transaction =>
            transaction.Id == transactionId;
            
        return await _dbRepository
            .FirstOrDefaultAsync<Transaction, TransactionDto>(predicate,
                source => source.Include(x => x.Account));
    }
}