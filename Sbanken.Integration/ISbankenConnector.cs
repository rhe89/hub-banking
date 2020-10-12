using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Dto.Sbanken;

namespace Sbanken.Integration
{
    public interface ISbankenConnector
    {
        Task<List<AccountDto>> GetAccounts();
        Task<IList<TransactionDto>> GetTransactions(DateTime startDate, DateTime? endDate);
    }
}