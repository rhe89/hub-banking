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

public class TransactionSubCategoriesTableService : TableService<TransactionSubCategoryQuery>
{
    private readonly ITransactionCategoryProvider _transactionSubCategoryProvider;
    private readonly ITransactionProvider _transactionProvider;
    private readonly BankingState _state;

    public TransactionSubCategoriesTableService(
        ITransactionCategoryProvider transactionSubCategoryProvider, 
        ITransactionProvider transactionProvider,
        BankingState state)
    {
        _transactionSubCategoryProvider = transactionSubCategoryProvider;
        _transactionProvider = transactionProvider;
        _state = state;
    }

    public override Func<UIService, long, Task> OnRowClicked => OpenEditItemDialog;

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Name" } });
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Amount" } });
    }

    public override async Task<IList<TableRow>> FetchData(TransactionSubCategoryQuery transactionSubCategoryQuery, TableState tableState)
    {
        transactionSubCategoryQuery.Take = Widget ? 5 : null;

        var transactionCategories = await _transactionSubCategoryProvider.Get(transactionSubCategoryQuery);

        var transactionQuery = new TransactionQuery
        {
            FromDate = _state.GetValidFromDateForMonthAndYear(),
            ToDate = _state.GetValidToDateForMonthAndYear()
        };

        var transactions = await _transactionProvider.Get(transactionQuery);
        
        return transactionCategories.Select(transactionSubCategory => new TableRow
        {
            Id = transactionSubCategory.Id,
            Columns = new List<Column>
            {
                new Column
                {
                    ColumnText = new ColumnText { Text = transactionSubCategory.Name.FirstCharToUpper() }
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = transactions
                            .Where(x => transactionSubCategory.Id == x.TransactionSubCategoryId)
                            .Sum(x => x.Amount).ToString("N2")
                    }
                }
            }
        }).ToList();
    }

    public override Task OpenAddItemDialog(UIService uiService)
    {
        return Task.CompletedTask;
    }

    private async Task OpenEditItemDialog(UIService uiService, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditTransactionSubCategoryDialog.TransactionSubCategoryId), id },
            { nameof(EditTransactionSubCategoryDialog.OnTransactionSubCategoryUpdated), OnItemUpdated },
            { nameof(EditTransactionSubCategoryDialog.OnTransactionSubCategoryDeleted), OnItemDeleted }
        };

        await uiService.ShowDialog<EditTransactionSubCategoryDialog>(parameters);
    }
}