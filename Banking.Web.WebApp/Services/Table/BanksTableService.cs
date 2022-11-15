using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Web.WebApp.Components.Banks;
using Banking.Web.WebApp.Shared;
using Hub.Shared.DataContracts.Banking.Query;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class BanksTableService : TableService<BankQuery>
{
    private readonly IBankProvider _bankProvider;
    private readonly IAccountProvider _accountProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public BanksTableService(
        IBankProvider bankProvider,
        IAccountProvider accountProvider, 
        State state) : base(state)
    {
        _bankProvider = bankProvider;
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
                Text = "Account number prefix",
                Icon = Icons.Sharp.Numbers
            },
            TdClass = "d-none d-md-table-cell d-widget-none"
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Accounts" }
        });
    }

    public override Task CreateFilters(BankQuery query)
    {
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(BankQuery bankQuery, TableState tableState)
    {
        bankQuery.Take = Widget ? 5 : null;
        
        var banks = await _bankProvider.GetBanks(bankQuery);
        var accounts = await _accountProvider.GetAccounts();

        return banks.Select(bank => new TableRow
        {
            Id = bank.Id,
            Columns = new List<Column>
            {
                new Column { ColumnText = new ColumnText { Text = bank.Name } },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = bank.AccountNumberPrefix,
                        Icon = Widget ? Icons.Sharp.Numbers : null
                    },
                    TdClass = "d-widget-none d-md-table-cell"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = accounts.Count(account => account.BankId == bank.Id).ToString()
                    }
                }
            }
        }).ToList();
    }

    public override async Task OpenFullVersionDialog(UIHelpers uiHelpers, BankQuery query)
    {
        await uiHelpers.ShowDialog<BanksOverviewDialog>();
    }

    public override async Task OpenAddItemDialog(UIHelpers uiHelpers)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddBankDialog.OnBankAdded), OnItemAdded }
        };

        await uiHelpers.ShowDialog<AddBankDialog>(parameters);
    }

    public override async Task OpenEditItemDialog(UIHelpers uiHelpers, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditBankDialog.BankId), id },
            { nameof(EditBankDialog.OnBankUpdated), OnItemUpdated },
            { nameof(EditBankDialog.OnBankDeleted), OnItemDeleted }
        };

        await uiHelpers.ShowDialog<EditBankDialog>(parameters);
    }
}