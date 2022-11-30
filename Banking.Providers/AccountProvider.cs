using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IAccountProvider
{
    Task<IList<AccountDto>> GetAccounts();
    Task<IList<AccountDto>> GetAccounts(AccountQuery accountQuery);
}

public class AccountProvider : IAccountProvider
{
    private readonly IAccountBalanceProvider _accountBalanceProvider;
    private readonly IAccumulatedAccountBalanceProvider _accumulatedAccountBalanceProvider;
    private readonly ICacheableHubDbRepository _dbRepository;

    public AccountProvider(
        IAccountBalanceProvider accountBalanceProvider,
        IAccumulatedAccountBalanceProvider accumulatedAccountBalanceProvider,
        ICacheableHubDbRepository dbRepository)
    {
        _accountBalanceProvider = accountBalanceProvider;
        _accumulatedAccountBalanceProvider = accumulatedAccountBalanceProvider;
        _dbRepository = dbRepository;
    }

    public Task<IList<AccountDto>> GetAccounts()
    {
        return GetAccounts(new AccountQuery());
    }

    public async Task<IList<AccountDto>> GetAccounts(AccountQuery accountQuery)
    {
        var accounts = await _dbRepository.GetAsync<Account, AccountDto>(GetQueryable(accountQuery));

        if (accountQuery.BalanceFromDate != null ||
            accountQuery.BalanceToDate != null)
        {
            accountQuery.AccountId = accountQuery.Id;
            accountQuery.Id = null;

            var accountBalances = await _accountBalanceProvider.GetAccountBalances(accountQuery);

            var accumulatedAccountBalances = await _accumulatedAccountBalanceProvider.GetAccountBalances();

            foreach (var account in accounts)
            {
                var accountBalance = accountBalances.Where(x => x.AccountId == account.Id).MaxBy(x => x.BalanceDate);

                if (accountBalance == null)
                {
                    accountBalance = accumulatedAccountBalances.Where(x => x.AccountId == account.Id).MaxBy(x => x.BalanceDate);
                    account.BalanceIsAccumulated = accountBalance != null;
                }

                account.NoBalanceForGivenPeriod = accountBalance == null;
                account.Balance = accountBalance?.Balance ?? 0;
                account.BalanceDate = accountBalance?.BalanceDate ?? DateTime.Now;
            }
        }

        if (accountQuery.Take != null)
        {
            return accounts
                .Take(accountQuery.Take.Value)
                .OrderByDescending(x => x.BalanceDate)
                .ThenByDescending(x => x.UpdatedDate)
                .ToList();
        }

        return accounts
            .OrderByDescending(x => x.BalanceDate)
            .ThenByDescending(x => x.UpdatedDate)
            .ToList();
    }
    
    
    
    private static Queryable<Account> GetQueryable(AccountQuery accountQuery)
    {
        if (accountQuery.Id != null && accountQuery.Id != 0)
        {
            accountQuery.IncludeExternalAccounts = true;
            accountQuery.IncludeSharedAccounts = true;
            accountQuery.IncludeDiscontinuedAccounts = true;
        }
        
        return new Queryable<Account>
        {
            Where = account =>
                (accountQuery.Id == null || accountQuery.Id == 0 || accountQuery.Id == account.Id) &&
                (accountQuery.AccountNumber == null || accountQuery.AccountNumber == account.AccountNumber) &&
                (accountQuery.AccountType == null || accountQuery.AccountType == account.AccountType) &&
                (accountQuery.AccountName == null || accountQuery.AccountName == account.Name) &&
                (accountQuery.AccountIds == null || accountQuery.AccountIds.Any(accountId => accountId == account.Id)) &&
                (accountQuery.BankId == null || accountQuery.BankId == 0 || accountQuery.BankId == account.BankId) &&
                (accountQuery.IncludeExternalAccounts || account.Name != account.AccountNumber) && 
                (accountQuery.IncludeSharedAccounts || !account.SharedAccount) &&
                (accountQuery.IncludeDiscontinuedAccounts || account.DiscontinuedDate == null || account.DiscontinuedDate > accountQuery.DiscontinuedDate),
            OrderBy = account => account.UpdatedDate,
            Include = account => account.Bank,
            Skip = accountQuery.Skip
        };
    }
}