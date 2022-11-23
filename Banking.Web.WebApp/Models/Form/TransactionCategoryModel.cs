using System.Collections.Generic;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Web.WebApp.Models.Form;

public sealed class TransactionCategoryModel : TransactionCategoryDto, IValidatableModel
{
    public IDictionary<string, string> ValidationErrors { get; } = new Dictionary<string, string>
    {
        { nameof(Name), "" }
    };
    
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
    }
}