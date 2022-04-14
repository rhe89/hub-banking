﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Banking;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;

namespace Banking.Providers;

public interface IAccountProvider
{
    Task<IList<AccountDto>> GetAccounts(string accountName, string accountType);
}
    
public class AccountProvider : IAccountProvider
{
    private readonly IHubDbRepository _dbRepository;

    public AccountProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }
        
    public async Task<IList<AccountDto>> GetAccounts(string accountName, string accountType)
    {
        Expression<Func<Account, bool>> predicate = account => 
            (string.IsNullOrEmpty(accountName) || account.Name.ToLower().Contains(accountName.ToLower())) && 
            (string.IsNullOrEmpty(accountType) || account.AccountType.ToLower().Contains(accountType.ToLower()));

        var accounts = await _dbRepository
            .WhereAsync<Account, AccountDto>(predicate);

        return accounts;
    }
}