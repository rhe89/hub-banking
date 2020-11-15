using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Storage.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;
using Sbanken.Core.Providers;

namespace Sbanken.Providers
{
    public class AccountProvider : IAccountProvider
    {
        private readonly IHubDbRepository _dbRepository;

        public AccountProvider(IHubDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }
        
        public async Task<IEnumerable<AccountDto>> GetAccounts()
        {
            var accounts = await _dbRepository.AllAsync<Account, AccountDto>();

            return accounts;
        }
        
        public async Task<IEnumerable<AccountDto>> GetStandardAccounts()
        {
            var accounts = await GetAccountsOfType("Standard account");

            return _dbRepository.Map<IEnumerable<Account>, IEnumerable<AccountDto>>(accounts);
        }

        public async Task<IEnumerable<AccountDto>> GetCreditAccounts()
        {
            var accounts = await GetAccountsOfType("Creditcard account");
            
            return _dbRepository.Map<IEnumerable<Account>, IEnumerable<AccountDto>>(accounts);
        }

        public async Task<IEnumerable<AccountDto>> GetSavingsAccounts()
        {
            var accounts =  await GetAccountsOfType("High interest account");
            
            var accountDtos = new List<AccountDto>();

            foreach (var account in accounts)
            {
                var lastMonthsBalance = GetLastMonthsBalance(account);
                var lastYearsBalance = GetLastYearsBalance(account);

                var accountDto = _dbRepository.Map<Account, AccountDto>(account);

                accountDto.LastMonthBalance = lastMonthsBalance?.Balance ?? 0;
                accountDto.LastYearBalance = lastYearsBalance?.Balance ?? 0;

                accountDtos.Add(accountDto);
            }

            return accountDtos;
        }
        
        private async Task<IEnumerable<Account>> GetAccountsOfType(string accountType)
        {
            var accounts = await _dbRepository
                .Where<Account>(x => x.AccountType == accountType)
                .Include(x => x.Transactions)
                .Include(x => x.AccountBalances)
                .ToListAsync();

            return accounts;
        }

        private static AccountBalance GetLastMonthsBalance(Account account)
        {
            var lastMonth = DateTime.Now.AddMonths(-1);
            var lastDayInMonth = DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month);

            var lastMonthDateTime = new DateTime(lastMonth.Year, lastMonth.Month, lastDayInMonth--);

            var lastMonthBalance =
                account.AccountBalances.FirstOrDefault(x => x.CreatedDate.Date == lastMonthDateTime);

            while (lastMonthBalance == null && lastDayInMonth != 0)
            {
                lastMonthDateTime = new DateTime(lastMonth.Year, lastMonth.Month, lastDayInMonth--);

                lastMonthBalance =
                    account.AccountBalances.FirstOrDefault(x => x.CreatedDate.Date == lastMonthDateTime.Date);
            }

            return lastMonthBalance;
        }
        
        private static AccountBalance GetLastYearsBalance(Account account)
        {
            var lastYear = DateTime.Now.AddYears(-1);
            const int lastMonthInYear = 12;
            var lastDayInLastMonthInYear = 31;

            var lastYearDateTime = new DateTime(lastYear.Year, lastMonthInYear, lastDayInLastMonthInYear--);

            var lastYearBalance =
                account.AccountBalances.FirstOrDefault(x => x.CreatedDate.Date == lastYearDateTime);

            while (lastYearBalance == null && lastDayInLastMonthInYear != 0)
            {
                lastYearDateTime = new DateTime(lastYear.Year, lastMonthInYear, lastDayInLastMonthInYear--);

                lastYearBalance =
                    account.AccountBalances.FirstOrDefault(x => x.CreatedDate.Date == lastYearDateTime.Date);
            }

            return lastYearBalance;
        }
    }
}
