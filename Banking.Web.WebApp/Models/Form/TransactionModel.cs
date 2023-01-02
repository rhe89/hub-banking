using System;
using System.Collections.Generic;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;

namespace Banking.Web.WebApp.Models.Form;

public class TransactionModel : TransactionDto, IValidatableModel
{
    public TransactionDirection TransactionDirection { get; set; }

    public new DateTime? TransactionDate
    {
        get => base.TransactionDate;
        set => base.TransactionDate = value ?? DateTimeUtils.Today;
    }

    public IDictionary<string, string> ValidationErrors { get; } = new Dictionary<string, string>
    {
        { nameof(Amount), "" },
        { nameof(AccountId), "" },
        { nameof(Description), "" },
        { nameof(TransactionDate), "" },
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
        
        if (AccountId <= 0)
        {
            ValidationErrors[nameof(AccountId)] = "Missing account";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(AccountId)] = "";
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
        
        if (TransactionDate == DateTime.MinValue)
        {
            ValidationErrors[nameof(TransactionDate)] = "Missing transaction date";
            isValid = false;
        }
        else
        {
            ValidationErrors[nameof(TransactionDate)] = "";
        }
    }
}