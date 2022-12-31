using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Microsoft.Extensions.Logging;

namespace Banking.Services;

public interface ITransactionCategoryService
{
    Task<TransactionCategoryDto> Add(TransactionCategoryDto newTransactionCategory, bool saveChanges);
    Task<TransactionSubCategoryDto> Add(TransactionSubCategoryDto newTransactionSubCategory, bool saveChanges);
    Task Update(TransactionCategoryDto updatedTransactionCategory, bool saveChanges);
    Task Update(TransactionSubCategoryDto updatedTransactionSubCategory, bool saveChanges);
    Task<TransactionSubCategoryDto> GetOrAdd(string category, string subCategory);
    Task DeleteTransactionCategory(long transactionCategoryId, bool saveChanges);
    Task DeleteTransactionSubCategory(long transactionSubCategoryId, bool saveChanges);
}

public class TransactionCategoryService : ITransactionCategoryService
{
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    private readonly ICacheableHubDbRepository _dbRepository;
    private readonly ILogger<TransactionCategoryService> _logger;

    public TransactionCategoryService(
        ITransactionCategoryProvider transactionCategoryProvider,
        ICacheableHubDbRepository dbRepository,
        ILogger<TransactionCategoryService> logger)
    {
        _transactionCategoryProvider = transactionCategoryProvider;
        _dbRepository = dbRepository;
        _logger = logger;
    }
    
    public async Task<TransactionCategoryDto> Add(TransactionCategoryDto newTransactionCategory, bool saveChanges)
    {
        var transactionCategoriesWithSameName = await _transactionCategoryProvider.Get(new TransactionCategoryQuery
        {
            Name = newTransactionCategory.Name
        });

        if (transactionCategoriesWithSameName.Any())
        {
            var existingTransactionCategory = transactionCategoriesWithSameName.First();
            
            foreach (var subCategory in newTransactionCategory.TransactionSubCategories)
            {
                subCategory.TransactionCategoryId = existingTransactionCategory.Id;
            
                var addedSubCategory = await Add(subCategory, saveChanges);
            
                existingTransactionCategory.TransactionSubCategories.Add(addedSubCategory);
            }
            
            return transactionCategoriesWithSameName.First();
        }
        
        _logger.LogInformation("Creating transaction category {Name}", newTransactionCategory.Name);

        var addedTransactionCategory = _dbRepository.Add<TransactionCategory, TransactionCategoryDto>(newTransactionCategory);

        if (newTransactionCategory.TransactionSubCategories == null)
        {
            return addedTransactionCategory;
        }
        
        foreach (var subCategory in newTransactionCategory.TransactionSubCategories)
        {
            subCategory.TransactionCategoryId = addedTransactionCategory.Id;
            
            var addedSubCategory = await Add(subCategory, saveChanges);
            
            addedTransactionCategory.TransactionSubCategories.Add(addedSubCategory);
        }

        return addedTransactionCategory;
    }
    
    public async Task<TransactionSubCategoryDto> Add(TransactionSubCategoryDto newTransactionSubCategory, bool saveChanges)
    {
        var transactionCategoriesWithSameName = await _transactionCategoryProvider.Get(new TransactionSubCategoryQuery
        {
            Name = newTransactionSubCategory.Name
        });

        if (transactionCategoriesWithSameName.Any())
        {
            return transactionCategoriesWithSameName.First();
        }
        
        _logger.LogInformation("Creating transaction sub category {Name}", newTransactionSubCategory.Name);

        if (saveChanges)
        {
            return _dbRepository.Add<TransactionSubCategory, TransactionSubCategoryDto>(newTransactionSubCategory);
        }
        
        _dbRepository.QueueAdd<TransactionSubCategory, TransactionSubCategoryDto>(newTransactionSubCategory);
        
        return newTransactionSubCategory;
    }
    
    public async Task Update(TransactionCategoryDto updatedTransactionCategory, bool saveChanges)
    {
        _logger.LogInformation(
            "Updating transaction category {Name} (Id: {Id})", 
            updatedTransactionCategory.Name,
            updatedTransactionCategory.Id);

        var transactionCategoryInDb = (await _transactionCategoryProvider.Get(new TransactionCategoryQuery
        {
            Id = updatedTransactionCategory.Id
        })).First();

        transactionCategoryInDb.Name = updatedTransactionCategory.Name;

        _dbRepository.QueueUpdate<TransactionCategory, TransactionCategoryDto>(transactionCategoryInDb);

        if (updatedTransactionCategory.TransactionSubCategories != null)
        {
            foreach (var subCategory in updatedTransactionCategory.TransactionSubCategories)
            {
                if (subCategory.Id == 0)
                {
                    subCategory.TransactionCategoryId = transactionCategoryInDb.Id;
                
                    await Add(subCategory, false);
                }
                else
                {
                    await Update(subCategory, false);
                }
            }

            foreach (var subCategory in transactionCategoryInDb.TransactionSubCategories)
            {
                if (updatedTransactionCategory.TransactionSubCategories.All(x => x.Id != subCategory.Id))
                {
                    await DeleteTransactionSubCategory(subCategory.Id, false);
                }
            }
        }
        
        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }
    
