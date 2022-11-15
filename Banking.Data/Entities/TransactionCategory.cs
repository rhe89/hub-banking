using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

public class TransactionCategory : EntityBase
{
    [Column]
    [UsedImplicitly]
    public string Name { get; set; }
    
    [UsedImplicitly] 
    public virtual ICollection<TransactionSubCategory> TransactionSubCategories { get; set; }
}