using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Utilities;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components.Billing;

public class BillingBaseComponent : BankingBaseComponent, IDisposable
{
    [Inject] private IScheduledTransactionProvider ScheduledTransactionProvider { get; set; }
    [Inject] private ITransactionProvider TransactionsProvider { get; set; }
    [Inject] private IAccountProvider AccountProvider { get; set; }
    
    protected TransactionQuery PaidBillsQuery { get; set; }
    protected ScheduledTransactionQuery UpcomingBillsQuery { get; set; }
    protected decimal BillingAccountsBalance { get; set; }
    protected decimal UpcomingBillsAmount { get; set; }
    protected decimal PaidBillsAmount { get; set; }
    protected bool MonthHasPassed => DateTimeUtils.Today > State.GetValidToDateForMonthAndYear();

    public BillingBaseComponent()
    {
        Working = true;
    }
    
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
        if (MonthHasPassed || State.GetValidFromDateForMonthAndYear() > DateTimeUtils.FirstDayOfMonth().AddMonths(1))
        {
            BillingAccountsBalance = 0;
        }
        else
        {
            var accountQuery = new AccountQuery
            {
                AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Billing,
                IncludeSharedAccounts = false,
                BalanceToDate = State.GetValidToDateForMonthAndYear(),
                DiscontinuedDate = State.GetValidToDateForMonthAndYear()
            };

            var accounts = await AccountProvider.Get(accountQuery);

            BillingAccountsBalance = accounts.Sum(x => x.Balance);
        }
    }

    private async Task SetUpcomingBillsAmount()
    {
        if (MonthHasPassed)
        {
            UpcomingBillsAmount = 0;
        }
        else if (State.GetValidFromDateForMonthAndYear().AddMonths(1) == DateTimeUtils.FirstDayOfMonth().AddMonths(1))
        {
            UpcomingBillsQuery = new ScheduledTransactionQuery
            {
                AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Billing,
                NextTransactionToDate = State.GetValidToDateForMonthAndYear()
            };

            UpcomingBillsAmount = (await ScheduledTransactionProvider
                    .Get(UpcomingBillsQuery))
                .Sum(x => x.Amount);
        }
        else
        {
            UpcomingBillsQuery = new ScheduledTransactionQuery
            {
                AccountType = Hub.Shared.DataContracts.Banking.Constants.AccountTypes.Billing,
                NextTransactionFromDate = State.GetValidFromDateForMonthAndYear(),
                NextTransactionToDate = State.GetValidToDateForMonthAndYear()
            };

            UpcomingBillsAmount = (await ScheduledTransactionProvider
                    .Get(UpcomingBillsQuery))
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
            FromDate = State.GetValidFromDateForMonthAndYear(),
            ToDate = MonthHasPassed
                ? State.GetValidToDateForMonthAndYear()
                : DateTimeUtils.Today
        };

        PaidBillsAmount = (await TransactionsProvider
                .Get(PaidBillsQuery))
            .Sum(x => x.Amount);
    }
    
    public void Dispose()
    {
        State.OnStateUpdated -= OnStateChanged;
    }
}