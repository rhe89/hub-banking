using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Providers;

public interface ITransactionProvider
{
    Task<IList<TransactionDto>> GetTransactions();
    Task<IList<TransactionDto>> GetTransactions(TransactionQuery transactionQuery);
    Task<IList<int>> GetTransactionYears();
    Task<IList<int>> GetTransactionMonths();
}

public class TransactionProvider : ITransactionProvider
{
    private readonly IHubDbRepository _dbRepository;

    public TransactionProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<TransactionDto>> GetTransactions()
    {
        return await GetTransactions(new TransactionQuery());
    }

    public async Task<IList<TransactionDto>> GetTransactions(TransactionQuery transactionQuery)
    {
        return await _dbRepository.GetAsync<Transaction, TransactionDto>(GetQueryable(transactionQuery));
    }

    public async Task<IList<int>> GetTransactionYears()
    {
        var transactions = await GetTransactions();

        return transactions
            .Select(x => x.TransactionDate.Year)
            .Distinct()
            .OrderBy(year => year)
            .ToList();
    }

    public async Task<IList<int>> GetTransactionMonths()
    {
        var transactions = await GetTransactions();

        return transactions
            .Select(x => x.TransactionDate.Month)
            .Distinct()
            .OrderBy(month => month)
            .ToList();
    }

    private Queryable<Transaction> GetQueryable(TransactionQuery transactionQuery)
    {
        if (transactionQuery.Id != null)
        {
            transactionQuery.IncludeExcludedTransactions = true;
            transactionQuery.IncludeTransactionsFromSharedAccounts = true;
        }
        
        return new Queryable<Transaction>
        {
            Where = transaction =>
                (transactionQuery.Id == null || transactionQuery.Id == transaction.Id) &&
                (transactionQuery.AccountId == null || transactionQuery.AccountId == 0 || transactionQuery.AccountId == transaction.AccountId) &&
                (transactionQuery.AccountIds == null || transactionQuery.AccountIds.Any(accountId => transaction.AccountId == accountId)) &&
                (transactionQuery.AccountType == null || transactionQuery.AccountType == transaction.Account.AccountType) &&
                (transactionQuery.AccountName == null || transactionQuery.AccountName == transaction.Account.Name) &&
                (transactionQuery.IncludeTransactionsFromSharedAccounts == true || !transaction.Account.SharedAccount) &&
                (transactionQuery.IncludeExcludedTransactions == true || !transaction.Exclude) &&
                (transactionQuery.BankId == null || transactionQuery.BankId == 0 || transactionQuery.BankId == transaction.Account.BankId) &&
                (transactionQuery.TransactionSubCategoryId == null || transactionQuery.TransactionSubCategoryId == transaction.TransactionSubCategoryId) &&
                (transactionQuery.TransactionCategoryId == null || transactionQuery.TransactionCategoryId == transaction.TransactionSubCategory.TransactionCategoryId) &&
                (transactionQuery.Source == null || transactionQuery.Source == transaction.Source) &&
                (transactionQuery.FromDate == null || transaction.TransactionDate >= transactionQuery.FromDate) &&
                (transactionQuery.ToDate == null || transaction.TransactionDate <= transactionQuery.ToDate) &&
                (transactionQuery.Month == null || transactionQuery.Month == transaction.TransactionDate.Month) &&
                (transactionQuery.Year == null || transactionQuery.Year == transaction.TransactionDate.Year) &&
                (string.IsNullOrEmpty(transactionQuery.Description) || transaction.Text.Contains(transactionQuery.Description)),

            Includes = new Expression<Func<Transaction, object>>[]
            {
                transaction => transaction.Account,
                transaction => transaction.Account.Bank,
                transaction => transaction.TransactionSubCategory,
                transaction => transaction.TransactionSubCategory.TransactionCategory
            },
            OrderByDescending = x => x.TransactionDate,
            Take = transactionQuery.Take,
            Skip = transactionQuery.Skip
        };
    }
}