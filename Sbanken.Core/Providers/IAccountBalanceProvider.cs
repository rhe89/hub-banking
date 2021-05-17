using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sbanken.Core.Dto.Data;

namespace Sbanken.Core.Providers
{
    public interface IAccountBalanceProvider
    {
        Task<IList<AccountBalanceDto>> GetAccountBalances(string accountName, 
            string accountType,
            DateTime? fromDate,
            DateTime? toDate);
    }
}