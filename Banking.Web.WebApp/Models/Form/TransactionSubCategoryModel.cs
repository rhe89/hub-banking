using System.Collections.Generic;
using System.Linq;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Web.WebApp.Models.Form;

public sealed class TransactionSubCategoryModel : TransactionSubCategoryDto, IValidatableModel
{
    public IDictionary<string, string> ValidationErrors { get; } = new Dictionary<string, string>
    {
        { nameof(Name), "" },
        { nameof(TransactionSubCategoryDto.KeywordList), "" },
    };

    public static TransactionSubCategoryModel CreateFromDto(TransactionSubCategoryDto transactionSubCategoryDto)
    {
        return new TransactionSubCategoryModel
        {
            Id = transactionSubCategoryDto.Id,
            CreatedDate = transactionSubCategoryDto.CreatedDate,
            UpdatedDate = transactionSubCategoryDto.UpdatedDate,
            Name = transactionSubCategoryDto.Name,
            KeywordList = transactionSubCategoryDto.KeywordList
        };
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

        if (KeywordList.Count == 0)
        {
            ValidationErrors[nameof(TransactionSubCategoryDto.KeywordList)] =
                "Must have at least one keyword";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(TransactionSubCategoryDto.KeywordList)] =
                "";
        }
        
        if (KeywordList.Any(keyword => string.IsNullOrWhiteSpace(keyword.Value)))
        {
            ValidationErrors[nameof(TransactionSubCategoryDto.KeywordList)] =
                "One or more keywords is missing a value";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(TransactionSubCategoryDto.KeywordList)] =
                "";
        }
    }
}