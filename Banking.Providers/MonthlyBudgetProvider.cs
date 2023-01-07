using System.Collections.Generic;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IMonthlyBudgetProvider
{
    Task<IList<MonthlyBudgetDto>> Get(MonthlyBudgetQuery query);
}

public class MonthlyBudgetProvider : IMonthlyBudgetProvider
{
    private readonly ICacheableHubDbRepository _dbRepository;

    public MonthlyBudgetProvider(ICacheableHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }
    
    public async Task<IList<MonthlyBudgetDto>> Get(MonthlyBudgetQuery query)
    {
        return await _dbRepository.GetAsync<MonthlyBudget, MonthlyBudgetDto>(GetQueryable(query));
    }
    
    private static Queryable<MonthlyBudget> GetQueryable(MonthlyBudgetQuery query)
    {
        return new Queryable<MonthlyBudget>(query)
        {
            Where = entity =>
                (query.Id == null || query.Id == entity.Id) &&
                (query.Month == null || query.Month.Value.Month == entity.Month.Month && query.Month.Value.Year == entity.Month.Year),
            OrderBy = entity => entity.Month
        };
    }
}