using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Services.Table;
using Hub.Shared.DataContracts.Banking.Query;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components.Savings;

public class SavingsBaseComponent : BaseComponent, IDisposable
{
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

    [Inject] private IAccountProvider AccountProvider { get; set; }
    [Inject] private IScheduledTransactionProvider ScheduledTransactionProvider { get; set; }
    [Inject] private ITransactionProvider TransactionProvider { get; set; }
    [Inject] private IMonthlyBudgetProvider MonthlyBudgetProvider { get; set; }

    protected TransactionQuery TransactionQuery { get; set; }
    protected AccountTypesQuery AccountTypesQuery { get; set; }
    protected decimal CurrentSavingsBalance { get; set; }
    protected decimal BudgetedDeposits { get; set; }
    protected decimal ActualDeposits { get; set; }
    protected decimal BudgetedWithdrawals { get; set; }
    protected decimal ActualWithdrawals { get; set; }
    protected decimal LastMonthSavingsBalance { get; set; }
    protected decimal SavingsBalanceDiffInPercentageComparedToPreviousMonth { get; set; }
    protected decimal SavingsBalanceComparedToPreviousMonth { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await Search();

        State.OnStateUpdated += OnStateOnStateChanged;
    }

    private async void OnStateOnStateChanged(object sender, EventArgs args)
    {
        await Search();

        await InvokeAsync(StateHasChanged);
    }

    private async Task Search()
    {
        await _semaphore.WaitAsync();
        
        Working = true;
        
        AccountTypesQuery = new AccountTypesQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
        };
        
        var fromDate = State.GetValidFromDateForMonthAndYear();
        var toDate = State.GetValidToDateForMonthAndYear();

        TransactionQuery = new TransactionQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
            FromDate = fromDate,
            ToDate = toDate
        };

        var accountQuery = new AccountQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
            BalanceToDate = toDate.AddMonths(-1),
            DiscontinuedDate = fromDate.AddMonths(-1)
        };

        var lastMonthsAccountBalances = await AccountProvider.Get(accountQuery);

        LastMonthSavingsBalance = lastMonthsAccountBalances.Sum(x => x.Balance);
        
        accountQuery.BalanceToDate = toDate;
        accountQuery.DiscontinuedDate = fromDate;
        
        CurrentSavingsBalance = (await AccountProvider.Get(accountQuery)).Sum(x => x.Balance);

        var scheduledTransactionQuery = new ScheduledTransactionQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
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
            LastMonthSavingsBalance += budgetedTransactions
                .Where(x => x.NextTransactionDate < fromDate && !x.Completed)
                .Sum(x => x.Amount);
            
            CurrentSavingsBalance += budgetedTransactions
                .Where(x => !x.Completed)
                .Sum(x => x.Amount);
        }
        
        var transactions = await TransactionProvider.Get(TransactionQuery);

        ActualDeposits = transactions.Where(x => x.Amount > 0).Sum(x => x.Amount);
        ActualWithdrawals = transactions.Where(x => x.Amount < 0).Sum(x => x.Amount);
        
        if (CurrentSavingsBalance == 0)
        {
            Working = false;
            SavingsBalanceDiffInPercentageComparedToPreviousMonth = LastMonthSavingsBalance > 0 ? 100 : 0;
            SavingsBalanceComparedToPreviousMonth = CurrentSavingsBalance - LastMonthSavingsBalance;
            return;
        }

        if (LastMonthSavingsBalance == 0)
        {
            Working = false;
            SavingsBalanceDiffInPercentageComparedToPreviousMonth = 100;
            SavingsBalanceComparedToPreviousMonth = CurrentSavingsBalance;
            return;
        }
        
        var isGainComparedToPreviousMonth = CurrentSavingsBalance >= LastMonthSavingsBalance;

        if (isGainComparedToPreviousMonth)
        {
            SavingsBalanceDiffInPercentageComparedToPreviousMonth = 100 - (LastMonthSavingsBalance / CurrentSavingsBalance * 100);
            SavingsBalanceComparedToPreviousMonth = CurrentSavingsBalance - LastMonthSavingsBalance;
        }
        else
        {
            SavingsBalanceDiffInPercentageComparedToPreviousMonth = 100 - (CurrentSavingsBalance / LastMonthSavingsBalance * 100);
            SavingsBalanceComparedToPreviousMonth = LastMonthSavingsBalance - CurrentSavingsBalance;
        }

        Working = false;

        _semaphore.Release();
    }
    
    public void Dispose()
    {
        State.OnStateUpdated -= OnStateOnStateChanged;
    }
}