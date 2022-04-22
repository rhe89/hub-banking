using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Microsoft.EntityFrameworkCore;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.SearchParameters;

namespace Banking.Providers;

public interface ITransactionProvider
{
    Task<IList<int>> GetTransactionYears(long accountId);
    Task<IList<int>> GetTransactionMonths(long accountId, int year);
    Task<IList<TransactionDto>> GetTransactions(TransactionSearchParameters transactionSearchParameters);
}
    
public class TransactionProvider : ITransactionProvider
{
    private readonly IHubDbRepository _dbRepository;

    public TransactionProvider(IHubDbRepository dbRepository) 
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<TransactionDto>> GetTransactions(TransactionSearchParameters transactionSearchParameters)
    {
        Expression<Func<Transaction, bool>> predicate = transaction =>
            (transactionSearchParameters.TransactionId == null ||
             transactionSearchParameters.TransactionId == transaction.Id) &&
            (transactionSearchParameters.AccountIds == null ||
             transactionSearchParameters.AccountIds.Any(accountId => accountId == transaction.AccountId)) &&
            (transactionSearchParameters.AccountNames == null ||
             transactionSearchParameters.AccountNames.Any(accountName => accountName == transaction.Account.Name)) &&
            (transactionSearchParameters.AccountTypes == null ||
             transactionSearchParameters.AccountTypes.Any(accountType =>
                 accountType == transaction.Account.AccountType)) &&
            (transactionSearchParameters.FromDate == null ||
             transaction.TransactionDate >= transactionSearchParameters.FromDate) &&
            (transactionSearchParameters.ToDate == null ||
             transaction.TransactionDate <= transactionSearchParameters.ToDate) &&
            (transactionSearchParameters.Months == null ||
             transactionSearchParameters.Months.Any(month => month == transaction.TransactionDate.Month)) &&
            (transactionSearchParameters.Years == null ||
             transactionSearchParameters.Years.Any(year => year == transaction.TransactionDate.Year)) &&
            (string.IsNullOrEmpty(transactionSearchParameters.Description) || transaction.Text
                .Contains(transactionSearchParameters.Description));

        var transactions = await _dbRepository
            .WhereAsync<Transaction, TransactionDto>(predicate,
                source => source.Include(x => x.Account));

        if (transactionSearchParameters.Take != null)
        {
            transactions = transactions
                .OrderByDescending(x => x.UpdatedDate)
                .Take(transactionSearchParameters.Take.Value)
                .ToList();
        }
    
        return transactions
            .OrderByDescending(x => x.TransactionDate)
            .ToList();        
    }
    
    public async Task<IList<int>> GetTransactionYears(long accountId)
    {
        var transactions = await _dbRepository
            .WhereAsync<Transaction, TransactionDto>(transaction => transaction.AccountId == accountId);

        return transactions
            .Select(x => x.TransactionDate.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToList();        
    }
    
    public async Task<IList<int>> GetTransactionMonths (long accountId, int year)
    {
        var transactions = await _dbRepository
            .WhereAsync<Transaction, TransactionDto>(transaction => transaction.AccountId == accountId && transaction.TransactionDate.Year == year);

        return transactions
            .Select(x => x.TransactionDate.Month)
            .Distinct()
            .OrderBy(month => month)
            .ToList();        
    }
}