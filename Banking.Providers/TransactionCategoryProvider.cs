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
    private readonly IHubDbRepository _dbRepository;

    public TransactionCategoryProvider(IHubDbRepository dbRepository)
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
    
    private static Hub.Shared.Storage.Repository.Core.Queryable<TransactionCategory> GetQueryable(TransactionCategoryQuery transactionCategoryQuery)
    {
        return new Hub.Shared.Storage.Repository.Core.Queryable<TransactionCategory>
        {
            Query = transactionCategoryQuery,
            Where = transactionCategory =>
                (transactionCategoryQuery.Id == null || transactionCategoryQuery.Id == transactionCategory.Id) &&
                (transactionCategoryQuery.Name == null || transactionCategoryQuery.Name == transactionCategory.Name),
            Include = transactionCategory => transactionCategory.TransactionSubCategories,
            OrderBy = x => x.Name
        };
    }

    private static Hub.Shared.Storage.Repository.Core.Queryable<TransactionSubCategory> GetQueryable(TransactionSubCategoryQuery transactionSubCategoryQuery)
    {
        return new Hub.Shared.Storage.Repository.Core.Queryable<TransactionSubCategory>
        {
            Query = transactionSubCategoryQuery,
            Where = transactionSubCategory =>
                (transactionSubCategoryQuery.Id == null || transactionSubCategoryQuery.Id == transactionSubCategory.Id) &&
                (transactionSubCategoryQuery.Name == null || transactionSubCategoryQuery.Name == transactionSubCategory.Name) &&
                (transactionSubCategoryQuery.TransactionCategoryId == null || transactionSubCategoryQuery.TransactionCategoryId == transactionSubCategory.TransactionCategoryId) &&
                (transactionSubCategoryQuery.TransactionCategoryIds == null || transactionSubCategoryQuery.TransactionCategoryIds.Any(transactionCategoryId => transactionCategoryId == transactionSubCategory.TransactionCategoryId)) &&
                (transactionSubCategoryQuery.TransactionCategoryName == null || transactionSubCategoryQuery.TransactionCategoryName == transactionSubCategory.TransactionCategory.Name),
            OrderByDescending = x => x.UpdatedDate
        };
    }
}