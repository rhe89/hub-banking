using System;
using System.Collections.Generic;

namespace Banking.Web.WebApp;

public class State
{
    private long _accountId;
    private int _month;
    private int _year;
    private long _bankId;
    public IList<int> Months { get; set; }
    public IList<int> Years { get; set; }

    public long BankId
    {
        get => _bankId;
        set
        {
            if (_bankId == value)
            {
                return;
            }
            
            _bankId = value;
            QueryParametersChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public long? NullableBankId
    {
        get => _bankId;
        set
        {
            if (_bankId == value || value == null)
            {
                return;
            }

            BankId = value.Value;
        }
    }

    public long AccountId
    {
        get => _accountId;
        set
        {
            if (_accountId == value)
            {
                return;
            }
            
            _accountId = value;
            QueryParametersChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Month
    {
        get => _month;
        set
        {
            if (_month == value)
            {
                return;
            }
            
            _month = value;
            QueryParametersChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public int Year
    {
        get => _year;
        set
        {
            if (_year == value)
            {
                return;
            }
            
            _year = value;
            QueryParametersChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public EventHandler QueryParametersChanged { get; set; }
}