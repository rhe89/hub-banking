using System;
using System.Runtime.Serialization;

namespace Banking.Integration.Dto;

[DataContract]
public class SbankenTransaction
{
    [DataMember]
    public DateTime AccountingDate { get; set; }
    
    [DataMember]
    public DateTime InterestDate { get; set; }
        
    [DataMember]
    public string ArchiveReference { get; set; }
        
    [DataMember]
    public string RecipientAccountNumber { get; set; }
        
    [DataMember]
    public string Type { get; set; }
        
    [DataMember]
    public string Text { get; set; }
    
    [DataMember]
    public decimal? AmountOut { get; set; }
    
    [DataMember]
    public decimal? AmountIn { get; set; }
}