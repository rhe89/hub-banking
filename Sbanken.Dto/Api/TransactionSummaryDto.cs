using System;
using System.Collections.Generic;

namespace Sbanken.Dto.Api
{
    public class TransactionSummaryDto
    {
        public decimal TotalAmount { get; set; }
        public TransactionPeriodDto CurrentMonthTransactions { get; set; }
        public TransactionPeriodDto CurrentYearTransactions { get; set; }
    }

    public class TransactionPeriodDto
    {
        public Period Period { get; set; }
        public decimal Amount { get; set; }
        public decimal PreviousPeriodAmount { get; set; }
        public IList<TransactionDto> Transactions { get; set; }
        public IList<TransactionDto> PreviousPeriodTransactions { get; set; }
    }

    public class Period : IComparable<Period>
    {
        private readonly PeriodType _periodType;
        private readonly DateTime _periodDate;
        
        public Period(int year, int month)
        {
            _periodType = PeriodType.Month;
            _periodDate = new DateTime(year, month, 1);
        }

        public Period(int year)
        {
            _periodType = PeriodType.Year;
            _periodDate = new DateTime(year, 1, 1);
        }

        public DateTime PreviousPeriodDate()
        {
            return _periodType switch
            {
                PeriodType.Month => _periodDate.AddMonths(-1),
                PeriodType.Year => _periodDate.AddYears(-1),
                _ => throw new ArgumentOutOfRangeException(nameof(PeriodType))
            };
        }

        public int CompareTo(Period other)
        {
            if (_periodDate > other._periodDate)
                return 1;
            if (_periodDate < other._periodDate)
                return -1;

            return 0;
        }
    }

}