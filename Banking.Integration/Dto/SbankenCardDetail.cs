using System.Runtime.Serialization;

namespace Banking.Integration.Dto;

[DataContract]
public class CardDetail
{
    [DataMember]
    public string TransactionId { get; set; }
}