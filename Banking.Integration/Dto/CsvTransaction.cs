using System;
using System.Runtime.Serialization;

namespace Banking.Integration.Dto;

public class CsvTransaction
{
    [DataMember]
    public DateTime TransactionDate { get; set; }
        
    [DataMember]
    public decimal? AmountIn { get; set; }
        
    [DataMember]
    public decimal? AmountOut { get; set; }

    [DataMember]
    public string ToAccountName { get; set; }
        
    [DataMember]
    public string ToAccountNumber { get; set; }
        
    [DataMember]
    public string FromAccountName { get; set; }
        
    [DataMember]
    public string FromAccountNumber { get; set; }
        
    [DataMember]
    public string Type { get; set; }
        
    [DataMember]
    public string Text { get; set; }
        
    [DataMember]
    public string Kid { get; set; }
        
    [DataMember]
    public string Category { get; set; }

    [DataMember]
    public string SubCategory { get; set; }
}