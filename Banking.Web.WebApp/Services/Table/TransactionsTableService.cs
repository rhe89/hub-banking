using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
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
    
    private InputList<TransactionQuery> BankSelectList { get; set; }
    private InputList<TransactionQuery> AccountSelectList { get; set; }
    private InputList<TransactionQuery> YearSelectList { get; set; }
    private InputList<TransactionQuery> MonthSelectList { get; set; }

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

    public async override Task CreateFilters(TransactionQuery transactionQuery)
    {
        Filter.Clear();
        
        BankSelectList = new InputList<TransactionQuery>
        {
            FilterType = FilterType.Select,
            Label = "Banks",
            OnChanged = OnBankChanged
        };
        
        Filter.Add(BankSelectList);

        AccountSelectList = new InputList<TransactionQuery>
        {
            FilterType = FilterType.Select,
            Label = "Accounts",
            OnChanged = OnAccountChanged
        };
        
        Filter.Add(AccountSelectList);
        
        YearSelectList = new InputList<TransactionQuery>
        {
            FilterType = FilterType.Select,
            Label = "Year",
            OnChanged = OnYearChanged
        };
        
        Filter.Add(YearSelectList);

        MonthSelectList = new InputList<TransactionQuery>
        {
            FilterType = FilterType.Select,
            Label = "Month",
            OnChanged = OnMonthChanged
        };
        
        Filter.Add(MonthSelectList);
        
        await InitBankSelectList(transactionQuery);
        await InitAccountSelectList(transactionQuery);
        InitYearSelectList(transactionQuery);
        InitMonthSelectList(transactionQuery);
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
            transactionQuery.Month = State.Month;
            transactionQuery.Year = State.Year;
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

    private Task OnBankChanged(
        InputList<TransactionQuery> input,
        string value,
        TransactionQuery transactionQuery)
    {
        if (!UseStateForQuerying && long.TryParse(value, out var bankId))
        {
            transactionQuery.BankId = bankId;
            State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        }
        
        SetSelectedBank(value);

        return Task.CompletedTask;
    }

    private Task OnAccountChanged(
        InputList<TransactionQuery> input,
        string value,
        TransactionQuery transactionQuery)
    {
        if (!UseStateForQuerying && long.TryParse(value, out var accountId))
        {
            transactionQuery.AccountId = accountId;
            State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        }
        
        SetSelectedAccount(value);

        return Task.CompletedTask;
    }
    
    private Task OnYearChanged(
        InputList<TransactionQuery> input,
        string value,
        TransactionQuery transactionQuery)
    {
        if (!UseStateForQuerying && int.TryParse(value, out var year))
        {
            transactionQuery.Year = year;
            State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        }
        
        SetSelectedYear(value);
        
        return Task.CompletedTask;
    }

    private Task OnMonthChanged(
        InputList<TransactionQuery> input,
        string value,
        TransactionQuery transactionQuery)
    {
        if (!UseStateForQuerying && int.TryParse(value, out var month))
        {
            transactionQuery.Month = month;
            State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        }
        
        SetSelectedMonth(value);
        
        return Task.CompletedTask;
    }
    
    private async Task InitBankSelectList(TransactionQuery transactionQuery)
    {
        var banks = await _bankProvider.GetBanks();
        
        BankSelectList.Items = new List<InputValue>
        {
            new InputValue
            {
                Text = "All",
                Value = "0"
            }
        }.Concat(banks.Select(bank => new InputValue
        {
            Text = bank.Name,
            Value = bank.Id.ToString()
        })).ToList();
        
        SetSelectedBank(UseStateForQuerying ? State.BankId.ToString() : transactionQuery.BankId.ToString());
    }
    
    private async Task InitAccountSelectList(TransactionQuery transactionQuery)
    {
        var accounts = await _accountProvider.GetAccounts(new AccountQuery {BankId = State.BankId });
        
        AccountSelectList.Items = new List<InputValue>
        {
            new InputValue
            {
                Text = "All",
                Value = "0"
            }
        }.Concat(accounts.Select(account => new InputValue
        {
            Text = $"{account.Name} ({account.Bank.Name})",
            Value = account.Id.ToString()
        })).ToList();
        
        SetSelectedAccount(UseStateForQuerying ? State.AccountId.ToString() : transactionQuery.AccountId.ToString());
    }
    
    private void InitYearSelectList(TransactionQuery transactionQuery)
    {
        YearSelectList.Items = State.Years.Select(year => new InputValue
        {
            Text = year.ToString(),
            Value = year.ToString()
        }).ToList();
        
        SetSelectedYear(UseStateForQuerying ? State.Year.ToString() : transactionQuery.Year.ToString());
    }
    
    private void InitMonthSelectList(TransactionQuery transactionQuery)
    {
        MonthSelectList.Items = State.Months.Select(month => new InputValue
        {
            Text = new DateTime(int.Parse(YearSelectList.Value), month, 1).ToString("MMMM"),
            Value = month.ToString()
        }).ToList();
        
        SetSelectedMonth(UseStateForQuerying ? State.Month.ToString() : transactionQuery.Month.ToString());
    }
    
    private void SetSelectedBank(string bankId)
    {
        BankSelectList.Value = bankId;
        
        if (UseStateForQuerying)
        {
            State.BankId = long.Parse(bankId);
        }
    }
    
    private void SetSelectedAccount(string accountId)
    {
        AccountSelectList.Value = accountId;

        if (UseStateForQuerying)
        {
            State.AccountId = long.Parse(accountId);
        }
    }
    
    private void SetSelectedYear(string year)
    {
        YearSelectList.Value = year;
        
        if (UseStateForQuerying)
        {
            State.Year = int.Parse(year);
        }
    }
    
    private void SetSelectedMonth(string month)
    {
        MonthSelectList.Value = month;
        
        if (UseStateForQuerying)
        {
            State.Month = int.Parse(month);
        }
    }
}