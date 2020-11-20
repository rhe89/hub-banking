using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hub.HostedServices.Tasks;
using Hub.Storage.Core.Factories;
using Hub.Storage.Core.Providers;
using Hub.Storage.Core.Repository;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Constants;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;
using Sbanken.Core.Integration;

namespace Sbanken.BackgroundTasks
{
    public class UpdateTransactionsTask : BackgroundTask
    {
        private readonly ISettingProvider _settingProvider;
        private readonly IHubDbRepository _dbRepository;
        private readonly ISbankenConnector _sbankenConnector;
        private readonly ILogger<UpdateTransactionsTask> _logger;

        public UpdateTransactionsTask(IBackgroundTaskConfigurationProvider backgroundTaskConfigurationProvider,
            IBackgroundTaskConfigurationFactory backgroundTaskConfigurationFactory,
            ISbankenConnector sbankenConnector, 
            ILoggerFactory loggerFactory, 
            ISettingProvider settingProvider,
            IHubDbRepository dbRepository) : base(backgroundTaskConfigurationProvider, backgroundTaskConfigurationFactory)
        {
            _settingProvider = settingProvider;
            _dbRepository = dbRepository;
            _logger = loggerFactory.CreateLogger<UpdateTransactionsTask>();
            _sbankenConnector = sbankenConnector;
        }
        
        
        public override async Task Execute(CancellationToken cancellationToken)
        {
            await UpdateTransactions();
        }

        private async Task UpdateTransactions()
        {
            _dbRepository.ToggleDispose(false);
                
            var transactions = _dbRepository.All<Transaction, TransactionDto>()
                .OrderByDescending(transaction => transaction.TransactionDate)
                .ToList();

            var accounts = _dbRepository.All<Account, AccountDto>()
                .ToList();

            if (!accounts.Any())
            {
                return;
            }

            SetStartAndEndDate(out var startDate, out var endDate);

            var transactionDtos = await _sbankenConnector.GetTransactions(startDate, endDate);

            transactionDtos = transactionDtos.Where(x => x.AccountingDate.Date >= startDate).ToList();

            _logger.LogInformation($"Found {transactionDtos.Count} transactions");

            var transactionsAdded = 0;

            foreach (var transactionDto in transactionDtos)
            {
                if (transactionDto.IsReservation)
                {
                    continue;
                }

                var accountId = accounts.First(x => x.Name == transactionDto.AccountName)?.Id;

                if (accountId == null)
                {
                    continue;
                }

                if (transactions.Any(x => x.Name == transactionDto.Text && x.TransactionDate == transactionDto.AccountingDate))
                {
                    continue;
                }

                var transactionId = transactionDto.TransactionDetails != null
                    ? transactionDto.TransactionDetails.TransactionId
                    : transactionDto.CardDetails?.TransactionId;

                var transaction = new TransactionDto
                {
                    Amount = transactionDto.Amount,
                    Name = transactionDto.Text,
                    TransactionType = transactionDto.TransactionTypeCode,
                    TransactionDate = transactionDto.AccountingDate,
                    AccountId = accountId.Value,
                    TransactionIdentifier = transactionId
                };

                _dbRepository.Add<Transaction, TransactionDto>(transaction);
                
                transactionsAdded++;
            }
            
            _logger.LogInformation($"Added {transactionsAdded} transactions to DB");
            
            _dbRepository.ToggleDispose(true);

            await _dbRepository.SaveChangesAsync();
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

            var logMessage = $"Getting transactions made after {startDate.ToShortDateString()}";
            
            
            if (endDate.HasValue)
            {
                logMessage = $"Getting transactions made between {startDate.ToShortDateString()}-{endDate.Value.ToShortDateString()}";
            }
                
            _logger.LogInformation(logMessage);

            if (startDate.Date < DateTime.Now.Date)
            {
                startDate = startDate.AddDays(1);
            }
        }
    }
}