using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface ITransactionCategoryProvider
{
    Task<IList<TransactionCategoryDto>> GetTransactionCategories();
    Task<IList<TransactionCategoryDto>> Get(TransactionCategoryQuery query);
    Task<IList<TransactionSubCategoryDto>> GetTransactionSubCategories();
    Task<IList<TransactionSubCategoryDto>> Get(TransactionSubCategoryQuery query);
}

public class TransactionCategoryProvider : ITransactionCategoryProvider
{
    private readonly ICacheableHubDbRepository _dbRepository;

    public TransactionCategoryProvider(ICacheableHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }

    public async Task<IList<TransactionCategoryDto>> GetTransactionCategories()
    {
        return await Get(new TransactionCategoryQuery());
    }

    public async Task<IList<TransactionCategoryDto>> Get(TransactionCategoryQuery query)
    {
        return await _dbRepository.GetAsync<TransactionCategory, TransactionCategoryDto>(GetQueryable(query));
    }

    public async Task<IList<TransactionSubCategoryDto>> GetTransactionSubCategories()
    {
        return await Get(new TransactionSubCategoryQuery());
    }

    public async Task<IList<TransactionSubCategoryDto>> Get(TransactionSubCategoryQuery query)
    {
        return await _dbRepository.GetAsync<TransactionSubCategory, TransactionSubCategoryDto>(GetQueryable(query));
    }
    
    private static Queryable<TransactionCategory> GetQueryable(TransactionCategoryQuery query)
    {
        return new Queryable<TransactionCategory>
        {
            Where = entity =>
                (query.Id == null || query.Id == entity.Id) &&
                (query.Name == null || query.Name == entity.Name),
            Include = entity => entity.TransactionSubCategories,
            OrderBy = x => x.Name,
            Take = query.Take,
            Skip = query.Skip
        };
    }

    private static Queryable<TransactionSubCategory> GetQueryable(TransactionSubCategoryQuery query)
    {
        return new Queryable<TransactionSubCategory>
        {
            Where = entity =>
                (query.Id == null || query.Id == entity.Id) &&
                (query.Name == null || query.Name == entity.Name) &&
                (query.TransactionCategoryId == null || query.TransactionCategoryId == entity.TransactionCategoryId) &&
                (query.TransactionCategoryIds == null || query.TransactionCategoryIds.Any(transactionCategoryId => transactionCategoryId == entity.TransactionCategoryId)) &&
                (query.TransactionCategoryName == null || query.TransactionCategoryName == entity.TransactionCategory.Name),
            OrderByDescending = entity => entity.UpdatedDate,
            Take = query.Take,
            Skip = query.Skip
        };
    }
}