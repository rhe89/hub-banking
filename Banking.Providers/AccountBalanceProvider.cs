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

public interface IAccountBalanceProvider
{
    Task<IList<AccountBalanceDto>> GetAccountBalances(AccountBalanceSearchParameters accountBalanceSearchParameters);
}
    
public class AccountBalanceProvider : IAccountBalanceProvider
{
    private readonly IHubDbRepository _dbRepository;

    public AccountBalanceProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }
        
    public async Task<IList<AccountBalanceDto>> GetAccountBalances(AccountBalanceSearchParameters accountBalanceSearchParameters)
    {
        Expression<Func<AccountBalance, bool>> predicate = accountBalance =>
            (accountBalanceSearchParameters.AccountNames == null ||
             accountBalanceSearchParameters.AccountNames.Any(accountName =>
                 accountBalance.Account.Name.Contains(accountName))) &&
            (accountBalanceSearchParameters.AccountIds == null ||
             accountBalanceSearchParameters.AccountIds.Any(accountId => accountBalance.AccountId == accountId)) &&
            (accountBalanceSearchParameters.Banks == null ||
             accountBalanceSearchParameters.Banks.Any(bank =>
                 accountBalance.Account.Bank.Contains(bank))) &&
            (accountBalanceSearchParameters.FromDate == null ||
             accountBalance.CreatedDate >= accountBalanceSearchParameters.FromDate.Value) &&
            (accountBalanceSearchParameters.ToDate == null ||
             accountBalance.CreatedDate <= accountBalanceSearchParameters.ToDate.Value);
        
        var accountBalances = await _dbRepository
            .WhereAsync<AccountBalance, AccountBalanceDto>(predicate, queryable => queryable.Include(x => x.Account));

        return accountBalances;
    }
}