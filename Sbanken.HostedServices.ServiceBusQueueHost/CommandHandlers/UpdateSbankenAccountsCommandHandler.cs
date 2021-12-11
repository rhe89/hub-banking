using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Sbanken;
using Hub.Shared.Storage.Repository.Core;
using Microsoft.Extensions.Logging;
using Sbanken.Data.Entities;
using Sbanken.Integration;
using Sbanken.Integration.Dto;

namespace Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers
{
    public interface IUpdateSbankenAccountsCommandHandler
    {
        Task UpdateAccounts();
    }
    
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
        
        public async Task UpdateAccounts()
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

            _logger.LogInformation("Fetching accounts from Sbanken");

            while (!success && retryCount < 1)
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

            _logger.LogInformation("Fetched {Count} accounts", accounts.Count);

            return accounts;
        }


        private async Task UpdateCurrentAccountBalances(IEnumerable<SbankenAccount> accountsFromSbanken)
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
        }

        
        private void CreateAccount(SbankenAccount sbankenAccount)
        {
            _logger.LogInformation("Adding new account {AccountName}", sbankenAccount.Name);
            
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
            _logger.LogInformation("Updating account {AccountName}", sbankenAccount.Name);

            accountInDb.Balance = sbankenAccount.Available;
            accountInDb.AccountType = sbankenAccount.AccountType;
            
            _dbRepository.QueueUpdate<Account, AccountDto>(accountInDb);
        }
    }
}