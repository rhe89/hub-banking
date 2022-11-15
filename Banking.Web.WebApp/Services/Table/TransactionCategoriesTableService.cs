using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Web.WebApp.Components.TransactionCategories;
using Banking.Web.WebApp.Shared;
using Hub.Shared.DataContracts.Banking.Query;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class TransactionCategoriesTableService : TableService<TransactionCategoryQuery>
{
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public TransactionCategoriesTableService(
        ITransactionCategoryProvider transactionCategoryProvider, 
        State state) : base(state)
    {
        _transactionCategoryProvider = transactionCategoryProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Name" } });
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Sub categories" } });
    }

    public override Task CreateFilters(TransactionCategoryQuery transactionCategoryQuery)
    {
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(TransactionCategoryQuery transactionCategoryQuery, TableState tableState)
    {
        transactionCategoryQuery.Take = Widget ? 5 : null;

        var transactionCategories = await _transactionCategoryProvider.GetTransactionCategories(transactionCategoryQuery);
        var transactionSubCategories = await _transactionCategoryProvider.GetTransactionSubCategories();

        return transactionCategories.Select(transactionCategory => new TableRow
        {
            Id = transactionCategory.Id,
            Columns = new List<Column>
            {
                new Column
                {
                    ColumnText = new ColumnText { Text = transactionCategory.Name }
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = string.Join(", ", transactionSubCategories
                                               .Where(transactionSubCategory =>
                                                          transactionSubCategory.TransactionCategoryId == transactionCategory.Id)
                                               .Select(x => x.Name))
                    }
                }
            }
        }).ToList();
    }

    public override async Task OpenFullVersionDialog(UIHelpers uiHelpers, TransactionCategoryQuery transactionCategoryQuery)
    {
        await uiHelpers.ShowDialog<TransactionCategoriesOverviewDialog>();
    }

    public override async Task OpenAddItemDialog(UIHelpers uiHelpers)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddTransactionCategoryDialog.OnTransactionCategoryAdded), OnItemAdded }
        };

        await uiHelpers.ShowDialog<AddTransactionCategoryDialog>(parameters);
    }

    public override async Task OpenEditItemDialog(UIHelpers uiHelpers, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditTransactionCategoryDialog.TransactionCategoryId), id },
            { nameof(EditTransactionCategoryDialog.OnTransactionCategoryUpdated), OnItemUpdated },
            { nameof(EditTransactionCategoryDialog.OnTransactionCategoryDeleted), OnItemDeleted }
        };

        await uiHelpers.ShowDialog<EditTransactionCategoryDialog>(parameters);
    }
}