    public async Task Update(TransactionSubCategoryDto updatedTransactionSubCategory, bool saveChanges)
    {
        _logger.LogInformation(
            "Updating transaction sub category {Name} (Id: {Id})", 
            updatedTransactionSubCategory.Name,
            updatedTransactionSubCategory.Id);
        
        var transactionSubCategoryInDb = (await _transactionCategoryProvider.Get(new TransactionSubCategoryQuery
        {
            Id = updatedTransactionSubCategory.Id
        })).First();

        transactionSubCategoryInDb.Name = updatedTransactionSubCategory.Name;
        transactionSubCategoryInDb.KeywordList = updatedTransactionSubCategory.KeywordList;

        if (saveChanges)
        {
            await _dbRepository.UpdateAsync<TransactionSubCategory, TransactionSubCategoryDto>(transactionSubCategoryInDb);
        }
        else
        {
            _dbRepository.QueueUpdate<TransactionSubCategory, TransactionSubCategoryDto>(transactionSubCategoryInDb);
        }
    }

    public async Task<TransactionSubCategoryDto> GetOrAdd(string category, string subCategory)
    {
        var transactionCategory =
            (await _transactionCategoryProvider.Get(new TransactionCategoryQuery { Name = category })).FirstOrDefault();

        if (transactionCategory != null)
        {
            var transactionSubCategory = transactionCategory.TransactionSubCategories.FirstOrDefault(x => x.Name == subCategory);

            if (transactionSubCategory == null)
            {
                return await Add(new TransactionSubCategoryDto
                {
                    TransactionCategoryId = transactionCategory.Id,
                    Name = subCategory,
                    Keywords = subCategory
                }, true);
            }
        }
        
        transactionCategory = await Add(
            new TransactionCategoryDto { Name = category , TransactionSubCategories = new List<TransactionSubCategoryDto>
            {
                new TransactionSubCategoryDto { Name = subCategory, Keywords = subCategory}
            }}, 
            true);
            
        return transactionCategory.TransactionSubCategories.First(x => x.Name == subCategory);
    }

    public async Task DeleteTransactionCategory(long transactionCategoryId, bool saveChanges)
    {
        var transactionCategoryInDb = (await _transactionCategoryProvider.Get(new TransactionCategoryQuery
        {
            Id = transactionCategoryId
        })).First();
        
        _logger.LogInformation(
            "Deleting transaction category {Name} (Id: {Id})", 
            transactionCategoryInDb.Name,
            transactionCategoryInDb.Id);
        
        foreach (var transactionSubCategory in transactionCategoryInDb.TransactionSubCategories)
        {
            await DeleteTransactionSubCategory(transactionSubCategory.Id, saveChanges);
        }
        
        if (saveChanges)
        {
            await _dbRepository.RemoveAsync<TransactionCategory, TransactionCategoryDto>(transactionCategoryInDb);
        }
        else
        {
            _dbRepository.QueueRemove<TransactionCategory, TransactionCategoryDto>(transactionCategoryInDb);
        }
    }
    
    public async Task DeleteTransactionSubCategory(long transactionSubCategoryId, bool saveChanges)
    {
        var transactionSubCategoryInDb = (await _transactionCategoryProvider.Get(new TransactionSubCategoryQuery
        {
            Id = transactionSubCategoryId
        })).First();
        
        _logger.LogInformation(
            "Deleting transaction sub category {Name} (Id: {Id})", 
            transactionSubCategoryInDb.Name,
            transactionSubCategoryInDb.Id);
        
        if (saveChanges)
        {
            await _dbRepository.RemoveAsync<TransactionSubCategory, TransactionSubCategoryDto>(transactionSubCategoryInDb);
        }
        else
        {
            _dbRepository.QueueRemove<TransactionSubCategory, TransactionSubCategoryDto>(transactionSubCategoryInDb);
        }
    }
}