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

public class AccountsTableService : TableService<AccountQuery>
{
    private readonly IAccountProvider _accountProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public AccountsTableService(IAccountProvider accountProvider, State state) : base(state)
    {
        _accountProvider = accountProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Name" }
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Account number",
                Icon = Icons.Sharp.Numbers
            },
            TdClass = "d-none d-md-table-cell d-widget-none"
        });

        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Bank" },
            TdClass = "d-none d-md-table-cell d-widget-none"
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Balance",
                Icon = Icons.Sharp.Money
            }
        });
    }

    public override Task CreateFilters(AccountQuery query)
    {
        Filter.Clear();

        Filter.Add(new Checkbox<AccountQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeSharedAccountsChanged,
            Value = query.IncludeSharedAccounts,
            Label = "Include shared accounts",
        });
        
        Filter.Add(new Checkbox<AccountQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeExternalAccountsChanged,
            Value = query.IncludeExternalAccounts,
            Label = "Include external accounts",
        });
        
        Filter.Add(new Checkbox<AccountQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeDiscontinuedAccountsChanged,
            Value = query.IncludeDiscontinuedAccounts,
            Label = "Include discontinued accounts",
        });

        return Task.CompletedTask;
    }

    private Task OnIncludeExternalAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeExternalAccounts = value;
        checkbox.Value = value;
        
        State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeDiscontinuedAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeDiscontinuedAccounts = value;
        checkbox.Value = value;
        
        State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeSharedAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeSharedAccounts = value;
        checkbox.Value = value;
        
        State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(AccountQuery accountQuery, TableState tableState)
    {
        if (!HideFilter && !Widget && !Filter.Any())
        {
            await CreateFilters(accountQuery);
        }

        if (UseStateForQuerying)
        {
            accountQuery.BankId = State.BankId;
            accountQuery.BalanceToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
            accountQuery.DiscontinuedDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
        }
        
        var accounts = await _accountProvider.GetAccounts(accountQuery);

        return accounts.Take(Widget ? 5 : accounts.Count).Select(account => new TableRow
        {
            Id = account.Id,
            Columns = new List<Column>
            {
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = account.Name,
                        Icon = IconUtils.GetAccountTypeIcon(account.AccountType)
                    }
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = account.AccountNumber,
                        Icon = Widget ? Icons.Sharp.Numbers : null
                    },
                    TdClass = "d-none d-md-table-cell d-widget-none"
                },
                new Column
                {
                    ColumnText = new ColumnText { Text = account.Bank.Name },
                    TdClass = "d-none d-md-table-cell d-widget-none"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = account.Balance.ToString("N2"),
                        Icon = Widget ? Icons.Sharp.Money : null,
                        SmallText = $"{account.BalanceDate:dd.MM.yyyy} {(account.BalanceIsAccumulated ? "(accumulated)" : "")}",
                    }
                }
            }
        }).ToList();
    }

    public override async Task OpenFullVersionDialog(UIHelpers uiHelpers, AccountQuery query)
    {
        var parameters = new DialogParameters
        {
            { nameof(AccountsOverviewDialog.AccountQuery), query }
        };

        await uiHelpers.ShowDialog<AccountsOverviewDialog>(parameters);
    }

    public override async Task OpenAddItemDialog(UIHelpers uiHelpers)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddAccountDialog.OnAccountAdded), OnItemAdded }
        };

        await uiHelpers.ShowDialog<AddAccountDialog>(parameters);
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