using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Utilities;

namespace Banking.Web.WebApp;

public class BankingState : IDisposable
{
    private readonly IBankProvider _bankProvider;
    private readonly IAccountProvider _accountProvider;
    private MonthInYear _monthInYear;
    private long _accountId;
    private long _bankId;

    public BankingState(IBankProvider bankProvider, IAccountProvider accountProvider)
    {
        _bankProvider = bankProvider;
        _accountProvider = accountProvider;
        
        OnMonthInYearChanged += MonthInYearHasChanged;
    }

    public IList<MonthInYear> MonthsInYears { get; set; }
    public IList<BankDto> AllBanks { get; set; }
    public IList<BankDto> Banks { get; set; } = new List<BankDto>();
    public IList<AccountDto> AllAccounts { get; set; } = new List<AccountDto>();
    public IList<AccountDto> Accounts { get; set; } = new List<AccountDto>();

    public EventHandler OnStateUpdated { get; set; }
    public EventHandler OnMonthInYearChanged { get; set; }

    public new async Task InitState()
    {
        MonthsInYears = new List<MonthInYear>();

        var months = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        for (int year = 2009; year <= DateTimeUtils.Today.AddYears(3).Year; year++)
        {
            foreach (var month in months)
            {
                MonthsInYears.Add(new MonthInYear(month, year));
            }
        }

        MonthInYear = MonthsInYears.First(x => x.Month == DateTimeUtils.Today.Month && x.Year == DateTime.Now.Year);
        
        await SetAccounts();
        await SetBanks();
    }

    public async Task SetAccounts()
    {
        Accounts.Clear();
        
        AllAccounts = await _accountProvider.Get(new AccountQuery
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
        AllBanks ??= await _bankProvider.Get();

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
            
            Accounts = AllAccounts.Where(x => BankId == 0 || x.BankId == _bankId).ToList();
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
    
    public MonthInYear MonthInYear
    {
        get => _monthInYear;
        set
        {
            if (_monthInYear == value)
            {
                return;
            }
            
            _monthInYear = value;

            OnMonthInYearChanged?.Invoke(this, EventArgs.Empty);
            
            OnStateUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public DateTime GetValidFromDateForMonthAndYear()
    {
        if (MonthInYear.Month == 0 && MonthInYear.Year == 0)
        {
            var today = DateTimeUtils.Today;

            return DateTimeUtils.FirstDayOfMonth(today.Year, today.Month);
        }
        
        return DateTimeUtils.FirstDayOfMonth(MonthInYear.Year, MonthInYear.Month);
    }

    public DateTime GetValidToDateForMonthAndYear()
    {
        if (MonthInYear.Month == 0 && MonthInYear.Year == 0)
        {
            var today = DateTimeUtils.Today;

            return DateTimeUtils.LastDayOfMonth(today.Year, today.Month);
        }
        
        return DateTimeUtils.LastDayOfMonth(MonthInYear.Year, MonthInYear.Month);
    }
    
    public void Dispose()
    {
        OnMonthInYearChanged -= MonthInYearHasChanged;
    }
    
    private async void MonthInYearHasChanged(object sender, EventArgs e)
    {
        await SetAccounts();
        await SetBanks();
    }
}

public class MonthInYear
{
    public int Month { get; }
    
    public int Year { get; }

    public MonthInYear(int month, int year)
    {
        Month = month;
        Year = year;
    }

    public string DisplayText => $"{TextUtils.GetMonthText(Month)} {Year}";
}