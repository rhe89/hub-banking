using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hub.Storage.Repository.Core;
using Microsoft.EntityFrameworkCore;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;
using Sbanken.Core.Providers;

namespace Sbanken.Providers
{
    public class AccountBalanceProvider : IAccountBalanceProvider
    {
        private readonly IHubDbRepository _dbRepository;

        public AccountBalanceProvider(IHubDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }
        
        public async Task<IList<AccountBalanceDto>> GetAccountBalances(string accountName, 
            string accountType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            Expression<Func<AccountBalance, bool>> predicate = accountBalance => 
                (string.IsNullOrEmpty(accountName) || accountBalance.Account.Name.ToLower().Contains(accountName.ToLower()))
                && (string.IsNullOrEmpty(accountType) || accountBalance.Account.AccountType.ToLower().Contains(accountType.ToLower())
                && (fromDate == null || accountBalance.CreatedDate >= fromDate.Value)
                && (toDate == null || accountBalance.CreatedDate <= toDate.Value));

            var accountBalances = await _dbRepository
                .WhereAsync<AccountBalance, AccountBalanceDto>(predicate, queryable => queryable.Include(x => x.Account));

            return accountBalances;
        }
    }
}
