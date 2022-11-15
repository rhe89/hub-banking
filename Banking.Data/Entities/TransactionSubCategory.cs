using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

public class TransactionSubCategory : EntityBase
{
    [Column]
    [UsedImplicitly]
    public long TransactionCategoryId { get; set; }
    
    [Column]
    [UsedImplicitly]
    public string Name { get; set; }
    
    [Column]
    [UsedImplicitly]
    public string Keywords { get; set; }
    
    [UsedImplicitly] 
    public virtual TransactionCategory TransactionCategory { get; set; }
}