using System.Runtime.Serialization;

namespace Sbanken.Integration.Dto;

[DataContract]
public class SbankenTransactionDetail
{
    [DataMember]
    public string TransactionId { get; set; }
}