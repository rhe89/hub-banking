using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Services.Table;
using Hub.Shared.DataContracts.Banking.Query;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components.Investments;

public class InvestmentsBaseComponent : BaseComponent, IDisposable
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
    
    [Inject] private IAccountProvider AccountProvider { get; set; }
    [Inject] private IScheduledTransactionProvider ScheduledTransactionProvider { get; set; }
    
    protected AccountTypesQuery AccountTypesQuery { get; set; }
    protected decimal LastMonthInvestmentBalance { get; set; }
    protected decimal CurrentInvestmentsBalance { get; set; }
    protected decimal BudgetedDeposits { get; set; }
    protected decimal BudgetedWithdrawals { get; set; }
    protected decimal LastMonthInvestmentsBalance { get; set; }
    protected decimal InvestmentsBalanceDiffInPercentageComparedToPreviousMonth { get; set; }
    protected decimal InvestmentsBalanceComparedToPreviousMonth { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await Search();

        State.OnStateUpdated += OnStateChanged;
    }

    private async void OnStateChanged(object sender, EventArgs args)
    {
        await InvokeAsync(Search);

        await InvokeAsync(StateHasChanged);
    }

    private async Task Search()
    {
        await _semaphore.WaitAsync();
        
        Working = true;

        LastMonthInvestmentBalance = 0;
        CurrentInvestmentsBalance = 0;

        var fromDate = State.GetValidFromDateForMonthAndYear();
        var toDate = State.GetValidToDateForMonthAndYear();
        
        AccountTypesQuery = new AccountTypesQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Investment,
        };
        
        var accountQuery = new AccountQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Investment,
            BalanceToDate = toDate.AddMonths(-1),
            DiscontinuedDate = fromDate.AddMonths(-1)
        };

        var lastMonthsAccountBalances = await AccountProvider.Get(accountQuery);

        LastMonthInvestmentBalance = lastMonthsAccountBalances.Sum(x => x.Balance);

        accountQuery.BalanceToDate = toDate;
        accountQuery.DiscontinuedDate = fromDate;

        CurrentInvestmentsBalance = (await AccountProvider.Get(accountQuery)).Sum(x => x.Balance);

        var scheduledTransactionQuery = new ScheduledTransactionQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Investment,
            NextTransactionToDate = toDate,
            IncludeCompletedTransactions = true
        };

        var budgetedTransactions = await ScheduledTransactionProvider.Get(scheduledTransactionQuery);

        BudgetedDeposits = budgetedTransactions
            .Where(x => x.Amount > 0 &&
                        x.NextTransactionDate >= fromDate &&
                        x.NextTransactionDate <= toDate)
            .Sum(x => x.Amount);
        
        BudgetedWithdrawals = budgetedTransactions
            .Where(x => x.Amount < 0 &&
                        x.NextTransactionDate >= fromDate &&
                        x.NextTransactionDate <= toDate)
            .Sum(x => x.Amount);

        //Include budgeted deposits/withdrawals in future months 
        if (toDate > DateTimeUtils.LastDayOfMonth())
        {
            LastMonthInvestmentBalance += budgetedTransactions
                .Where(x => x.NextTransactionDate < fromDate && !x.Completed)
                .Sum(x => x.Amount);
            
            CurrentInvestmentsBalance += budgetedTransactions
                .Where(x => !x.Completed)
                .Sum(x => x.Amount);
        }
        
        
        if (CurrentInvestmentsBalance == 0)
        {
            Working = false;
            InvestmentsBalanceDiffInPercentageComparedToPreviousMonth = LastMonthInvestmentBalance > 0 ? 100 : 0;
            InvestmentsBalanceComparedToPreviousMonth = CurrentInvestmentsBalance - LastMonthInvestmentBalance;
            return;
        }

        if (LastMonthInvestmentBalance == 0)
        {
            Working = false;
            InvestmentsBalanceDiffInPercentageComparedToPreviousMonth = 100;
            InvestmentsBalanceComparedToPreviousMonth = CurrentInvestmentsBalance;
            return;
        }

        var isGainComparedToPreviousMonth = CurrentInvestmentsBalance >= LastMonthInvestmentsBalance;

        if (isGainComparedToPreviousMonth)
        {
            InvestmentsBalanceDiffInPercentageComparedToPreviousMonth = 100 - (LastMonthInvestmentBalance / CurrentInvestmentsBalance * 100);
            InvestmentsBalanceComparedToPreviousMonth = CurrentInvestmentsBalance - LastMonthInvestmentBalance;
        }
        else
        {
            InvestmentsBalanceDiffInPercentageComparedToPreviousMonth = 100 - (CurrentInvestmentsBalance / LastMonthInvestmentBalance * 100);
            InvestmentsBalanceComparedToPreviousMonth = LastMonthInvestmentBalance - CurrentInvestmentsBalance;
        }

        Working = false;
        
        _semaphore.Release();
    }
    
    public void Dispose()
    {
        State.OnStateUpdated -= OnStateChanged;
    }
}