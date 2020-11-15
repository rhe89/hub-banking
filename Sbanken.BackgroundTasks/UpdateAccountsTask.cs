using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hub.HostedServices.Tasks;
using Hub.Storage.Core.Factories;
using Hub.Storage.Core.Providers;
using Hub.Storage.Core.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Dto.Sbanken;
using Sbanken.Core.Entities;
using Sbanken.Core.Integration;

namespace Sbanken.BackgroundTasks
{
    public class UpdateAccountsTask : BackgroundTask
    {
        private readonly ISbankenConnector _sbankenConnector;
        private readonly ILogger<UpdateAccountsTask> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UpdateAccountsTask(IBackgroundTaskConfigurationProvider backgroundTaskConfigurationProvider,
            IBackgroundTaskConfigurationFactory backgroundTaskConfigurationFactory,
            IServiceScopeFactory serviceScopeFactory,
            ILoggerFactory loggerFactory,
            ISbankenConnector sbankenConnector) : base(backgroundTaskConfigurationProvider, backgroundTaskConfigurationFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateAccountsTask>();
            _sbankenConnector = sbankenConnector;
            _serviceScopeFactory = serviceScopeFactory;
        }
        
        public override async Task Execute(CancellationToken cancellationToken)
        {
            await FetchAndUpdateCurrentAccountBalances();
        }

        private async Task FetchAndUpdateCurrentAccountBalances()
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
            using var scope = _serviceScopeFactory.CreateScope();

            using var dbRepository = scope.ServiceProvider.GetService<IScopedHubDbRepository>();
            
            var existingAccounts = dbRepository.All<Account, AccountDto>().ToList();

            foreach (var sbankenAccount in accountsFromSbanken)
            {
                var accountInDb = existingAccounts.FirstOrDefault(x => x.Name == sbankenAccount.Name);

                if (accountInDb == null)
                {
                    CreateAccount(sbankenAccount, dbRepository);
                }
                else
                {
                    UpdateAccount(accountInDb, sbankenAccount, dbRepository);
                }

                UpdateAccountBalanceHistory(accountInDb, dbRepository);
            }
            
            await dbRepository.SaveChangesAsync();
        }

        
        private void CreateAccount(SbankenAccount sbankenAccount, IScopedHubDbRepository dbRepository)
        {
            _logger.LogInformation($"Adding new account {sbankenAccount.Name}");
            
            var account = new AccountDto
            {
                Name = sbankenAccount.Name,
                Balance = sbankenAccount.Available,
                AccountType = sbankenAccount.AccountType
            };

            dbRepository.Add<Account, AccountDto>(account);
        }

        private void UpdateAccount(AccountDto accountInDb, SbankenAccount sbankenAccount, IScopedHubDbRepository dbRepository)
        {
            _logger.LogInformation($"Updating account {sbankenAccount.Name}");

            accountInDb.Balance = sbankenAccount.Available;
            accountInDb.AccountType = sbankenAccount.AccountType;
            
            dbRepository.Update<Account, AccountDto>(accountInDb);
        }
        
        private void UpdateAccountBalanceHistory(AccountDto accountDto, IScopedHubDbRepository dbRepository)
        {
            var now = DateTime.Now;

            var accountBalanceForCurrentDay = dbRepository.Where<AccountBalance>(x =>
                    x.AccountId == accountDto.Id &&
                    x.CreatedDate.Year == now.Year &&
                    x.CreatedDate.Month == now.Month &&
                    x.CreatedDate.Day == now.Day)
                .FirstOrDefault();

            var accountBalanceForCurrentDayDto =
                dbRepository.Map<AccountBalance, AccountBalanceDto>(accountBalanceForCurrentDay);
            
            if (accountBalanceForCurrentDayDto == null)
            {
                accountBalanceForCurrentDayDto = new AccountBalanceDto
                {
                    AccountId = accountDto.Id,
                    Balance = accountDto.Balance
                };

                dbRepository.Add<AccountBalance, AccountBalanceDto>(accountBalanceForCurrentDayDto);
            }
            else
            {
                accountBalanceForCurrentDayDto.Balance = accountDto.Balance;
                
                dbRepository.Update<AccountBalance, AccountBalanceDto>(accountBalanceForCurrentDayDto);
            }
        }
    }
}