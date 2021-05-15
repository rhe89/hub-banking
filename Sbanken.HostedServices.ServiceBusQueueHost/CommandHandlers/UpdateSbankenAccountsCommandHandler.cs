using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hub.Storage.Repository.Core;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Dto.Integration;
using Sbanken.Core.Entities;
using Sbanken.Core.Integration;

namespace Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers
{
    public class UpdateSbankenAccountsCommandHandler : IUpdateSbankenAccountsCommandHandler
    {
        private readonly ISbankenConnector _sbankenConnector;
        private readonly IHubDbRepository _dbRepository;
        private readonly ILogger<UpdateSbankenAccountsCommandHandler> _logger;

        public UpdateSbankenAccountsCommandHandler(ILogger<UpdateSbankenAccountsCommandHandler> logger,
            ISbankenConnector sbankenConnector,
            IHubDbRepository dbRepository)
        {
            _logger = logger;
            _sbankenConnector = sbankenConnector;
            _dbRepository = dbRepository;
        }
        
        public async Task FetchAndUpdateCurrentAccountBalances()
        {
            var accountDtos = await FetchCurrentAccountBalances();

            await UpdateCurrentAccountBalances(accountDtos);
        }

        private async Task<IList<SbankenAccount>> FetchCurrentAccountBalances()
        {
            var success = false;
            var retryCount = 0;
            Exception ex = null;
            var accounts = new List<SbankenAccount>();

            _logger.LogInformation("Fetching accounts from Sbanken.");

            while (!success && retryCount < 100)
            {
                try
                {
                    accounts = await _sbankenConnector.GetAccounts();
                    success = true;
                    ex = null;
                }
                catch (Exception e)
                {
                    ex = e;
                    retryCount++;
                    success = false;
                    Thread.Sleep(1000);
                }
            }
            if (ex != null)
            {
                throw ex;
            }

            _logger.LogInformation($"Finished fetching accounts from Sbanken. Found {accounts.Count} accounts.");

            return accounts;
        }


        private async Task UpdateCurrentAccountBalances(IList<SbankenAccount> accountsFromSbanken)
        {
            var existingAccounts = _dbRepository.All<Account, AccountDto>().ToList();

            foreach (var sbankenAccount in accountsFromSbanken)
            {
                var accountInDb = existingAccounts.FirstOrDefault(x => x.Name == sbankenAccount.Name);

                if (accountInDb == null)
                {
                    CreateAccount(sbankenAccount);
                }
                else
                {
                    UpdateAccount(accountInDb, sbankenAccount);
                }
            }
            
            await _dbRepository.ExecuteQueueAsync();

            foreach (var sbankenAccount in accountsFromSbanken)
            {
                var accountInDb = existingAccounts.FirstOrDefault(x => x.Name == sbankenAccount.Name);

                UpdateAccountBalanceHistory(accountInDb);
            }
            
            await _dbRepository.ExecuteQueueAsync();
            
            _logger.LogInformation("Finished updating accounts");
        }

        
        private void CreateAccount(SbankenAccount sbankenAccount)
        {
            _logger.LogInformation($"Adding new account {sbankenAccount.Name}");
            
            var account = new AccountDto
            {
                Name = sbankenAccount.Name,
                Balance = sbankenAccount.Available,
                AccountType = sbankenAccount.AccountType
            };

            _dbRepository.QueueAdd<Account, AccountDto>(account);
        }

        private void UpdateAccount(AccountDto accountInDb, SbankenAccount sbankenAccount)
        {
            _logger.LogInformation($"Updating account {sbankenAccount.Name}");

            accountInDb.Balance = sbankenAccount.Available;
            accountInDb.AccountType = sbankenAccount.AccountType;
            
            _dbRepository.QueueUpdate<Account, AccountDto>(accountInDb);
        }
        
        private void UpdateAccountBalanceHistory(AccountDto accountDto)
        {
            var now = DateTime.Now;

            _logger.LogInformation($"Updating account balance history for account {accountDto.Name}");

            var accountBalanceForCurrentDay = accountDto.AccountBalances.FirstOrDefault(x =>
                x.AccountId == accountDto.Id &&
                x.CreatedDate.Year == now.Year &&
                x.CreatedDate.Month == now.Month &&
                x.CreatedDate.Day == now.Day);
            
            if (accountBalanceForCurrentDay == null)
            {
                accountBalanceForCurrentDay = new AccountBalanceDto
                {
                    AccountId = accountDto.Id,
                    Balance = accountDto.Balance
                };

                _dbRepository.QueueAdd<AccountBalance, AccountBalanceDto>(accountBalanceForCurrentDay);
            }
            else
            {
                accountBalanceForCurrentDay.Balance = accountDto.Balance;
                
                _dbRepository.QueueUpdate<AccountBalance, AccountBalanceDto>(accountBalanceForCurrentDay);
            }
        }
    }
}