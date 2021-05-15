using System;
using Hub.Storage.Repository.Dto;

namespace Sbanken.Core.Dto.Data
{
    public class TransactionDto : EntityDtoBase, IComparable<TransactionDto>
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime TransactionDate { get; set; }
        public int TransactionType { get; set; }
        public string TransactionIdentifier { get; set; }
        
        public int CompareTo(TransactionDto other)
        {
            if (TransactionDate > other.TransactionDate)
            {
                return 1;
            } 
            
            if (TransactionDate < other.TransactionDate)
            {
                return -1;
            }

            return 0;
        }
    }
}