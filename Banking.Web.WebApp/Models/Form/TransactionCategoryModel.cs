using System.Collections.Generic;
using System.Linq;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Web.WebApp.Models.Form;

public sealed class TransactionCategoryModel : TransactionCategoryDto, IValidatableModel
{
    public IDictionary<string, string> ValidationErrors { get; } = new Dictionary<string, string>
    {
        { nameof(Name), "" },
        { $"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.Name)}", "" },
        { $"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.KeywordList)}", "" },
    };
    
    public TransactionCategoryModel()
    {
        TransactionSubCategories = new List<TransactionSubCategoryDto>();

        CreateNewTransactionSubCategory();
    }

    public static TransactionCategoryModel CreateFromDto(TransactionCategoryDto transactionCategoryDto)
    {
        return new TransactionCategoryModel
        {
            Id = transactionCategoryDto.Id,
            CreatedDate = transactionCategoryDto.CreatedDate,
            UpdatedDate = transactionCategoryDto.UpdatedDate,
            Name = transactionCategoryDto.Name,
            TransactionSubCategories = transactionCategoryDto.TransactionSubCategories
        };
    }

    public void CreateNewTransactionSubCategory()
    {
        TransactionSubCategories.Add(new TransactionSubCategoryDto { KeywordList = new List<Keyword> { new() { Value = "" } } });
    }
    
    public void Validate(out bool isValid)
    {
        isValid = true;

        if (string.IsNullOrEmpty(Name))
        {
            ValidationErrors[nameof(Name)] = "Missing category name";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(Name)] = "";
        }

        if (TransactionSubCategories.Any(transactionSubCategory => string.IsNullOrWhiteSpace(transactionSubCategory.Name)))
        {
            ValidationErrors[$"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.Name)}"] =
                "All sub categories must have a name";
            isValid = false;
        }
        else
        {
            ValidationErrors[$"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.Name)}"] = "";
        }

        if (TransactionSubCategories.Any(transactionSubCategory => transactionSubCategory.KeywordList.Count == 0))
        {
            ValidationErrors[$"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.KeywordList)}"] =
                "All sub categories must have at least one keyword";
            isValid = false;
        }
        else
        {
            ValidationErrors[$"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.KeywordList)}"] =
                "";
        }
        
        if (TransactionSubCategories.Any(transactionSubCategory => transactionSubCategory.KeywordList.Any(keyword => string.IsNullOrWhiteSpace(keyword.Value))))
        {
            ValidationErrors[$"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.KeywordList)}"] =
                "One or more sub category keywords is missing a value";
            isValid = false;
        }
        else
        {
            ValidationErrors[$"{nameof(TransactionSubCategories)}.${nameof(TransactionSubCategoryDto.KeywordList)}"] =
                "";
        }
    }
}