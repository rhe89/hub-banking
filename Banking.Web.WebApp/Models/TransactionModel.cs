using System;
using System.Collections.Generic;
using Banking.Web.WebApp.Extensions;
using Hub.Shared.DataContracts.Banking.Dto;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Models;

public class TransactionModel : TransactionDto
{
    public TransactionDirection TransactionDirection { get; set; }
    
    public IDictionary<string, string> ValidationErrors { get; } = new Dictionary<string, string>()
    {
        { nameof(Amount), "" },
        { nameof(AccountId), "" },
        { nameof(Description), "" },
        { nameof(TransactionDate), "" },
    };
    
    public void OnDescriptionInput(ChangeEventArgs obj, Action<string> onDescriptionChanged)
    {
        if (obj?.Value is not string value)
        {
            return;
        }

        Description = value.ToUpperUnderscored();
        
        onDescriptionChanged(Description);
    }
    
    public void OnDescriptionInput(ChangeEventArgs obj)
    {
        if (obj?.Value is not string value)
        {
            return;
        }

        Description = value.ToUpperUnderscored();
    }
    
    public void OnAmountInput(ChangeEventArgs obj)
    {
        if (obj?.Value is not string strValue || 
            !decimal.TryParse(strValue, out var decimalValue))
        {
            return;
        }

        Amount = TransactionDirection switch
        {
            TransactionDirection.Out when decimalValue > 0 => decimal.Negate(decimalValue),
            TransactionDirection.In when decimalValue < 0 => Math.Abs(decimalValue),
            _ => Amount
        };
    }

    public void OnTransactionDirectionChanged(ChangeEventArgs obj)
    {
        if (obj.Value is not string value || 
            !Enum.TryParse(typeof(TransactionDirection), value, out var transactionDirection))
        {
            return;
        }
        
        Amount = transactionDirection switch
        {
            TransactionDirection.Out when Amount > 0 => decimal.Negate(Amount),
            TransactionDirection.In when Amount < 0 => Math.Abs(Amount),
            _ => Amount
        };
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