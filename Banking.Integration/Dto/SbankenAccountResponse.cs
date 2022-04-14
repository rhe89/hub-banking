using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Banking.Integration.Dto;

[DataContract]
public class SbankenAccountResponse
{
    [DataMember]
    public List<SbankenAccount> Items { get; set; }

    [DataMember]
    public bool IsError { get; set; }
        
    [DataMember]
    public string ErrorMessage { get; set; }
}