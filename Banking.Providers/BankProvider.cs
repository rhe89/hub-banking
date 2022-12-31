using System.Collections.Generic;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IBankProvider
{
    Task<IList<BankDto>> Get();
    Task<IList<BankDto>> Get(BankQuery query);
}
    
public class BankProvider : IBankProvider
{
    private readonly ICacheableHubDbRepository _dbRepository;

    public BankProvider(ICacheableHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<BankDto>> Get()
    {
        return await Get(new BankQuery());
    }

    public async Task<IList<BankDto>> Get(BankQuery query)
    {
        return await _dbRepository.GetAsync<Bank, BankDto>(GetQueryable(query));
    }
    
    private static Queryable<Bank> GetQueryable(BankQuery query)
    {
        return new Queryable<Bank>
        {
            Where = entity =>
                (query.Id == null || query.Id == 0 || query.Id == entity.Id) &&
                (query.Name == null || query.Name == entity.Name) &&
                (query.AccountNumberPrefix == null || query.AccountNumberPrefix == entity.AccountNumberPrefix),
            OrderBy = entity => entity.Name,
            Take = query.Take,
            Skip = query.Skip
        };
    }
}