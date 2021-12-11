using System.Runtime.Serialization;

namespace Sbanken.Integration.Dto
{
    [DataContract]
    public class CardDetail
    {
        [DataMember]
        public string TransactionId { get; set; }
    }
}