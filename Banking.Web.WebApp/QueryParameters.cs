using System;

namespace Banking.Web.WebApp;

public class QueryParameters
{
    public long AccountId { get; set; }
    public long BankId { get; set; }
    public long Month { get; set; }
    public long Year { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}