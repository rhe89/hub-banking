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
    Task<IList<TransactionDto>> Get();
    Task<IList<TransactionDto>> Get(TransactionQuery query);
}

public class TransactionProvider : ITransactionProvider
{
    private readonly IHubDbRepository _dbRepository;

    public TransactionProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<TransactionDto>> Get()
    {
        return await Get(new TransactionQuery());
    }

    public async Task<IList<TransactionDto>> Get(TransactionQuery query)
    {
        return await _dbRepository.GetAsync<Transaction, TransactionDto>(GetQueryable(query));
    }

    private static Queryable<Transaction> GetQueryable(TransactionQuery query)
    {
        if (query.Id != null)
        {
            query.IncludeExcludedTransactions = true;
            query.IncludeTransactionsFromSharedAccounts = true;
        }
        
        return new Queryable<Transaction>(query)
        {
            Where = entity =>
                (query.Id == null || query.Id == entity.Id) &&
                (query.AccountId == null || query.AccountId == 0 || query.AccountId == entity.AccountId) &&
                (query.AccountIds == null || query.AccountIds.Any(accountId => entity.AccountId == accountId)) &&
                (query.AccountType == null || query.AccountType == entity.Account.AccountType) &&
                (query.AccountName == null || query.AccountName == entity.Account.Name) &&
                (query.IncludeTransactionsFromSharedAccounts == true || !entity.Account.SharedAccount) &&
                (query.IncludeExcludedTransactions == true || !entity.Exclude) &&
                (query.BankId == null || query.BankId == 0 || query.BankId == entity.Account.BankId) &&
                (query.TransactionSubCategoryId == null || query.TransactionSubCategoryId == entity.TransactionSubCategoryId) &&
                (query.TransactionCategoryId == null || query.TransactionCategoryId == entity.TransactionSubCategory.TransactionCategoryId) &&
                (query.Source == null || query.Source == entity.Source) &&
                (query.FromDate == null || entity.TransactionDate.Date >= query.FromDate.Value.Date) &&
                (query.ToDate == null || entity.TransactionDate.Date <= query.ToDate.Value.Date) &&
                (string.IsNullOrEmpty(query.Description) || entity.Text.Contains(query.Description)),

            Includes = new Expression<Func<Transaction, object>>[]
            {
                entity => entity.Account,
                entity => entity.Account.Bank,
                entity => entity.TransactionSubCategory,
                entity => entity.TransactionSubCategory.TransactionCategory
            },
            OrderByDescending = entity => entity.TransactionDate
        };
    }
}