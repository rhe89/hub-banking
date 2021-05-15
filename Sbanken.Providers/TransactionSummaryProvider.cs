using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sbanken.Core.Dto.Api;
using Sbanken.Core.Dto.Data;
using Sbanken.Core.Providers;

namespace Sbanken.Providers
{
    public class TransactionSummaryProvider : ITransactionSummaryProvider
    {
        private readonly ITransactionProvider _transactionProvider;

        public TransactionSummaryProvider(ITransactionProvider transactionProvider)
        {
            _transactionProvider = transactionProvider;
        }

        public async Task<TransactionSummaryDto> GetMikrosparTransactions()
        {
            var transactions = await _transactionProvider.GetTransactionsWithText("Mikrospar");

            transactions = transactions.Where(x => x.Amount > 0).ToList();
            
            return GetTransactionSummaryDto(transactions);
        }

        public async Task<TransactionSummaryDto> GetInvestmentTransactions()
        {
            var transactions = await _transactionProvider.GetTransactionsWithText("Verdipapirhandel");

            transactions.ToList().ForEach(x => x.Amount = -x.Amount);
            
            return GetTransactionSummaryDto(transactions);
        }

        private TransactionSummaryDto GetTransactionSummaryDto(IList<TransactionDto> transactions)
        {
            var currentMonthTransactions =
                GetTransactionsForPeriodType(PeriodType.Month, transactions);
            var currentYearTransactions = GetTransactionsForPeriodType(PeriodType.Year, transactions);
            var total = transactions.Sum(x => x.Amount);

            return new TransactionSummaryDto
            {
                TotalAmount = total,
                CurrentMonthTransactions = currentMonthTransactions,
                CurrentYearTransactions = currentYearTransactions
            };
        }

        private TransactionPeriodDto GetTransactionsForPeriodType(PeriodType periodType, IEnumerable<TransactionDto> transactions)
        {
            var savingPeriods = new SortedList<Period, TransactionPeriodDto>();

            foreach (var transaction in transactions)
            {
                var periodKey = GetPeriod(periodType, transaction.TransactionDate);

                AddOrUpdatePeriod(periodKey, transaction, savingPeriods);
            }

            var currentPeriod = GetPeriod(periodType, DateTime.Now);

            var hasTransactionsInCurrentPeriod = savingPeriods.TryGetValue(currentPeriod, out var currentTransactionPeriod);

            if (!hasTransactionsInCurrentPeriod)
            {
                return null;
            }
            
            var previousPeriodDate = currentPeriod.PreviousPeriodDate();

            var previousPeriod = GetPeriod(periodType, previousPeriodDate);

            var hasTransactionsInPreviousPeriod = savingPeriods.TryGetValue(previousPeriod, out var previousTransactionPeriod);

            if (!hasTransactionsInPreviousPeriod)
            {
                return currentTransactionPeriod;
            }

            currentTransactionPeriod.PreviousPeriodAmount = previousTransactionPeriod.Amount;
            currentTransactionPeriod.PreviousPeriodTransactions = previousTransactionPeriod.Transactions;

            return currentTransactionPeriod;
        }

        private void AddOrUpdatePeriod(Period period, TransactionDto transaction, IDictionary<Period, TransactionPeriodDto> periodDictionary)
        {
            var periodExist = periodDictionary.TryGetValue(period, out _);

            if (periodExist)
            {
                periodDictionary[period].Amount += transaction.Amount;
                periodDictionary[period].Transactions.Add(transaction);
            }
            else
            {
                periodDictionary.Add(period, new TransactionPeriodDto
                {
                    Period = period,
                    Amount = transaction.Amount,
                    Transactions = new List<TransactionDto> { transaction }
                });
            }
        }

        private static Period GetPeriod(PeriodType periodType, DateTime date)
        {
            return periodType switch
            {
                PeriodType.Month => new Period(date.Year, date.Month),
                PeriodType.Year => new Period(date.Year),
                _ => throw new ArgumentOutOfRangeException(nameof(periodType), periodType, null)
            };
        }
    }
}