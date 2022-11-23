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

public class TransactionSubCategoriesTableService : TableService<TransactionSubCategoryQuery>
{
    private readonly ITransactionCategoryProvider _transactionSubCategoryProvider;
    private readonly ITransactionProvider _transactionProvider;
    
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public TransactionSubCategoriesTableService(
        ITransactionCategoryProvider transactionSubCategoryProvider, 
        ITransactionProvider transactionProvider,
        State state) : base(state)
    {
        _transactionSubCategoryProvider = transactionSubCategoryProvider;
        _transactionProvider = transactionProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Name" } });
        HeaderRow.Add(new Column { ColumnText = new ColumnText { Text = "Amount" } });
    }

    public override Task CreateFilters(TransactionSubCategoryQuery transactionSubCategoryQuery)
    {
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(TransactionSubCategoryQuery transactionSubCategoryQuery, TableState tableState)
    {
        transactionSubCategoryQuery.Take = Widget ? 5 : null;

        var transactionCategories = await _transactionSubCategoryProvider.GetTransactionSubCategories(transactionSubCategoryQuery);

        var transactionQuery = new TransactionQuery();

        if (UseStateForQuerying)
        {
            transactionQuery.FromDate = DateTimeUtils.FirstDayOfMonth(State.Year, State.Month);
            transactionQuery.ToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
        }
        
        var transactions = await _transactionProvider.GetTransactions(transactionQuery);
        
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

    public override Task OpenFullVersionDialog(UIHelpers uiHelpers, TransactionSubCategoryQuery transactionSubCategoryQuery)
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
            { nameof(EditTransactionSubCategoryDialog.TransactionSubCategoryId), id },
            { nameof(EditTransactionSubCategoryDialog.OnTransactionSubCategoryUpdated), OnItemUpdated },
            { nameof(EditTransactionSubCategoryDialog.OnTransactionSubCategoryDeleted), OnItemDeleted }
        };

        await uiHelpers.ShowDialog<EditTransactionSubCategoryDialog>(parameters);
    }
}