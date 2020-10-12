using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hub.HostedServices.Tasks;
using Hub.Storage.Factories;
using Hub.Storage.Providers;
using Hub.Storage.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sbanken.Data.Entities;
using Sbanken.Dto.Sbanken;
using Sbanken.Integration;

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

        public async Task FetchAndUpdateCurrentAccountBalances()
        {
            var accountDtos = await FetchCurrentAccountBalances();

            await UpdateCurrentAccountBalances(accountDtos);
        }

        private async Task<IList<AccountDto>> FetchCurrentAccountBalances()
        {
            var success = false;
            var retryCount = 0;
            Exception ex = null;
            var accounts = new List<AccountDto>();

            _logger.LogInformation("Fetching accounts from Sbanken.");

            while (!success && retryCount < 100)
            {
                try
                {
                    accounts = await _sbankenConnector.GetAccounts();

                    success = true;
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
                throw (ex);

            _logger.LogInformation($"Finished fetching accounts from Sbanken. Found {accounts.Count} accounts.");

            return accounts;
        }


        private async Task UpdateCurrentAccountBalances(IList<AccountDto> accountDtos)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            using var dbRepository = scope.ServiceProvider.GetService<IScopedDbRepository>();
            
            var existingAccounts = dbRepository.GetMany<Account>().ToList();

            foreach (var accountDto in accountDtos)
            {
                var accountInDb = existingAccounts.FirstOrDefault(x => x.Name == accountDto.Name);

                if (accountInDb == null)
                    CreateAccount(accountDto, dbRepository);
                else
                    UpdateAccount(accountInDb, accountDto, dbRepository);

                UpdateAccountBalanceHistory(accountInDb, dbRepository);
            }
            
            await dbRepository.SaveChangesAsync();
        }

        
        private void CreateAccount(AccountDto accountDto, IDbRepository dbRepository)
        {
            _logger.LogInformation($"Adding new account {accountDto.Name}");
            var account = new Account
            {
                Name = accountDto.Name,
                CurrentBalance = accountDto.Available,
                AccountType = accountDto.AccountType
            };

            dbRepository.Add(account);
        }

        private void UpdateAccount(Account accountInDb, AccountDto accountDto, IScopedDbRepository dbRepository)
        {
            _logger.LogInformation($"Updating account {accountDto.Name}");

            accountInDb.CurrentBalance = accountDto.Available;

            if (accountInDb.AccountType != accountDto.AccountType)
                accountInDb.AccountType = accountDto.AccountType;
                
            
            dbRepository.Update(accountInDb);
        }
        
        private void UpdateAccountBalanceHistory(Account accountInDb, IScopedDbRepository dbRepository)
        {
            var now = DateTime.Now;

            var accountBalanceForCurrentDay = dbRepository.GetMany<AccountBalance>(x =>
                x.AccountId == accountInDb.Id &&
                x.CreatedDate.Year == now.Year &&
                x.CreatedDate.Month == now.Month &&
                x.CreatedDate.Day == now.Day)
                .FirstOrDefault();

            if (accountBalanceForCurrentDay == null)
            {
                accountBalanceForCurrentDay = new AccountBalance
                {
                    AccountId = accountInDb.Id,
                    Balance = accountInDb.CurrentBalance
                };

                dbRepository.Add(accountBalanceForCurrentDay);
            }
            else
            {
                accountBalanceForCurrentDay.Balance = accountInDb.CurrentBalance;
                
                dbRepository.Update(accountBalanceForCurrentDay);
            }
        }
    }
}