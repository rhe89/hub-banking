using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

public class RecurringTransaction : EntityBase
{
    [UsedImplicitly]
    [Column]
    public string Text { get; set; }
        
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [UsedImplicitly]
    [Column]
    public long AccountId { get; set; }

    [UsedImplicitly]
    [Column]
    public DateTime LatestTransactionCreated { get; set; }
    
    [UsedImplicitly]
    [Column]
    public DateTime NextTransactionDate { get; set; }
    
    [UsedImplicitly]
    [Column]
    public Occurrence Occurrence { get; set; }
    
    
}
