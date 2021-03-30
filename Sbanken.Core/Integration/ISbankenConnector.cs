using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Core.Dto.Integration;

namespace Sbanken.Core.Integration
{
    public interface ISbankenConnector
    {
        Task<List<SbankenAccount>> GetAccounts();
        Task<IList<SbankenTransaction>> GetTransactions(DateTime startDate, DateTime? endDate);
        Task<IList<object>> GetTransactionsRaw(string accountName = null);
        Task<object> GetAccountsRaw();

    }
}