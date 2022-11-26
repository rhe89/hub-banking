using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Services.Table;
using Hub.Shared.DataContracts.Banking.Query;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components.AccountTypes;

public class SavingsBaseComponent : BaseComponent, IDisposable
{
    [Inject] private IAccountProvider AccountProvider { get; set; }
    [Inject] private IScheduledTransactionProvider ScheduledTransactionProvider { get; set; }
    [Inject] private ITransactionProvider TransactionProvider { get; set; }

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
    protected bool IsGainComparedToPreviousMonth { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        
        await Search();

        State.QueryParametersChanged += OnStateOnStateChanged;
    }

    private async void OnStateOnStateChanged(object sender, EventArgs args)
    {
        await Search();

        await InvokeAsync(StateHasChanged);
    }

    private async Task Search()
    {
        Working = true;
        
        AccountTypesQuery = new AccountTypesQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
        };

        TransactionQuery = new TransactionQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
            FromDate = DateTimeUtils.FirstDayOfMonth(State.Year, State.Month),
            ToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month)
        };

        var accountQuery = new AccountQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
            BalanceToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month).AddMonths(-1),
            DiscontinuedDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month).AddMonths(-1)
        };

        var lastMonthsAccountBalances = await AccountProvider.GetAccounts(accountQuery);

        LastMonthSavingsBalance = lastMonthsAccountBalances.Sum(x => x.Balance);
        
        accountQuery.BalanceToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
        accountQuery.DiscontinuedDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
        
        CurrentSavingsBalance = (await AccountProvider.GetAccounts(accountQuery)).Sum(x => x.Balance);

        var scheduledTransactionQuery = new ScheduledTransactionQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Saving,
            NextTransactionFromDate = DateTimeUtils.FirstDayOfMonth(State.Year, State.Month),
            NextTransactionToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month),
            IncludeCompletedTransactions = true
        };

        var budgetedTransactions = await ScheduledTransactionProvider.GetScheduledTransactions(scheduledTransactionQuery);

        BudgetedDeposits = budgetedTransactions.Where(x => x.Amount > 0).Sum(x => x.Amount);
        BudgetedWithdrawals = budgetedTransactions.Where(x => x.Amount < 0).Sum(x => x.Amount);

        var transactions = await TransactionProvider.GetTransactions(TransactionQuery);

        ActualDeposits = transactions.Where(x => x.Amount > 0).Sum(x => x.Amount);
        ActualWithdrawals = transactions.Where(x => x.Amount < 0).Sum(x => x.Amount);
        
        IsGainComparedToPreviousMonth = CurrentSavingsBalance >= LastMonthSavingsBalance;

        if (CurrentSavingsBalance == 0)
        {
            Working = false;
            SavingsBalanceDiffInPercentageComparedToPreviousMonth = LastMonthSavingsBalance > 0 ? 100 : 0;
            IsGainComparedToPreviousMonth = LastMonthSavingsBalance == 0;
            SavingsBalanceComparedToPreviousMonth = CurrentSavingsBalance - LastMonthSavingsBalance;
            return;
        }

        if (LastMonthSavingsBalance == 0)
        {
            Working = false;
            SavingsBalanceDiffInPercentageComparedToPreviousMonth = 100;
            IsGainComparedToPreviousMonth = true;
            SavingsBalanceComparedToPreviousMonth = CurrentSavingsBalance;
            return;
        }
        
        if (IsGainComparedToPreviousMonth)
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
    }
    
    public void Dispose()
    {
        State.QueryParametersChanged -= OnStateOnStateChanged;
    }
}