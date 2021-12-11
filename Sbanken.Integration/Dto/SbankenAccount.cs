using System.Runtime.Serialization;

namespace Sbanken.Integration.Dto
{
    [DataContract]
    public class SbankenAccount
    {
        [DataMember]
        public string AccountId { get; set; }
        
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public decimal Available { get; set; }
        
        [DataMember]
        public decimal Balance { get; set; }
        
        [DataMember]
        public string AccountType { get; set; }
    }
}
