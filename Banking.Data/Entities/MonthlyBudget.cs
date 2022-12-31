using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

[UsedImplicitly]
public class MonthlyBudget : EntityBase
{
    [UsedImplicitly]
    [Column]
    public DateTime Month { get; set; }

    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Income { get; set; }

    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Savings { get; set; }
    
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Mortgage { get; set; }
    
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal SharedAccountTransactions{ get; set; }
    
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Investments { get; set; }

    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Bills { get; set; }
    
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Result { get; set; }
}