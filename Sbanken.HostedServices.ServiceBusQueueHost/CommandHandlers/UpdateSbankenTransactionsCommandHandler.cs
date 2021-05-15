using System;
using System.Linq;
using System.Threading.Tasks;
using Hub.Settings.Core;
using Hub.Storage.Repository.Core;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Constants;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Entities;
using Sbanken.Core.Integration;

namespace Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers
{
    public class UpdateSbankenTransactionsCommandHandler : IUpdateSbankenTransactionsCommandHandler
    {
        private readonly ISettingProvider _settingProvider;
        private readonly IHubDbRepository _dbRepository;
        private readonly ISbankenConnector _sbankenConnector;
        private readonly ILogger<UpdateSbankenTransactionsCommandHandler> _logger;

        public UpdateSbankenTransactionsCommandHandler(
            ISbankenConnector sbankenConnector, 
            ILogger<UpdateSbankenTransactionsCommandHandler> logger, 
            ISettingProvider settingProvider,
            IHubDbRepository dbRepository)
        {
            _settingProvider = settingProvider;
            _dbRepository = dbRepository;
            _logger = logger;
            _sbankenConnector = sbankenConnector;
        }

       public async Task UpdateTransactions()
        {
            var transactionsInDb = _dbRepository.All<Transaction, TransactionDto>()
                .OrderByDescending(transaction => transaction.TransactionDate)
                .ToList();

            var accounts = _dbRepository.All<Account, AccountDto>()
                .ToList();

            if (!accounts.Any())
            {
                return;
            }

            SetStartAndEndDate(out var startDate, out var endDate);

            var transactionsFromSbanken = await _sbankenConnector.GetTransactions(startDate, endDate);

            transactionsFromSbanken = transactionsFromSbanken.Where(x => x.AccountingDate.Date >= startDate).ToList();

            _logger.LogInformation($"Found {transactionsFromSbanken.Count} transactions");

            var transactionsAdded = 0;

            foreach (var transactionFromSbanken in transactionsFromSbanken)
            {
                if (transactionFromSbanken.IsReservation)
                {
                    continue;
                }

                var accountId = accounts.First(x => x.Name == transactionFromSbanken.AccountName)?.Id;

                if (accountId == null)
                {
                    continue;
                }

                var transactionId = transactionFromSbanken.TransactionDetails != null
                    ? transactionFromSbanken.TransactionDetails.TransactionId
                    : transactionFromSbanken.CardDetails?.TransactionId;
                
                if (transactionsInDb.Any(x => x.Name == transactionFromSbanken.Text && x.TransactionDate == transactionFromSbanken.AccountingDate))
                {
                    continue;
                }

                

                var transaction = new TransactionDto
                {
                    Amount = transactionFromSbanken.Amount,
                    Name = transactionFromSbanken.Text,
                    TransactionType = transactionFromSbanken.TransactionTypeCode,
                    TransactionDate = transactionFromSbanken.AccountingDate,
                    AccountId = accountId.Value,
                    TransactionIdentifier = transactionId
                };

                _dbRepository.QueueAdd<Transaction, TransactionDto>(transaction);
                
                transactionsAdded++;
            }
            
            await _dbRepository.ExecuteQueueAsync();
            
            _logger.LogInformation($"Added {transactionsAdded} transactions to DB");
        }

        private void SetStartAndEndDate(out DateTime startDate, out DateTime? endDate)
        {
            startDate = _settingProvider.GetSetting<DateTime>(SettingKeys.TransactionsStartDate);
            endDate = _settingProvider.GetSetting<DateTime?>(SettingKeys.TransactionsEndDate);
    
            if (!endDate.HasValue)
            {
                startDate = DateTime.Now.AddDays(-30);
                _logger.LogInformation($"No end date. Setting start date to {startDate.ToShortDateString()} ");
            }
            else if ((endDate.Value - startDate).Days > 365)
            {
                startDate = DateTime.Now.AddDays(-30);
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