using System.ComponentModel.DataAnnotations.Schema;
using Hub.Storage.Core.Entities;

namespace Sbanken.Core.Entities
{
    public class AccountBalance : EntityBase
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
        public long AccountId { get; set; }
        public virtual Account Account { get; set; }
    }
}