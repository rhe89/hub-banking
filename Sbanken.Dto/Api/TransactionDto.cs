using System;

namespace Sbanken.Dto.Api
{
    public class TransactionDto : IComparable<TransactionDto>
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string AccountName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TransactionType { get; set; }
        
        public int CompareTo(TransactionDto other)
        {
            if (CreatedDate > other.CreatedDate)
            {
                return 1;
            } 
            
            if (CreatedDate < other.CreatedDate)
            {
                return -1;
            }

            return 0;
        }
    }
}