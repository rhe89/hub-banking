using System;
using System.Collections.Generic;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Web.WebApp.Models.Form;

public class ScheduledTransactionModel : ScheduledTransactionDto, IValidatableModel
{
    public TransactionDirection TransactionDirection { get; set; }

    public new DateTime? NextTransactionDate
    {
        get => base.NextTransactionDate;
        set => base.NextTransactionDate = value ?? DateTimeUtils.Today;
    }

    public IDictionary<string, string> ValidationErrors { get; } = new Dictionary<string, string>
    {
        { nameof(Amount), "" },
        { nameof(TransactionSubCategoryId), "" },
        { nameof(Description), "" },
        { nameof(NextTransactionDate), "" }
    };

    public void OnAmountChanged(decimal value)
    {
        Amount = TransactionDirection switch
        {
            TransactionDirection.Out when value > 0 => decimal.Negate(value),
            TransactionDirection.In when value < 0 => Math.Abs(value),
            _ => value
        };
        
        TransactionDirection = Amount > 0 ? TransactionDirection.In : TransactionDirection.Out;
    }

    public void OnTransactionDirectionChanged(TransactionDirection transactionDirection)
    {
        Amount = transactionDirection switch
        {
            TransactionDirection.Out when Amount > 0 => decimal.Negate(Amount),
            TransactionDirection.In when Amount < 0 => Math.Abs(Amount),
            _ => Amount
        };

        TransactionDirection = transactionDirection;
    }

    public void Validate(out bool isValid)
    {
        isValid = true;

        if (string.IsNullOrEmpty(Description))
        {
            ValidationErrors[nameof(Description)] = "Missing description";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(Description)] = "";
        }
        
        if (TransactionSubCategoryId <= 0)
        {
            ValidationErrors[nameof(TransactionSubCategoryId)] = "Missing category";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(TransactionSubCategoryId)] = "";
        }
        
        if (Amount == 0)
        {
            ValidationErrors[nameof(Amount)] = "Missing amount";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(Amount)] = "";
        }
        
        if (NextTransactionDate == DateTime.MinValue)
        {
            ValidationErrors[nameof(NextTransactionDate)] = "Missing next transaction date";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(NextTransactionDate)] = "";
        }
        
        if (TransactionSubCategoryId == null)
        {
            ValidationErrors[nameof(TransactionSubCategoryId)] = "Missing transaction category";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(TransactionSubCategoryId)] = "";
        }
    }
}