using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

[UsedImplicitly]
public class ScheduledTransaction : EntityBase
{
    [UsedImplicitly]
    [Column]
    public long AccountId { get; set; }
    
    [UsedImplicitly]
    [Column]
    public long? TransactionSubCategoryId { get; set; }
    
    [UsedImplicitly]
    [Column]
    public string Text { get; set; }
    
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [UsedImplicitly]
    [Column]
    public Guid TransactionKey { get; set; }

    [UsedImplicitly]
    [Column]
    public DateTime LatestTransactionCreated { get; set; }
    
    [UsedImplicitly]
    [Column]
    public DateTime NextTransactionDate { get; set; }
    
    [UsedImplicitly]
    [Column]
    public Occurrence Occurrence { get; set; }
    
    [UsedImplicitly]
    [Column]
    public bool Completed { get; set; }
    
    [UsedImplicitly]
    public virtual TransactionSubCategory TransactionSubCategory { get; set; }
    
    [UsedImplicitly]
    public virtual Account Account { get; set; }
}
