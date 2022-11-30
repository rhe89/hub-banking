using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

public class AccumulatedAccountBalance : EntityBase
{
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
    
    [UsedImplicitly]
    public DateTime BalanceDate { get; set; }
        
    [UsedImplicitly]
    [Column]
    public long AccountId { get; set; }
        
    [UsedImplicitly]
    [ForeignKey("AccountId")]
    public virtual Account Account { get; set; }
}