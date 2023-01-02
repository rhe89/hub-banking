using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Web.WebApp.Components.Banks;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Web.BlazorServer.Services;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class BanksTableService : TableService<BankQuery>
{
    private readonly IBankProvider _bankProvider;
    private readonly IAccountProvider _accountProvider;
    
    public BanksTableService(
        IBankProvider bankProvider,
        IAccountProvider accountProvider)
    {
        _bankProvider = bankProvider;
        _accountProvider = accountProvider;
    }
    
    public override Func<UIService, long, Task> OnRowClicked => OpenEditItemDialog;

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
    
    public override async Task<IList<TableRow>> FetchData(BankQuery bankQuery, TableState tableState)
    {
        bankQuery.Take = Widget ? 5 : null;
        
        var banks = await _bankProvider.Get(bankQuery);
        var accounts = await _accountProvider.Get();

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

    public override async Task OpenAddItemDialog(UIService uiService)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddBankDialog.OnBankAdded), OnItemAdded }
        };

        await uiService.ShowDialog<AddBankDialog>(parameters);
    }

    private async Task OpenEditItemDialog(UIService uiService, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditBankDialog.BankId), id },
            { nameof(EditBankDialog.OnBankUpdated), OnItemUpdated },
            { nameof(EditBankDialog.OnBankDeleted), OnItemDeleted }
        };

        await uiService.ShowDialog<EditBankDialog>(parameters);
    }
}