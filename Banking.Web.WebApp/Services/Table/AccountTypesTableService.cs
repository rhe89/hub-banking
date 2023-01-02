using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Web.WebApp.Components.Accounts;
using Banking.Web.WebApp.Utils;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Utilities;
using Hub.Shared.Web.BlazorServer.Services;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class AccountTypesTableService : TableService<AccountTypesQuery>
{
    private readonly IAccountProvider _accountProvider;
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    private readonly BankingState _state;
    
    public AccountTypesTableService(
        IAccountProvider accountProvider,
        IScheduledTransactionProvider scheduledTransactionProvider,
        BankingState state)
    {
        _accountProvider = accountProvider;
        _scheduledTransactionProvider = scheduledTransactionProvider;
        _state = state;
    }
    
    public override Func<UIService, long, Task> OnRowClicked => OpenEditItemDialog;

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

    public override async Task<IList<TableRow>> FetchData(AccountTypesQuery accountTypesQuery, TableState tablestate)
    {
        accountTypesQuery.BalanceToDate = _state.GetValidToDateForMonthAndYear();
        accountTypesQuery.DiscontinuedDate = _state.GetValidFromDateForMonthAndYear();
        
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
            if (_state.GetValidToDateForMonthAndYear() > DateTimeUtils.LastDayOfMonth())
            {
                var scheduledTransactions = await _scheduledTransactionProvider.Get(new ScheduledTransactionQuery
                {
                    AccountId = account.Id,
                    NextTransactionToDate = _state.GetValidToDateForMonthAndYear()
                });
                
                accountBalanceLastMonth += scheduledTransactions.Where(x => x.NextTransactionDate < _state.GetValidFromDateForMonthAndYear())
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

    public override Task OpenAddItemDialog(UIService uiService)
    {
        return Task.CompletedTask;
    }

    private async Task OpenEditItemDialog(UIService uiService, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditAccountDialog.AccountId), id },
            { nameof(EditAccountDialog.OnAccountUpdated), OnItemUpdated },
            { nameof(EditAccountDialog.OnAccountDeleted), OnItemDeleted }
        };

        await uiService.ShowDialog<EditAccountDialog>(parameters);
    }
}

public class AccountTypesQuery : AccountQuery
{
}