using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Components.Accounts;
using Banking.Web.WebApp.Shared;
using Banking.Web.WebApp.Utils;
using Hub.Shared.DataContracts.Banking.Query;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class AccountTypesTableService : TableService<AccountTypesQuery>
{
    private readonly IAccountProvider _accountProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public AccountTypesTableService(
        IAccountProvider accountProvider,
        State state) : base(state)
    {
        _accountProvider = accountProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Account" } });
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Balance",
                Icon = Icons.Sharp.Money
            }
        });
    }

    public override Task CreateFilters(AccountTypesQuery accountTypesQuery)
    {
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(AccountTypesQuery accountTypesQuery, TableState tablestate)
    {
        if (UseStateForQuerying)
        {
            accountTypesQuery.BalanceToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
        }
        
        var thisMonthsAccountBalances = await _accountProvider.GetAccounts(accountTypesQuery);

        var tableRows = new List<TableRow>();

        accountTypesQuery.BalanceToDate = accountTypesQuery.BalanceToDate!.Value.AddMonths(-1);

        var lastMonthsAccountBalances = await _accountProvider.GetAccounts(accountTypesQuery);

        foreach (var account in thisMonthsAccountBalances.Where(x => !x.NoBalanceForGivenPeriod))
        {
            var accountBalanceLastMonth = lastMonthsAccountBalances.FirstOrDefault(x => x.Id == account.Id);

            var accountBalanceValueLastMonth = accountBalanceLastMonth?.Balance ?? 0;

            var isGain = account.Balance >= accountBalanceValueLastMonth;

            var diff = isGain ? account.Balance - accountBalanceValueLastMonth : accountBalanceValueLastMonth - account.Balance;

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
                        }
                    },
                    new Column
                    {
                        ChildElements = new List<ColumnText>
                        {
                            new ColumnText
                            {
                                Text = account.Balance.ToString("N2"),
                                Icon = Widget ? Icons.Sharp.Money : null,
                                Class = "d-inline-block"
                            },
                            new ColumnText
                            {
                                Text = $"({(isGain ? "+" : "-")}kr {diff.ToString("N2")})",
                                Color = isGain ? Color.Success : Color.Error,
                                Class = "align-top fs-small d-inline-block ms-1"
                            }
                        }
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