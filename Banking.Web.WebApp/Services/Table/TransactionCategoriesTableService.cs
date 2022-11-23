using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Components.TransactionCategories;
using Banking.Web.WebApp.Shared;
using Hub.Shared.DataContracts.Banking.Query;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class TransactionCategoriesTableService : TableService<TransactionCategoryQuery>
{
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    private readonly ITransactionProvider _transactionProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public TransactionCategoriesTableService(
        ITransactionCategoryProvider transactionCategoryProvider, 
        ITransactionProvider transactionProvider,
        State state) : base(state)
    {
        _transactionCategoryProvider = transactionCategoryProvider;
        _transactionProvider = transactionProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Name" } });
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Amount" } });
    }

    public override Task CreateFilters(TransactionCategoryQuery transactionCategoryQuery)
    {
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(TransactionCategoryQuery transactionCategoryQuery, TableState tableState)
    {
        transactionCategoryQuery.Take = Widget ? 5 : null;

        var transactionCategories = await _transactionCategoryProvider.GetTransactionCategories(transactionCategoryQuery);

        var transactionQuery = new TransactionQuery();

        if (UseStateForQuerying)
        {
            transactionQuery.FromDate = DateTimeUtils.FirstDayOfMonth(State.Year, State.Month);
            transactionQuery.ToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
        }
        
        var transactions = await _transactionProvider.GetTransactions(transactionQuery);
        
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