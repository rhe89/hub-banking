using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.WebApp;

public class State : IDisposable
{
    private readonly IBankProvider _bankProvider;
    private readonly IAccountProvider _accountProvider;
    private long _accountId;
    private int _month;
    private int _year;
    private long _bankId;

    public IList<BankDto> AllBanks { get; set; }
    public IList<BankDto> Banks { get; set; } = new List<BankDto>();
    public IList<AccountDto> AllAccounts { get; set; } = new List<AccountDto>();
    public IList<AccountDto> Accounts { get; set; } = new List<AccountDto>();
    public IList<int> Months { get; set; }
    public IList<int> Years { get; set; }

    public State(IBankProvider bankProvider, IAccountProvider accountProvider)
    {
        _bankProvider = bankProvider;
        _accountProvider = accountProvider;
        
        Months = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        Years = new List<int>();

        OnYearChanged += MonthHasChanged;
        OnMonthChanged += YearHasChanged;
    }

    private async void MonthHasChanged(object sender, EventArgs e)
    {
        await SetAccounts();
        await SetBanks();
    }

    private async void YearHasChanged(object sender, EventArgs e)
    {
        await SetAccounts();
        await SetBanks();
    }

    public async Task InitState()
    {
        for (int year = DateTime.Now.AddYears(3).Year; year >= 2009; year--)
        {
            Years.Add(year);
        }
        
        Years = Years.Reverse().ToList();
        
        Month = DateTime.Now.Month;
        Year = DateTime.Now.Year;

        await SetAccounts();
        await SetBanks();
    }

    public async Task SetAccounts()
    {
        Accounts.Clear();
        
        AllAccounts = await _accountProvider.GetAccounts(new AccountQuery
        {
            BalanceToDate = GetValidFromDateForMonthAndYear(),
            DiscontinuedDate = GetValidToDateForMonthAndYear(),
            IncludeSharedAccounts = true
        });

        Accounts = AllAccounts
            .Where(account => BankId == 0 || account.BankId == BankId)
            .ToList();

        if (Accounts.All(x => x.Id != AccountId))
        {
            AccountId = 0;
        }
        
        OnStateUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public async Task SetBanks()
    {
        AllBanks ??= await _bankProvider.GetBanks();

        Banks = AllBanks
            .Where(bank => Accounts.Any(account => account.BankId == bank.Id))
            .ToList();

        if (Banks.All(x => x.Id != BankId))
        {
            BankId = 0;
        }
        
        OnStateUpdated?.Invoke(this, EventArgs.Empty);
    }

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
            
            Accounts = AllAccounts.Where(x => x.BankId == _bankId).ToList();
            AccountId = 0;
            
            OnStateUpdated?.Invoke(this, EventArgs.Empty);
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
            
            OnStateUpdated?.Invoke(this, EventArgs.Empty);
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

            OnMonthChanged?.Invoke(this, EventArgs.Empty);
            
            OnStateUpdated?.Invoke(this, EventArgs.Empty);
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
            
            OnYearChanged?.Invoke(this, EventArgs.Empty);
            
            OnStateUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public DateTime GetValidFromDateForMonthAndYear()
    {
        DateTime now = DateTime.Now;
        
        if (Month == 0 || Year == 0)
        {
            var year = Year == 0 ? Years[1] : Year;

            var month = Month == 0 ? year == now.Year ? now.Month : Months[1] : Month;
            
            return DateTimeUtils.FirstDayOfMonth(year, month);
        }
        
        return DateTimeUtils.FirstDayOfMonth(Year, Month);
    }

    public DateTime GetValidToDateForMonthAndYear()
    {
        DateTime now = DateTime.Now;
        
        if (Month == 0 || Year == 0)
        {
            var year = Year == 0 ? now.Year : Year;

            var month = Month == 0 ? year == now.Year ? now.Month : Months.Last() : Month;
            
            return DateTimeUtils.LastDayOfMonth(year, month);
        }
        
        return DateTimeUtils.LastDayOfMonth(Year, Month);
    }

    public EventHandler OnStateUpdated { get; set; }
    public EventHandler OnMonthChanged { get; set; }
    public EventHandler OnYearChanged { get; set; }
    
    public void Dispose()
    {
        OnYearChanged -= MonthHasChanged;
        OnMonthChanged -= YearHasChanged;
    }
}