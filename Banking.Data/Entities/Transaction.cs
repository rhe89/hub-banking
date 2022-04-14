using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

[UsedImplicitly]
public class Transaction : EntityBase
{
    [UsedImplicitly]
    [Column]
    public string Text { get; set; }
        
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
        
    [UsedImplicitly]
    [Column]
    public string TransactionId { get; set; }
        
    [UsedImplicitly]
    [Column]
    public long AccountId { get; set; }
        
    [UsedImplicitly]
    [Column]
    public int TransactionType { get; set; }
        
    [UsedImplicitly]
    [Column]
    public DateTime TransactionDate { get; set; }
        
    [UsedImplicitly]
    public virtual Account Account { get; set; }
}