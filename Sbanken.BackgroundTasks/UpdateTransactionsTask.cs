using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hub.HostedServices.Tasks;
using Hub.Storage.Factories;
using Hub.Storage.Providers;
using Hub.Storage.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sbanken.Constants;
using Sbanken.Data.Entities;
using Sbanken.Integration;

namespace Sbanken.BackgroundTasks
{
    public class UpdateTransactionsTask : BackgroundTask
    {
        private readonly ISettingProvider _settingProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISbankenConnector _sbankenConnector;
        private readonly ILogger<UpdateTransactionsTask> _logger;

        public UpdateTransactionsTask(IBackgroundTaskConfigurationProvider backgroundTaskConfigurationProvider,
            IBackgroundTaskConfigurationFactory backgroundTaskConfigurationFactory,
            ISbankenConnector sbankenConnector, 
            ILoggerFactory loggerFactory, 
            IServiceScopeFactory serviceScopeFactory,
            ISettingProvider settingProvider) : base(backgroundTaskConfigurationProvider, backgroundTaskConfigurationFactory)
        {
            _settingProvider = settingProvider;
            _logger = loggerFactory.CreateLogger<UpdateTransactionsTask>();
            _sbankenConnector = sbankenConnector;
            _serviceScopeFactory = serviceScopeFactory;        
        }
        
        
        public override async Task Execute(CancellationToken cancellationToken)
        {
            await UpdateTransactions();
        }

        public async Task UpdateTransactions()
        {
            using var scope = _serviceScopeFactory.CreateScope();

            using var dbRepository = scope.ServiceProvider.GetService<IScopedDbRepository>();
            
            var transactions = dbRepository.GetMany<Transaction>()
                .OrderByDescending(transaction => transaction.CreatedDate)
                .ToList();

            var accounts = dbRepository.GetMany<Account>()
                .ToList();

            if (!accounts.Any()) return;

            SetStartAndEndDate(out var startDate, out var endDate);

            var transactionDtos = await _sbankenConnector.GetTransactions(startDate, endDate);

            transactionDtos = transactionDtos.Where(x => x.AccountingDate.Date >= startDate).ToList();

            _logger.LogInformation($"Found {transactionDtos.Count} transactions");

            var transactionsAdded = 0;

            foreach (var transactionDto in transactionDtos)
            {
                if (transactionDto.IsReservation)
                    continue;

                var accountId = accounts.First(x => x.Name == transactionDto.AccountName)?.Id;

                if (accountId == null)
                    continue;

                if (transactions.Any(x => x.Text == transactionDto.Text && x.TransactionDate == transactionDto.AccountingDate))
                    continue;

                var transactionId = transactionDto.TransactionDetails != null
                    ? transactionDto.TransactionDetails.TransactionId
                    : transactionDto.CardDetails?.TransactionId;

                var transaction = new Transaction
                {
                    Amount = transactionDto.Amount,
                    Text = transactionDto.Text,
                    TransactionType = transactionDto.TransactionTypeCode,
                    TransactionDate = transactionDto.AccountingDate,
                    AccountId = accountId.Value,
                    TransactionId = transactionId
                };

                dbRepository.Add(transaction);
                
                transactionsAdded++;
                
            }
            
            _logger.LogInformation($"Added {transactionsAdded} transactions to DB");

            await dbRepository.SaveChangesAsync();
        }

        private void SetStartAndEndDate(out DateTime startDate, out DateTime? endDate)
        {
            startDate = _settingProvider.GetSetting<DateTime>(SettingConstants.TransactionsStartDate);
            endDate = _settingProvider.GetSetting<DateTime?>(SettingConstants.TransactionsEndDate);

            if (!endDate.HasValue)
            {
                startDate = DateTime.Now.AddDays(-365);
                _logger.LogInformation($"No end date. Setting start date to {startDate.ToShortDateString()} ");
            }
            else if ((endDate.Value - startDate).Days > 365)
            {
                startDate = DateTime.Now.AddDays(-365);
                _logger.LogInformation(
                    $"Start and end date spanned more than 365 days. Setting start date to {startDate.ToShortDateString()} ");
            }


            if (endDate.HasValue)
                _logger.LogInformation(
                    $"Getting transactions made between {startDate.ToShortDateString()}-{endDate.Value.ToShortDateString()}");
            else
                _logger.LogInformation($"Getting transactions made after {startDate.ToShortDateString()}");

            if (startDate.Date < DateTime.Now.Date)
                startDate = startDate.AddDays(1);
        }
    }
}