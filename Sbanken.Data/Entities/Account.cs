using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Sbanken.Data.Entities
{
    [UsedImplicitly]
    [Table("Account")]
    public class Account : EntityBase
    {
        [UsedImplicitly]
        [Column]
        public string Name { get; set; }
        
        [UsedImplicitly]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        [UsedImplicitly]
        [Column]
        public string AccountType { get; set; }
        
        [UsedImplicitly]
        public virtual ICollection<Transaction> Transactions { get; set; }
        
        [UsedImplicitly]
        public virtual ICollection<AccountBalance> AccountBalances { get; set; }
    }
}