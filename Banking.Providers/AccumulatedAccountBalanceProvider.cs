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

public interface IAccumulatedAccountBalanceProvider
{
    Task<IList<AccountBalanceDto>> GetAccountBalances();
}
    
public class AccumulatedAccountBalanceProvider : IAccumulatedAccountBalanceProvider
{
    private readonly IHubDbRepository _dbRepository;

    public AccumulatedAccountBalanceProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<AccountBalanceDto>> GetAccountBalances()
    {
        return await GetAccountBalances(new AccountQuery());
    }

    public async Task<IList<AccountBalanceDto>> GetAccountBalances(AccountQuery accountQuery)
    {
        return await _dbRepository.GetAsync<AccumulatedAccountBalance, AccountBalanceDto>(GetQueryable(accountQuery));
    }

    private static Queryable<AccumulatedAccountBalance> GetQueryable(AccountQuery accountQuery)
    {
        if (accountQuery.Id != null || accountQuery.AccountId != null)
        {
            accountQuery.IncludeExternalAccounts = true;
            accountQuery.IncludeSharedAccounts = true;
            accountQuery.IncludeDiscontinuedAccounts = true;
        }
        
        return new Queryable<AccumulatedAccountBalance>
        {
            Where = accountBalance =>
                (accountQuery.Id == null || accountQuery.Id == accountBalance.Id) &&
                (accountQuery.AccountNumber == null || accountQuery.AccountNumber == accountBalance.Account.AccountNumber) &&
                (accountQuery.AccountType == null || accountQuery.AccountType == accountBalance.Account.AccountType) &&
                (accountQuery.AccountName == null || accountQuery.AccountName == accountBalance.Account.Name) &&
                (accountQuery.AccountId == null || accountQuery.AccountId == accountBalance.AccountId) &&
                (accountQuery.AccountIds == null || accountQuery.AccountIds.Any(accountId => accountBalance.AccountId == accountId)) &&
                (accountQuery.BankId == null || accountQuery.BankId == 0 || accountQuery.BankId == accountBalance.Account.BankId) &&
                (accountQuery.BalanceFromDate == null || accountBalance.BalanceDate >= accountQuery.BalanceFromDate.Value) &&
                (accountQuery.BalanceToDate == null || accountBalance.BalanceDate <= accountQuery.BalanceToDate.Value) &&
                (accountQuery.IncludeExternalAccounts || accountBalance.Account.Name != accountBalance.Account.AccountNumber) &&
                (accountQuery.IncludeSharedAccounts || !accountBalance.Account.SharedAccount) &&
                (accountQuery.DiscontinuedDate == null || accountQuery.IncludeDiscontinuedAccounts || accountBalance.Account.DiscontinuedDate == null || accountBalance.Account.DiscontinuedDate > accountQuery.DiscontinuedDate),
            Includes = new List<Expression<Func<AccumulatedAccountBalance, object>>>
            {
                accountBalance => accountBalance.Account,
                accountBalance => accountBalance.Account.Bank
            },
            OrderByDescending = accountBalance => accountBalance.BalanceDate,
            ThenOrderByDescending = accountBalance => accountBalance.CreatedDate,
            Take = accountQuery.Take,
            Skip = accountQuery.Skip
        };
    }
}