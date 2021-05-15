using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hub.Storage.Repository.Entities;

namespace Sbanken.Core.Entities
{
    public class Transaction : EntityBase
    {
        public string Text { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public long AccountId { get; set; }
        public virtual Account Account { get; set; }
        public int TransactionType { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}