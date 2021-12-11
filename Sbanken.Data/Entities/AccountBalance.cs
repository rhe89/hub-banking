using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Sbanken.Data.Entities;

[UsedImplicitly]
public class AccountBalance : EntityBase
{
    [UsedImplicitly]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
        
    [UsedImplicitly]
    [Column]
    public long AccountId { get; set; }
        
    [UsedImplicitly]
    public virtual Account Account { get; set; }
}