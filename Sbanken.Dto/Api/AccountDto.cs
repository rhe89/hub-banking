using System.Collections.Generic;

namespace Sbanken.Dto.Api
{
    public class AccountDto
    {
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string AccountType { get; set; }
        public IList<TransactionDto> Transactions { get; set; }
        public decimal LastMonthBalance { get; set; }
        public decimal LastYearBalance { get; set; }
    }
}