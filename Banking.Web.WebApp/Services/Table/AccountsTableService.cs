using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Web.WebApp.Components;
using Banking.Web.WebApp.Components.Accounts;
using Banking.Web.WebApp.Components.Banks;
using Banking.Web.WebApp.Shared;
using Banking.Web.WebApp.Utils;
using Hub.Shared.DataContracts.Banking.Query;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class AccountsTableService : TableService<AccountQuery>
{
    private readonly IAccountProvider _accountProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;
    
    public bool IncludeAccountsWithNoBalanceForGivenPeriod { get; set; }
    
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
                Text = "Balance",
                Icon = Icons.Sharp.Money
            }
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
    }

    public override Task CreateFilters(AccountQuery accountQuery)
    {
        Filter.Clear();
        
        Filter.Add(new Input
        {
            FilterType = FilterType.Component,
            Name = nameof(BanksSelect)
        });

        Filter.Add(new Input
        {
            FilterType = FilterType.Component,
            Name = nameof(MonthYearSelect)
        });
        
        Filter.Add(new Checkbox<AccountQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeSharedAccountsChanged,
            Value = accountQuery.IncludeSharedAccounts,
            Name = "Include shared accounts",
        });
        
        Filter.Add(new Checkbox<AccountQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeExternalAccountsChanged,
            Value = accountQuery.IncludeExternalAccounts,
            Name = "Include external accounts",
        });
        
        Filter.Add(new Checkbox<AccountQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeDiscontinuedAccountsChanged,
            Value = accountQuery.IncludeDiscontinuedAccounts,
            Name = "Include discontinued accounts",
        });
        
        Filter.Add(new Checkbox<AccountQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeAccountsWithNoBalanceForGivenPeriodChanged,
            Value = IncludeAccountsWithNoBalanceForGivenPeriod,
            Name = "Include accounts with no balance",
        });
        
        return Task.CompletedTask;
    }

    private Task OnIncludeExternalAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeExternalAccounts = value;
        checkbox.Value = value;
        
        State.OnStateUpdated.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeDiscontinuedAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeDiscontinuedAccounts = value;
        checkbox.Value = value;
        
        State.OnStateUpdated.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeSharedAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeSharedAccounts = value;
        checkbox.Value = value;
        
        State.OnStateUpdated.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeAccountsWithNoBalanceForGivenPeriodChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        IncludeAccountsWithNoBalanceForGivenPeriod = value;
        checkbox.Value = value;
        
        State.OnStateUpdated.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(AccountQuery accountQuery, TableState tableState)
    {
        if (!HideFilter && !Widget && !Filter.Any())
        {
            await CreateFilters(accountQuery);
        }
        
        accountQuery.BankId = State.BankId;
        accountQuery.BalanceToDate = State.GetValidToDateForMonthAndYear();
        accountQuery.DiscontinuedDate = State.GetValidFromDateForMonthAndYear();
        
        var accounts = await _accountProvider.Get(accountQuery);
        
        return accounts.Where(account => IncludeAccountsWithNoBalanceForGivenPeriod || !account.NoBalanceForGivenPeriod).Take(Widget ? 5 : accounts.Count).Select(account => new TableRow
        {
            Id = account.Id,
            Columns = new List<Column>
            {
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = $"{account.Name}{(account.Bank != null ? $" ({account.Bank.Name})" : "")}",
                        Icon = IconUtils.GetAccountTypeIcon(account.AccountType)
                    },
                    TdClass = Widget ? "td-width-60" : "td-md-width-40 td-width-60" 
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = account.Balance.ToString("N2"),
                        Icon = Widget ? Icons.Sharp.Money : null,
                        SmallText = $"{account.BalanceDate:dd.MM} {(account.BalanceIsAccumulated ? "(accumulated)" : "")}",
                    },
                    TdClass = Widget ? "td-width-40" : "td-md-width-40 td-width-40"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = account.AccountNumber,
                        Icon = Widget ? Icons.Sharp.Numbers : null
                    },
                    TdClass = "d-none d-md-table-cell d-widget-none td-md-width-20"
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