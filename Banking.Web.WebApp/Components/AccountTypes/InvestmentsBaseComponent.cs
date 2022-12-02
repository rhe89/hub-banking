using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Services.Table;
using Hub.Shared.DataContracts.Banking.Query;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components.AccountTypes;

public class InvestmentsBaseComponent : BaseComponent, IDisposable
{
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
    protected bool IsGainComparedToPreviousMonth { get; set; }
    
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
        Working = true;
        
        AccountTypesQuery = new AccountTypesQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Investment,
        };
        
        var accountQuery = new AccountQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Investment,
            BalanceToDate = State.GetValidToDateForMonthAndYear().AddMonths(-1),
            DiscontinuedDate = State.GetValidFromDateForMonthAndYear().AddMonths(-1)
        };

        var lastMonthsAccountBalances = await AccountProvider.GetAccounts(accountQuery);

        LastMonthInvestmentBalance = lastMonthsAccountBalances.Sum(x => x.Balance);

        accountQuery.BalanceToDate = State.GetValidToDateForMonthAndYear();
        accountQuery.DiscontinuedDate = State.GetValidFromDateForMonthAndYear();

        CurrentInvestmentsBalance = (await AccountProvider.GetAccounts(accountQuery)).Sum(x => x.Balance);

        var scheduledTransactionQuery = new ScheduledTransactionQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Investment,
            NextTransactionFromDate = State.GetValidFromDateForMonthAndYear(),
            NextTransactionToDate = State.GetValidToDateForMonthAndYear(),
            IncludeCompletedTransactions = true
        };

        var budgetedTransactions = await ScheduledTransactionProvider.GetScheduledTransactions(scheduledTransactionQuery);

        BudgetedDeposits = budgetedTransactions.Where(x => x.Amount > 0).Sum(x => x.Amount);
        BudgetedWithdrawals = budgetedTransactions.Where(x => x.Amount < 0).Sum(x => x.Amount);

        IsGainComparedToPreviousMonth = CurrentInvestmentsBalance >= LastMonthInvestmentsBalance;
        
        if (CurrentInvestmentsBalance == 0)
        {
            Working = false;
            InvestmentsBalanceDiffInPercentageComparedToPreviousMonth = LastMonthInvestmentBalance > 0 ? 100 : 0;
            IsGainComparedToPreviousMonth = LastMonthInvestmentBalance == 0;
            InvestmentsBalanceComparedToPreviousMonth = CurrentInvestmentsBalance - LastMonthInvestmentBalance;
            return;
        }

        if (LastMonthInvestmentBalance == 0)
        {
            Working = false;
            InvestmentsBalanceDiffInPercentageComparedToPreviousMonth = 100;
            IsGainComparedToPreviousMonth = true;
            InvestmentsBalanceComparedToPreviousMonth = CurrentInvestmentsBalance;
            return;
        }

        if (IsGainComparedToPreviousMonth)
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
    }
    
    public void Dispose()
    {
        State.OnStateUpdated -= OnStateChanged;
    }
}