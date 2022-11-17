﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IBankProvider
{
    Task<IList<BankDto>> GetBanks();
    Task<IList<BankDto>> GetBanks(BankQuery bankQuery);
}
    
public class BankProvider : IBankProvider
{
    private readonly IHubDbRepository _dbRepository;

    public BankProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<BankDto>> GetBanks()
    {
        return await GetBanks(new BankQuery());
    }

    public async Task<IList<BankDto>> GetBanks(BankQuery bankQuery)
    {
        return await _dbRepository.GetAsync<Bank, BankDto>(GetQueryable(bankQuery));
    }
    
    private static Queryable<Bank> GetQueryable(BankQuery bankQuery)
    {
        return new Queryable<Bank>
        {
            Query = bankQuery,
            Where = bank =>
                (bankQuery.Id == bank.Id) ||
                (bankQuery.Name == null || bankQuery.Name == bank.Name) &&
                (bankQuery.AccountNumberPrefix == null || bankQuery.AccountNumberPrefix == bank.AccountNumberPrefix),
            OrderBy = bank => bank.Name
        };
    }
}