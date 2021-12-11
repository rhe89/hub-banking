using System;
using System.Runtime.Serialization;

namespace Sbanken.Integration.Dto
{
    [DataContract]
    public class SbankenTransaction
    {
        [DataMember]
        public DateTime AccountingDate { get; set; }
        
        [DataMember]
        public string TransactionId { get; set; }
        
        [DataMember]
        public int TransactionTypeCode { get; set; }
        
        [DataMember]
        public string Text { get; set; }
        
        [DataMember]
        public decimal Amount { get; set; }
        
        [DataMember]
        public SbankenTransactionDetail TransactionDetails { get; set; }
        
        [DataMember]
        public CardDetail CardDetails { get; set; }

        [DataMember]
        public string AccountName { get; set; }
    }
}
