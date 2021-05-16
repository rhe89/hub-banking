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
        Task<IList<object>> GetTransactionsRaw();
        Task<IList<object>> GetTransactionsRaw(string accountName);
        Task<IList<object>> GetArchivedTransactionsRaw();
        Task<IList<object>> GetArchivedTransactionsRaw(string accountName);
        Task<object> GetAccountsRaw();

    }
}