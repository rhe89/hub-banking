using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Web.WebApp.Components.Accounts;
using Banking.Web.WebApp.Components.Banks;
using Banking.Web.WebApp.Components.Shared;
using Banking.Web.WebApp.Utils;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Web.BlazorServer.Services;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class AccountsTableService : TableService<AccountQuery>
{
    private readonly IAccountProvider _accountProvider;
    private readonly BankingState _state;

    public AccountsTableService(IAccountProvider accountProvider, BankingState state)
    {
        _accountProvider = accountProvider;
        _state = state;
    }
    
    public override Func<UIService, long, Task> OnRowClicked => OpenEditItemDialog;
    private bool IncludeAccountsWithNoBalanceForGivenPeriod { get; set; }

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

    public override async Task<IList<TableRow>> FetchData(AccountQuery accountQuery, TableState tableState)
    {
        if (!Widget && !Filter.Any())
        {
            CreateFilters(accountQuery);
        }
        
        accountQuery.BankId = _state.BankId;
        accountQuery.BalanceToDate = _state.GetValidToDateForMonthAndYear();
        accountQuery.DiscontinuedDate = _state.GetValidFromDateForMonthAndYear();
        
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
    
    private void CreateFilters(AccountQuery accountQuery)
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
    }
    
    private Task OnIncludeExternalAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeExternalAccounts = value;
        checkbox.Value = value;
        
        _state.OnStateUpdated.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeDiscontinuedAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeDiscontinuedAccounts = value;
        checkbox.Value = value;
        
        _state.OnStateUpdated.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeSharedAccountsChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        query.IncludeSharedAccounts = value;
        checkbox.Value = value;
        
        _state.OnStateUpdated.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }
    
    private Task OnIncludeAccountsWithNoBalanceForGivenPeriodChanged(Checkbox<AccountQuery> checkbox, bool value, AccountQuery query)
    {
        IncludeAccountsWithNoBalanceForGivenPeriod = value;
        checkbox.Value = value;
        
        _state.OnStateUpdated.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }
    
    public override async Task OpenAddItemDialog(UIService uiService)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddAccountDialog.OnAccountAdded), OnItemAdded }
        };

        await uiService.ShowDialog<AddAccountDialog>(parameters);
    }

    public async Task OpenEditItemDialog(UIService uiService, long id)
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