using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IAccountProvider
{
    Task<IList<AccountDto>> GetAccounts();
    Task<IList<AccountDto>> GetAccounts(AccountQuery accountQuery);
    Task<IList<AccountDto>> GetAccountBalancesForMonth(AccountQuery accountQuery, int? year, int? month);
    Task<IList<AccountDto>> GetAccountBalancesForLastMonth(AccountQuery accountQuery, int? year, int? month);
}

public class AccountProvider : IAccountProvider
{
    private readonly IAccountBalanceProvider _accountBalanceProvider;
    private readonly IHubDbRepository _dbRepository;

    public AccountProvider(
        IAccountBalanceProvider accountBalanceProvider,
        IHubDbRepository dbRepository)
    {
        _accountBalanceProvider = accountBalanceProvider;
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
            var accountBalances = await _accountBalanceProvider.GetAccountBalances(accountQuery);

            foreach (var account in accounts)
            {
                var accountBalance = accountBalances.Where(x => x.AccountId == account.Id).MaxBy(x => x.CreatedDate);
                
                var latestAccountBalance = accountBalances.Where(x => x.AccountId == account.Id).MaxBy(x => x.CreatedDate) ??
                                           await _accountBalanceProvider.GetAccumulatedAccountBalance(account, accountQuery);

                account.NoBalanceForGivenPeriod = latestAccountBalance == null && accountBalance == null;
                account.Balance = latestAccountBalance?.Balance ?? 0;
                account.BalanceDate = latestAccountBalance?.BalanceDate ?? DateTime.Now;
            }
        }

        if (accountQuery.Take != null)
        {
            return accounts
                .OrderByDescending(x => x.UpdatedDate)
                .Take(accountQuery.Take.Value)
                .ToList();
        }

        return accounts
            .OrderByDescending(x => x.UpdatedDate)
            .ToList();
    }
    
    public async Task<IList<AccountDto>> GetAccountBalancesForLastMonth(AccountQuery accountQuery, int? year, int? month)
    {
        year ??= DateTime.Now.Year;
        month ??= DateTime.Now.Month;

        var date = new DateTime(year.Value, month.Value, 1);

        var lastMonth = date.AddMonths(-1);
        
        accountQuery.BalanceToDate = DateTimeUtils.LastDayOfMonth(lastMonth.Year, lastMonth.Month);

        return await GetAccounts(accountQuery);
    }
    
    public async Task<IList<AccountDto>> GetAccountBalancesForMonth(AccountQuery accountQuery, int? year, int? month)
    {
        month ??= DateTime.Now.Month;
        year ??= DateTime.Now.Year;
        
        accountQuery.BalanceToDate = DateTimeUtils.LastDayOfMonth(year, month);

        return await GetAccounts(accountQuery);
    }

    private static Queryable<Account> GetQueryable(AccountQuery accountQuery)
    {
        return new Queryable<Account>
        {
            Query = accountQuery,
            Where = account =>
                (accountQuery.Id == null || accountQuery.Id == 0 || accountQuery.Id == account.Id) &&
                (accountQuery.AccountNumber == null || accountQuery.AccountNumber == account.AccountNumber) &&
                (accountQuery.AccountType == null || accountQuery.AccountType == account.AccountType) &&
                (accountQuery.AccountName == null || accountQuery.AccountName == account.Name) &&
                (accountQuery.AccountIds == null || accountQuery.AccountIds.Any(accountId => accountId == account.Id)) &&
                (accountQuery.BankId == null || accountQuery.BankId == 0 || accountQuery.BankId == account.BankId) &&
                (accountQuery.IncludeExternalAccounts || account.Name != account.AccountNumber) && 
                (accountQuery.IncludeSharedAccounts || !account.SharedAccount) &&
                (accountQuery.DiscontinuedDate == null || accountQuery.IncludeDiscontinuedAccounts || account.DiscontinuedDate == null || account.DiscontinuedDate > accountQuery.DiscontinuedDate),
            OrderBy = account => account.UpdatedDate
        };
    }
}