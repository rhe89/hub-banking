using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Query;
using Humanizer;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components.AccountTypes;

public class BillingBaseComponent : BaseComponent, IDisposable
{
    [Inject] private IScheduledTransactionProvider ScheduledTransactionProvider { get; set; }
    [Inject] private ITransactionProvider TransactionsProvider { get; set; }
    [Inject] private IAccountProvider AccountProvider { get; set; }
    [Inject] private IAccountBalanceProvider AccountBalanceProvider { get; set; }
    
    protected TransactionQuery PaidBillsQuery { get; set; }
    protected ScheduledTransactionQuery UpcomingBillsQuery { get; set; }
    protected decimal BillingAccountsBalance { get; set; }
    protected decimal UpcomingBillsAmount { get; set; }
    protected decimal PaidBillsAmount { get; set; }
    protected bool MonthHasPassed => DateTime.Now > DateTimeUtils.LastDayOfMonth(State.Year, State.Month);

    public BillingBaseComponent()
    {
        Working = true;
    }
    
    protected override async Task OnInitializedAsync()
    {
        await Search();

        State.QueryParametersChanged += OnStateChanged;
    }

    private async void OnStateChanged(object sender, EventArgs args)
    {
        await InvokeAsync(Search);

        await InvokeAsync(StateHasChanged);
    }
    
    protected async Task Search()
    {
        Working = true;

        await SetBillingAccountsBalance();

        await SetUpcomingBillsAmount();

        await SetPaidBillsAmount();
        
        Working = false;
    }

    private async Task SetBillingAccountsBalance()
    {
        var accountQuery = new AccountQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Billing,
            IncludeSharedAccounts = false,
            BalanceToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month)
        };

        var accounts = await AccountProvider.GetAccounts(accountQuery);

        BillingAccountsBalance = accounts.Sum(x => x.Balance);
    }

    private async Task SetUpcomingBillsAmount()
    {
        if (MonthHasPassed)
        {
            UpcomingBillsAmount = 0;
        }
        else
        {
            UpcomingBillsQuery = new ScheduledTransactionQuery
            {
                AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Billing,
                IncludeCompletedTransactions = false,
                NextTransactionFromDate = DateTimeUtils.FirstDayOfMonth(State.Year, State.Month),
                NextTransactionToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month)
            };

            UpcomingBillsAmount = (await ScheduledTransactionProvider
                    .GetScheduledTransactions(UpcomingBillsQuery))
                .Sum(x => x.Amount);
        }
    }

    private async Task SetPaidBillsAmount()
    {
        PaidBillsQuery = new TransactionQuery
        {
            AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Billing,
            IncludeTransactionsFromSharedAccounts = false,
            IncludeExcludedTransactions = false,
            FromDate = DateTimeUtils.FirstDayOfMonth(State.Year, State.Month),
            ToDate = MonthHasPassed
                ? DateTimeUtils.LastDayOfMonth(State.Year, State.Month)
                : DateTime.Now
        };

        PaidBillsAmount = (await TransactionsProvider
                .GetTransactions(PaidBillsQuery))
            .Sum(x => x.Amount);
    }
    
    public void Dispose()
    {
        State.QueryParametersChanged -= OnStateChanged;
    }
}