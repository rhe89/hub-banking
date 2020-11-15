using System;

namespace Sbanken.Core.Dto.Integration
{
    public class SbankenTransaction
    {
        public DateTime AccountingDate { get; set; }
        public int TransactionTypeCode { get; set; }
        public string Text { get; set; }
        public decimal Amount { get; set; }
        public bool IsReservation { get; set; }
        public SbankenTransactionDetail TransactionDetails { get; set; }
        public CardDetail CardDetails { get; set; }

        public string AccountName { get; set; }
    }
}
