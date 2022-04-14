using System.Runtime.Serialization;

namespace Banking.Integration.Dto;

[DataContract]
public class SbankenTransactionDetail
{
    [DataMember]
    public string TransactionId { get; set; }
}