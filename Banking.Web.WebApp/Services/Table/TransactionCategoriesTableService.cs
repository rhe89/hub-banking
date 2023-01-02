using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Web.WebApp.Components.TransactionCategories;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Extensions;
using Hub.Shared.Web.BlazorServer.Services;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class TransactionCategoriesTableService : TableService<TransactionCategoryQuery>
{
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    private readonly ITransactionProvider _transactionProvider;
    private readonly BankingState _state;

    public TransactionCategoriesTableService(
        ITransactionCategoryProvider transactionCategoryProvider, 
        ITransactionProvider transactionProvider,
        BankingState state)
    {
        _transactionCategoryProvider = transactionCategoryProvider;
        _transactionProvider = transactionProvider;
        _state = state;
    }
    
    public override Func<UIService, long, Task> OnRowClicked => OpenEditItemDialog;

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Name" } });
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Amount" } });
    }

    public override async Task<IList<TableRow>> FetchData(TransactionCategoryQuery transactionCategoryQuery, TableState tableState)
    {
        transactionCategoryQuery.Take = Widget ? 5 : null;

        var transactionCategories = await _transactionCategoryProvider.Get(transactionCategoryQuery);

        var transactionQuery = new TransactionQuery
        {
            FromDate = _state.GetValidFromDateForMonthAndYear(),
            ToDate = _state.GetValidToDateForMonthAndYear()
        };

        var transactions = await _transactionProvider.Get(transactionQuery);
        
        return transactionCategories.Select(transactionCategory => new TableRow
        {
            Id = transactionCategory.Id,
            Columns = new List<Column>
            {
                new Column
                {
                    ColumnText = new ColumnText { Text = transactionCategory.Name.FirstCharToUpper() }
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = transactions
                            .Where(x => transactionCategory.TransactionSubCategories
                                       .Any(tsc => tsc.Id == x.TransactionSubCategoryId)).Sum(x => x.Amount).ToString("N2")
                    }
                }
            }
        }).ToList();
    }

    public override async Task OpenAddItemDialog(UIService uiService)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddTransactionCategoryDialog.OnTransactionCategoryAdded), OnItemAdded }
        };

        await uiService.ShowDialog<AddTransactionCategoryDialog>(parameters);
    }

    private async Task OpenEditItemDialog(UIService uiService, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditTransactionCategoryDialog.TransactionCategoryId), id },
            { nameof(EditTransactionCategoryDialog.OnTransactionCategoryUpdated), OnItemUpdated },
            { nameof(EditTransactionCategoryDialog.OnTransactionCategoryDeleted), OnItemDeleted }
        };

        await uiService.ShowDialog<EditTransactionCategoryDialog>(parameters);
    }
}