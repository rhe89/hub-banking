using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.SearchParameters;

namespace Banking.Providers;

public interface IAccountProvider
{
    Task<IList<AccountDto>> GetAccounts(AccountSearchParameters accountSearchParameters);
}
    
public class AccountProvider : IAccountProvider
{
    private readonly IHubDbRepository _dbRepository;

    public AccountProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<AccountDto>> GetAccounts(AccountSearchParameters accountSearchParameters)
    {
        Expression<Func<Account, bool>> predicate = account => 
            (accountSearchParameters.AccountNames == null || accountSearchParameters.AccountNames.Any(accountName => account.Name.Contains(accountName))) && 
            (accountSearchParameters.AccountIds == null || accountSearchParameters.AccountIds.Any(accountId => account.Id == accountId)) && 
            (accountSearchParameters.Banks == null || accountSearchParameters.Banks.Any(bank => account.Bank.Contains(bank)));

        var accounts = await _dbRepository
            .WhereAsync<Account, AccountDto>(predicate);
        
        if (accountSearchParameters.Take != null)
        {
            return accounts
                .OrderByDescending(x => x.UpdatedDate)
                .Take(accountSearchParameters.Take.Value)
                .ToList();
        }

        return accounts
            .OrderByDescending(x => x.UpdatedDate)
            .ToList();
    }
}