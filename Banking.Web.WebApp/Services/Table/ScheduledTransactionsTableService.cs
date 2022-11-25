using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Shared;
using Banking.Web.WebApp.Components.ScheduledTransactions;
using Banking.Web.WebApp.Extensions;
using Banking.Web.WebApp.Shared;
using Banking.Web.WebApp.Utils;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Extensions;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public class ScheduledTransactionsTableService : TableService<ScheduledTransactionQuery>
{
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    public override Func<UIHelpers, long, Task> OnRowClicked => OpenEditItemDialog;
    
    private InputList<ScheduledTransactionQuery> YearSelectList { get; set; }
    private InputList<ScheduledTransactionQuery> MonthSelectList { get; set; }

    public ScheduledTransactionsTableService(
        IScheduledTransactionProvider scheduledTransactionProvider, 
        State state) : base(state)
    {
        _scheduledTransactionProvider = scheduledTransactionProvider;
    }

    public override void CreateHeaderRow()
    {
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText
            {
                Text = "Date",
                Icon = Icons.Sharp.Update
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
            },
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Occurence" },
            TdClass = "d-none d-md-table-cell d-widget-none"
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Category" },
            TdClass = "d-none d-lg-table-cell d-widget-none"
        });
        
        HeaderRow.Add(new Column
        {
            ColumnText = new ColumnText { Text = "Sub category" },
            TdClass = "d-none d-lg-table-cell d-widget-none"
        });
    }

    public override Task CreateFilters(ScheduledTransactionQuery scheduledTransactionQuery)
    {
        Filter.Clear();
        
        YearSelectList = new InputList<ScheduledTransactionQuery>
        {
            FilterType = FilterType.Select,
            Label = "Year",
            OnChanged = OnYearChanged
        };
        
        Filter.Add(YearSelectList);

        MonthSelectList = new InputList<ScheduledTransactionQuery>
        {
            FilterType = FilterType.Select,
            Label = "Month",
            OnChanged = OnMonthChanged
        };
        
        Filter.Add(MonthSelectList);
        
        Filter.Add(new Checkbox<ScheduledTransactionQuery>
        {
            FilterType = FilterType.Checkbox,
            OnChanged = OnIncludeCompletedChanged,
            Value = scheduledTransactionQuery.IncludeCompletedTransactions,
            Label = "Include completed transactions"
        });
        
        InitYearSelectList(scheduledTransactionQuery);
        InitMonthSelectList(scheduledTransactionQuery);
        
        return Task.CompletedTask;
    }

    private Task OnIncludeCompletedChanged(Checkbox<ScheduledTransactionQuery> checkbox, bool value, ScheduledTransactionQuery scheduledTransactionQuery)
    {
        scheduledTransactionQuery.IncludeCompletedTransactions = value;
        checkbox.Value = value;
        
        State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        
        return Task.CompletedTask;
    }

    public override async Task<IList<TableRow>> FetchData(ScheduledTransactionQuery scheduledTransactionQuery, TableState tableState)
    {
        if (!HideFilter && !Widget && !Filter.Any())
        {
            await CreateFilters(scheduledTransactionQuery);
        }
        
        scheduledTransactionQuery.Take = Widget ? 5 : null;
        
        if (UseStateForQuerying)
        {
            scheduledTransactionQuery.NextTransactionFromDate = DateTimeUtils.FirstDayOfMonth(State.Year, State.Month);
            scheduledTransactionQuery.NextTransactionToDate = DateTimeUtils.LastDayOfMonth(State.Year, State.Month);
        }
        
        var scheduledTransactions = await _scheduledTransactionProvider.GetScheduledTransactions(scheduledTransactionQuery);

        var tableRows = scheduledTransactions.Select(scheduledTransaction => new TableRow
        {
            Id = scheduledTransaction.Id,
            Columns = new List<Column>
            {
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Icon = Widget ? Icons.Sharp.DateRange : null,
                        Text = scheduledTransaction.NextTransactionDate.ToNorwegianDateString()
                    },
                    TdClass = Widget ? "td-width-20 td-md-width-25" : "td-md-width-5 td-width-20"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Icon = Widget ? IconUtils.TransactionDescription : null,
                        Text = scheduledTransaction.Description
                    },
                    TdClass = Widget ? "td-width-50 td-md-width-45" : "td-md-width-25 td-width-50"
                },
                new Column
                {
                    ColumnText = new ColumnText
                    {
                        Icon = Widget ? Icons.Sharp.Money : null,
                        Text = scheduledTransaction.Amount.ToString(CultureInfo.InvariantCulture)
                    },
                    TdClass = Widget ? "td-width-30" : "td-md-width-10 td-width-30"
                },
                new Column
                {
                    ColumnText = new ColumnText { Text = scheduledTransaction.Occurrence.GetEnumDisplayName() },
                    TdClass = "d-none d-md-table-cell d-widget-none td-md-width-20"
                },
                new Column
                {
                    ColumnText = new ColumnText { Text = scheduledTransaction.TransactionSubCategory?.TransactionCategory?.Name.FirstCharToUpper() ?? "N/A" },
                    TdClass = "d-none d-md-table-cell d-widget-none td-md-width-20"
                },
                new Column
                {
                    ColumnText = new ColumnText { Text = scheduledTransaction.TransactionSubCategory?.Name.FirstCharToUpper() ?? "N/A" },
                    TdClass = "d-none d-md-table-cell d-widget-none td-md-width-20"
                }
            }
        }).ToList();
        
        Footer = Widget ? null : new Column { ColumnText = new ColumnText { Text = $"Sum: {scheduledTransactions.Sum(x => x.Amount)}" } };
        
        return tableRows;
    }

    public override async Task OpenFullVersionDialog(UIHelpers uiHelpers, ScheduledTransactionQuery scheduledTransactionQuery)
    {
        await uiHelpers.ShowDialog<ScheduledTransactionsOverviewDialog>();
    }

    public override async Task OpenAddItemDialog(UIHelpers uiHelpers)
    {
        var parameters = new DialogParameters
        {
            { nameof(AddScheduledTransactionDialog.OnScheduledTransactionAdded), OnItemAdded }
        };

        await uiHelpers.ShowDialog<AddScheduledTransactionDialog>(parameters);
    }

    public override async Task OpenEditItemDialog(UIHelpers uiHelpers, long id)
    {
        var parameters = new DialogParameters
        {
            { nameof(EditScheduledTransactionDialog.ScheduledTransactionId), id },
            { nameof(EditScheduledTransactionDialog.OnScheduledTransactionUpdated), OnItemUpdated },
            { nameof(EditScheduledTransactionDialog.OnScheduledTransactionDeleted), OnItemDeleted }
        };

        await uiHelpers.ShowDialog<EditScheduledTransactionDialog>(parameters);
    }
    
    private Task OnYearChanged(
        InputList<ScheduledTransactionQuery> input,
        string value,
        ScheduledTransactionQuery scheduledTransactionQuery)
    {
        if (!UseStateForQuerying && int.TryParse(value, out var year) && int.TryParse(MonthSelectList.Value, out var month))
        {
            scheduledTransactionQuery.NextTransactionFromDate = DateTimeUtils.FirstDayOfMonth(year, month);
            scheduledTransactionQuery.NextTransactionToDate = DateTimeUtils.LastDayOfMonth(year, month);
            State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        }
        
        SetSelectedYear(value);
        
        return Task.CompletedTask;
    }

    private Task OnMonthChanged(
        InputList<ScheduledTransactionQuery> input,
        string value,
        ScheduledTransactionQuery scheduledTransactionQuery)
    {
        if (!UseStateForQuerying && int.TryParse(value, out var month) && int.TryParse(YearSelectList.Value, out var year))
        {
            scheduledTransactionQuery.NextTransactionFromDate = DateTimeUtils.FirstDayOfMonth(year, month);
            scheduledTransactionQuery.NextTransactionToDate = DateTimeUtils.LastDayOfMonth(year, month);
            State.QueryParametersChanged.Invoke(this, EventArgs.Empty);
        }
        
        SetSelectedMonth(value);
        
        return Task.CompletedTask;
    }
    
    private void InitYearSelectList(ScheduledTransactionQuery scheduledTransactionQuery)
    {
        YearSelectList.Items = State.Years.Select(year => new InputValue
        {
            Text = year.ToString(),
            Value = year.ToString()
        }).ToList();
        
        SetSelectedYear(UseStateForQuerying ? State.Year.ToString() : scheduledTransactionQuery.NextTransactionFromDate?.Year.ToString());
    }
    
    private void InitMonthSelectList(ScheduledTransactionQuery scheduledTransactionQuery)
    {
        MonthSelectList.Items = State.Months.Select(month => new InputValue
        {
            Text = new DateTime(int.Parse(YearSelectList.Value), month, 1).ToString("MMMM"),
            Value = month.ToString()
        }).ToList();
        
        SetSelectedMonth(UseStateForQuerying ? State.Month.ToString() : scheduledTransactionQuery.NextTransactionFromDate?.Month.ToString());
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