using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Components;
using Banking.Web.WebApp.Components.Accounts;
using Banking.Web.WebApp.Components.Banks;
using Banking.Web.WebApp.Components.Transactions;
using Banking.Web.WebApp.Extensions;
using Banking.Web.WebApp.Shared;
using Banking.Web.WebApp.Utils;
using Hub.Shared.DataContracts.Banking.Query;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class TransactionsTableService : TableService<TransactionQuery>
{
    private readonly ITransactionProvider _transactionProvider;
    private readonly IAccountProvider _accountProvider;
    private readonly IBankProvider _bankProvider;
    
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;

    public TransactionsTableService(
        ITransactionProvider transactionProvider,
        IAccountProvider accountProvider,
        IBankProvider bankProvider,
        State state) : base(state)
    {
        _transactionProvider = transactionProvider;
        _accountProvider = accountProvider;
        _bankProvider = bankProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Clear();
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Date",
                Icon = Icons.Sharp.DateRange
            }
        });

        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Description",
                Icon = IconUtils.TransactionDescription
            }
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Amount",
                Icon = Icons.Sharp.Money
            }
        });

        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Account" },
            TdClass = "d-none d-md-table-cell d-widget-none"
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Category",
                Icon = IconUtils.TransactionCategoryIcon
            },
            TdClass = "d-none d-md-table-cell d-widget-none"
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Sub category",
                Icon = IconUtils.TransactionCategoryIcon
            },
            TdClass = "d-none d-md-table-cell d-widget-none"
        });
    }

    public override Task CreateFilters(TransactionQuery transactionQuery)
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
            Name = nameof(AccountsSelect)
        });
        
        Filter.Add(new Input
        {
            FilterType = FilterType.Component,
            Name = nameof(MonthYearSelect)
        });
        
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(TransactionQuery transactionQuery, TableState tableState)
    {
        if (!HideFilter && !Widget && !Filter.Any())
        {
            await CreateFilters(transactionQuery);
        }

        if (UseStateForQuerying)
        {
            transactionQuery.BankId = State.BankId;
            transactionQuery.AccountId = State.AccountId;
            transactionQuery.FromDate = State.GetValidFromDateForMonthAndYear();
            transactionQuery.ToDate = State.GetValidToDateForMonthAndYear();
            transactionQuery.IncludeTransactionsFromSharedAccounts = true;
        }

        transactionQuery.Take = Widget ? 5 : null;

        var transactions = await _transactionProvider.GetTransactions(transactionQuery);

        var tableRows = transactions.Select(transaction => new TableRow
        {
            Id = transaction.Id,
            ValueToOrderBy = transaction.TransactionDate,
            Columns = new List<Column>
            {
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = transaction.TransactionDate.ToNorwegianDateString(),
                        Icon = Widget ? Icons.Sharp.DateRange : null
                    },
                    TdClass = Widget ? "td-width-20 td-md-width-25" : "td-md-width-5 td-width-20"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Class = "d-inline-block",
                        Icon = Widget ? IconUtils.TransactionDescription : null,
                        Text = transaction.Description
                    },
                    TdClass = Widget ? "td-width-50 td-md-width-45" : "td-md-width-25 td-width-50"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = transaction.Amount.ToString(CultureInfo.InvariantCulture),
                        Icon = Widget ? Icons.Sharp.Money : null
                    },
                    TdClass = Widget ? "td-width-30" : "td-md-width-10 td-width-30"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = transaction.Account?.Name,
                        Icon = IconUtils.GetAccountTypeIcon(transaction.Account?.AccountType)
                    },
                    TdClass = "d-none d-md-table-cell d-widget-none td-md-width-20"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = transaction.TransactionSubCategory?.TransactionCategory.Name.FirstCharToUpper() ?? "N/A",
                        Icon = Widget ? IconUtils.TransactionCategoryIcon : null
                    },
                    TdClass = "d-none d-md-table-cell d-widget-none td-md-width-20"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Text = transaction.TransactionSubCategory?.Name.FirstCharToUpper() ?? "N/A",
                        Icon = Widget ? IconUtils.TransactionCategoryIcon : null
                    },
                    TdClass = "d-none d-md-table-cell d-widget-none td-md-width-20"
                },
            }
        }).ToList();
        
        Footer = Widget ? null : new Column { ColumnText = new ColumnText { Text = $"Sum: {transactions.Sum(x => x.Amount)}" } };

        return tableRows;
    }

    public override async Task OpenFullVersionDialog(UIHelpers uiHelpers, TransactionQuery transactionQuery)
    {
        var parameters = new DialogParameters
        {
            { nameof(TransactionsOverviewDialog.TransactionQuery), transactionQuery }
        };

        await uiHelpers.ShowDialog<TransactionsOverviewDialog>(parameters);
    }

    public override async Task OpenAddItemDialog(UIHelpers uiHelpers)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddTransactionDialog.OnTransactionAdded), OnItemAdded }
        };

        await uiHelpers.ShowDialog<AddTransactionDialog>(parameters);
    }

    public override async Task OpenEditItemDialog(UIHelpers uiHelpers, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditTransactionDialog.TransactionId), id.ToString() },
            { nameof(EditTransactionDialog.OnTransactionUpdated), OnItemUpdated },
            { nameof(EditTransactionDialog.OnTransactionDeleted), OnItemDeleted }
        };

        await uiHelpers.ShowDialog<EditTransactionDialog>(parameters);
    }
}