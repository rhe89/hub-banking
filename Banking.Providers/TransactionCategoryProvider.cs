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
    Task<IList<TransactionCategoryDto>> GetTransactionCategories(TransactionCategoryQuery transactionCategoryQuery);
    Task<IList<TransactionSubCategoryDto>> GetTransactionSubCategories();
    Task<IList<TransactionSubCategoryDto>> GetTransactionSubCategories(TransactionSubCategoryQuery transactionSubCategoryQuery);
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
        return await GetTransactionCategories(new TransactionCategoryQuery());
    }

    public async Task<IList<TransactionCategoryDto>> GetTransactionCategories(TransactionCategoryQuery transactionCategoryQuery)
    {
        return await _dbRepository.GetAsync<TransactionCategory, TransactionCategoryDto>(GetQueryable(transactionCategoryQuery));
    }

    public async Task<IList<TransactionSubCategoryDto>> GetTransactionSubCategories()
    {
        return await GetTransactionSubCategories(new TransactionSubCategoryQuery());
    }

    public async Task<IList<TransactionSubCategoryDto>> GetTransactionSubCategories(TransactionSubCategoryQuery transactionSubCategoryQuery)
    {
        return await _dbRepository.GetAsync<TransactionSubCategory, TransactionSubCategoryDto>(GetQueryable(transactionSubCategoryQuery));
    }
    
    private static Queryable<TransactionCategory> GetQueryable(TransactionCategoryQuery transactionCategoryQuery)
    {
        return new Queryable<TransactionCategory>
        {
            Where = transactionCategory =>
                (transactionCategoryQuery.Id == null || transactionCategoryQuery.Id == transactionCategory.Id) &&
                (transactionCategoryQuery.Name == null || transactionCategoryQuery.Name == transactionCategory.Name),
            Include = transactionCategory => transactionCategory.TransactionSubCategories,
            OrderBy = x => x.Name,
            Take = transactionCategoryQuery.Take,
            Skip = transactionCategoryQuery.Skip
        };
    }

    private static Queryable<TransactionSubCategory> GetQueryable(TransactionSubCategoryQuery transactionSubCategoryQuery)
    {
        return new Queryable<TransactionSubCategory>
        {
            Where = transactionSubCategory =>
                (transactionSubCategoryQuery.Id == null || transactionSubCategoryQuery.Id == transactionSubCategory.Id) &&
                (transactionSubCategoryQuery.Name == null || transactionSubCategoryQuery.Name == transactionSubCategory.Name) &&
                (transactionSubCategoryQuery.TransactionCategoryId == null || transactionSubCategoryQuery.TransactionCategoryId == transactionSubCategory.TransactionCategoryId) &&
                (transactionSubCategoryQuery.TransactionCategoryIds == null || transactionSubCategoryQuery.TransactionCategoryIds.Any(transactionCategoryId => transactionCategoryId == transactionSubCategory.TransactionCategoryId)) &&
                (transactionSubCategoryQuery.TransactionCategoryName == null || transactionSubCategoryQuery.TransactionCategoryName == transactionSubCategory.TransactionCategory.Name),
            OrderByDescending = x => x.UpdatedDate,
            Take = transactionSubCategoryQuery.Take,
            Skip = transactionSubCategoryQuery.Skip
        };
    }
}