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

public interface IAccountBalanceProvider
{
    Task<IList<AccountBalanceDto>> Get();
    Task<IList<AccountBalanceDto>> Get(AccountQuery query);
}
    
public class AccountBalanceProvider : IAccountBalanceProvider
{
    private readonly IHubDbRepository _dbRepository;

    public AccountBalanceProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<AccountBalanceDto>> Get()
    {
        return await Get(new AccountQuery());
    }

    public async Task<IList<AccountBalanceDto>> Get(AccountQuery query)
    {
        return await _dbRepository.GetAsync<AccountBalance, AccountBalanceDto>(GetQueryable(query));
    }
    
    private static Queryable<AccountBalance> GetQueryable(AccountQuery query)
    {
        if (query.Id != null || query.AccountId != null)
        {
            query.IncludeExternalAccounts = true;
            query.IncludeSharedAccounts = true;
            query.IncludeDiscontinuedAccounts = true;
        }
        
        return new Queryable<AccountBalance>
        {
            Where = entity =>
                (query.Id == null || query.Id == entity.Id) &&
                (query.AccountNumber == null || query.AccountNumber == entity.Account.AccountNumber) &&
                (query.AccountType == null || query.AccountType == entity.Account.AccountType) &&
                (query.AccountName == null || query.AccountName == entity.Account.Name) &&
                (query.AccountId == null || query.AccountId == entity.AccountId) &&
                (query.AccountIds == null || query.AccountIds.Any(accountId => entity.AccountId == accountId)) &&
                (query.BankId == null || query.BankId == 0 || query.BankId == entity.Account.BankId) &&
                (query.BalanceFromDate == null || entity.BalanceDate.Date >= query.BalanceFromDate.Value.Date) &&
                (query.BalanceToDate == null || entity.BalanceDate.Date <= query.BalanceToDate.Value.Date) &&
                (query.IncludeExternalAccounts || entity.Account.Name != entity.Account.AccountNumber) &&
                (query.IncludeSharedAccounts || !entity.Account.SharedAccount) &&
                (query.DiscontinuedDate == null || query.IncludeDiscontinuedAccounts || entity.Account.DiscontinuedDate == null || entity.Account.DiscontinuedDate >= query.DiscontinuedDate),
            Includes = new List<Expression<Func<AccountBalance, object>>>
            {
                entity => entity.Account,
                entity => entity.Account.Bank
            },
            OrderByDescending = entity => entity.BalanceDate,
            ThenOrderByDescending = entity => entity.CreatedDate,
            Take = query.Take,
            Skip = query.Skip
        };
    }
}