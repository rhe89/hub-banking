using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Storage.Repository;
using Sbanken.Data.Entities;
using Sbanken.Dto.Api;

namespace Sbanken.Providers
{
    public class AccountProvider : IAccountProvider
    {
        private readonly IDbRepository _dbRepository;

        public AccountProvider(IDbRepository dbRepository)
        {
            _dbRepository = dbRepository;
        }
        
        public async Task<IEnumerable<AccountDto>> GetAccounts()
        {
            var accounts = await _dbRepository.GetManyAsync<Account>();
            
            return accounts.Select(x => new AccountDto
            {
                Name = x.Name,
                Balance = x.CurrentBalance
            });
        }
        
        public async Task<IEnumerable<AccountDto>> GetStandardAccounts()
        {
            var accounts = await GetAccountsOfType("Standard account", nameof(Account.Transactions));
            
            return accounts.Select(EntityToDtoMapper.Map);
        }

        public async Task<IEnumerable<AccountDto>> GetCreditAccounts()
        {
            var accounts = await GetAccountsOfType("Creditcard account", nameof(Account.Transactions));
            
            return accounts.Select(EntityToDtoMapper.Map);
        }

        public async Task<IEnumerable<AccountDto>> GetSavingsAccounts()
        {
            var accounts =  await GetAccountsOfType("High interest account", nameof(Account.Transactions), nameof(Account.AccountBalances));
            
            var accountDtos = new List<AccountDto>();

            foreach (var account in accounts)
            {
                var lastMonthsBalance = GetLastMonthsBalance(account);
                var lastYearsBalance = GetLastYearsBalance(account);

                var accountDto = EntityToDtoMapper.Map(account);

                accountDto.LastMonthBalance = lastMonthsBalance?.Balance ?? 0;
                accountDto.LastYearBalance = lastYearsBalance?.Balance ?? 0;

                accountDtos.Add(accountDto);
            }

            return accountDtos;
        }
        
        private async Task<IEnumerable<Account>> GetAccountsOfType(string accountType, params string[] includes)
        {
            var accounts = await _dbRepository
                .GetManyAsync<Account>(
                    x => x.AccountType == accountType, 
                    includes);

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
