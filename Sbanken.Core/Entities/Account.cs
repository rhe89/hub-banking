using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Storage.Core.Entities;

namespace Sbanken.Core.Entities
{
    public class Account : EntityBase
    {
        public string Name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        public string AccountType { get; set; }
        
        public virtual ICollection<Transaction> Transactions { get; set; }
        public virtual ICollection<AccountBalance> AccountBalances { get; set; }
    }
}