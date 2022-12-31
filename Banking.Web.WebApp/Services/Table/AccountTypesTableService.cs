using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Components;
using Banking.Web.WebApp.Components.Accounts;
using Banking.Web.WebApp.Shared;
using Banking.Web.WebApp.Utils;
using Hub.Shared.DataContracts.Banking.Query;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class AccountTypesTableService : TableService<AccountTypesQuery>
{
    private readonly IAccountProvider _accountProvider;
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public AccountTypesTableService(
        IAccountProvider accountProvider,
        IScheduledTransactionProvider scheduledTransactionProvider,
        State state) : base(state)
    {
        _accountProvider = accountProvider;
        _scheduledTransactionProvider = scheduledTransactionProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Account" },
            TdClass = Widget ? "td-width-50" : "td-width-50 td-md-width-50"
        });
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Balance",
                Icon = Icons.Sharp.Money
            },
            TdClass = Widget ? "td-width-50" : "td-width-50 td-md-width-50 td-lg-width-15"
        });
    }

    public override Task CreateFilters(AccountTypesQuery accountTypesQuery)
    {
        Filter.Clear();

        Filter.Add(new Input
        {
            FilterType = FilterType.Component,
            Name = nameof(MonthYearSelect)
        });
        
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(AccountTypesQuery accountTypesQuery, TableState tablestate)
    {
        accountTypesQuery.BalanceToDate = State.GetValidToDateForMonthAndYear();
        accountTypesQuery.DiscontinuedDate = State.GetValidFromDateForMonthAndYear();
        
        var thisMonthsAccountStates = await _accountProvider.Get(accountTypesQuery);

        var tableRows = new List<TableRow>();

        accountTypesQuery.BalanceToDate = accountTypesQuery.BalanceToDate!.Value.AddMonths(-1);

        var lastMonthsAccountStates = await _accountProvider.Get(accountTypesQuery);
        
        foreach (var account in thisMonthsAccountStates)
        {
            var accountStateLastMonth = lastMonthsAccountStates.FirstOrDefault(x => x.Id == account.Id);

            var accountBalanceLastMonth = accountStateLastMonth?.Balance ?? 0;

            var accountBalanceThisMonth = account.Balance;
            
            //Include budgeted deposits/withdrawals in future months 
            if (State.GetValidToDateForMonthAndYear() > DateTimeUtils.LastDayOfMonth())
            {
                var scheduledTransactions = await _scheduledTransactionProvider.Get(new ScheduledTransactionQuery
                {
                    AccountId = account.Id,
                    NextTransactionToDate = State.GetValidToDateForMonthAndYear()
                });
                
                accountBalanceLastMonth += scheduledTransactions.Where(x => x.NextTransactionDate < State.GetValidFromDateForMonthAndYear())
                    .Sum(x => x.Amount);
                accountBalanceThisMonth += scheduledTransactions.Sum(x => x.Amount);
            }
            
            var diff = accountBalanceThisMonth - accountBalanceLastMonth;

            tableRows.Add(new TableRow
            {
                Id = account.Id,
                Columns = new List<Column>
                {
                    new Column
                    {
                        ColumnText = new ColumnText
                        {
                            Text = $"{account.Name} ({account.Bank.Name})",
                            Icon = IconUtils.GetAccountTypeIcon(account.AccountType)
                        },
                        TdClass = Widget ? "td-width-40" : "td-width-40 td-md-width-50"
                    },
                    new Column
                    {
                        ChildElements = new List<ColumnText>
                        {
                            new ColumnText
                            {
                                Text = accountBalanceThisMonth.ToString("N2"),
                                Icon = Widget ? Icons.Sharp.Money : null,
                                Class = "d-inline-block"
                            },
                            new ColumnText
                            {
                                Text = $"kr {diff:N2})",
                                Color = diff > 0 ? Color.Success : Color.Error,
                                Class = "align-top fs-small d-inline-block ms-1"
                            }
                        },
                        TdClass = Widget ? "td-width-60" : "td-width-60 td-md-width-50"
                    }
                }
            });
        }

        return tableRows;
    }

    public override Task OpenFullVersionDialog(UIHelpers uiHelpers, AccountTypesQuery accountTypesQuery)
    {
        return Task.CompletedTask;
    }

    public override Task OpenAddItemDialog(UIHelpers uiHelpers)
    {
        return Task.CompletedTask;
    }

    public override async Task OpenEditItemDialog(UIHelpers uiHelpers, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditAccountDialog.AccountId), id },
            { nameof(EditAccountDialog.OnAccountUpdated), OnItemUpdated },
            { nameof(EditAccountDialog.OnAccountDeleted), OnItemDeleted }
        };

        await uiHelpers.ShowDialog<EditAccountDialog>(parameters);
    }
}

public class AccountTypesQuery : AccountQuery
{
}