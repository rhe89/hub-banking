using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

[UsedImplicitly]
public class Account : EntityBase
{
    [UsedImplicitly]
    [Column]
    public long? BankId { get; set; }
    
    [UsedImplicitly]
    [Column]
    public string Name { get; set; }
    
    [UsedImplicitly]
    [Column]
    public string AccountNumber { get; set; }

    [UsedImplicitly]
    [Column]
    public string AccountType { get; set; }

    [UsedImplicitly]
    [Column]
    public bool SharedAccount { get; set; }
    
    [UsedImplicitly]
    [Column]
    public DateTime? DiscontinuedDate { get; set; }
    
    [UsedImplicitly]
    public virtual Bank Bank { get; set; }
    
    [UsedImplicitly]
    public virtual ICollection<AccountBalance> AccountBalance { get; set; }
}