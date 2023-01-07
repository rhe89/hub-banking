using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IAccountProvider
{
    Task<IList<AccountDto>> Get();
    Task<IList<AccountDto>> Get(AccountQuery query);
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

    public Task<IList<AccountDto>> Get()
    {
        return Get(new AccountQuery());
    }

    public async Task<IList<AccountDto>> Get(AccountQuery query)
    {
        var accounts = await _dbRepository.GetAsync<Account, AccountDto>(GetQueryable(query));

        var accountBalanceQuery = query;
        
        if (accountBalanceQuery.BalanceFromDate != null ||
            accountBalanceQuery.BalanceToDate != null)
        {
            accountBalanceQuery.AccountId = accountBalanceQuery.Id;
            accountBalanceQuery.Id = null;

            var accountBalances = await _accountBalanceProvider.Get(accountBalanceQuery);

            var accumulatedAccountBalances = await _accumulatedAccountBalanceProvider.Get(query);

            foreach (var account in accounts)
            {
                var accountBalance = accountBalances.Where(x => x.AccountId == account.Id).MaxBy(x => x.BalanceDate);

                if (accountBalance == null)
                {
                    accountBalance = accumulatedAccountBalances.Where(x => x.AccountId == account.Id).MaxBy(x => x.BalanceDate);
                    account.BalanceIsAccumulated = accountBalance != null;
                }

                account.Balance = accountBalance?.Balance ?? 0;
                account.BalanceDate = accountBalance?.BalanceDate ?? DateTimeUtils.Today;
            }
        }

        if (query.Take != null)
        {
            return accounts
                .Take(query.Take.Value)
                .OrderByDescending(x => x.BalanceDate)
                .ThenByDescending(x => x.UpdatedDate)
                .ToList();
        }

        return accounts
            .OrderByDescending(x => x.BalanceDate)
            .ThenByDescending(x => x.UpdatedDate)
            .ToList();
    }
    
    
    
    private static Queryable<Account> GetQueryable(AccountQuery query)
    {
        if ((query.Id != null && query.Id != 0) ||
            query.AccountNumber != null)
        {
            query.IncludeSharedAccounts = true;
            query.IncludeDiscontinuedAccounts = true;
        }
        
        return new Queryable<Account>(query)
        {
            Where = entity =>
                (query.Id == null || query.Id == 0 || query.Id == entity.Id) &&
                (query.AccountNumber == null || query.AccountNumber == entity.AccountNumber) &&
                (query.AccountType == null || query.AccountType == entity.AccountType) &&
                (query.AccountName == null || query.AccountName == entity.Name) &&
                (query.AccountIds == null || query.AccountIds.Any(accountId => accountId == entity.Id)) &&
                (query.BankId == null || query.BankId == 0 || query.BankId == entity.BankId) &&
                (query.BankName == null || query.BankName == entity.Bank.Name) &&
                (query.IncludeSharedAccounts || !entity.SharedAccount) &&
                (query.IncludeDiscontinuedAccounts || entity.DiscontinuedDate == null || entity.DiscontinuedDate >= query.DiscontinuedDate),
            OrderBy = entity => entity.UpdatedDate,
            Include = entity => entity.Bank,
        };
    }
}