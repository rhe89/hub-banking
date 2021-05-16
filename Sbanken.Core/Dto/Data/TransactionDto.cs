using System;
using Hub.Storage.Repository.Dto;

namespace Sbanken.Core.Dto.Data
{
    public class TransactionDto : EntityDtoBase
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public long AccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int TransactionType { get; set; }
        public string TransactionId { get; set; }
    }
}