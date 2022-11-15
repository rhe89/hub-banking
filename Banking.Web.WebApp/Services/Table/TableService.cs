using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Banking.Web.WebApp.Shared;
using Hub.Shared.Storage.Repository.Core;
using MudBlazor;

namespace Banking.Web.WebApp.Services.Table;

public abstract class TableService<TQuery> where TQuery : Query, new()
{
    protected readonly State State;
    public IList<Input> Filter { get; } = new List<Input>();
    public IList<Column> HeaderRow { get; } = new List<Column>();
    public Column Footer { get; set; }
    public HashSet<TableRow> SelectedItems { get; set; } = new();
    public MudTable<TableRow> MudTableRef { get; set; }
    public abstract Func<UIHelpers, long, Task> OnRowClicked { get; }
    public int FullSizeColumns => HeaderRow.Count;
    public int WidgetColumnsCount => HeaderRow.Count(x => x.TdClass == null || !x.TdClass.Contains("d-widget-none"));
    public bool Widget { get; set; }
    public bool HideFilter { get; set; }
    public bool UseStateForQuerying { get; set; } = true;

    public abstract void CreateHeaderRow();
    public abstract Task CreateFilters(TQuery query);
    public abstract Task<IList<TableRow>> FetchData(TQuery query, TableState tablestate);
    public abstract Task OpenFullVersionDialog(UIHelpers uiHelpers, TQuery query);
    public abstract Task OpenAddItemDialog(UIHelpers uiHelpers);
    public abstract Task OpenEditItemDialog(UIHelpers uiHelpers, long id);

    protected TableService(State state)
    {
        State = state;
    }
    protected async Task ReloadServerData()
    {
        if (MudTableRef != null)
        {
            await MudTableRef.ReloadServerData();
        }
    }

    protected async Task OnItemAdded(UIHelpers uiHelpers, long id)
    {
        await ReloadServerData();
        
        uiHelpers.ShowSnackbar("Item added", Severity.Success);
    }

    protected async Task OnItemUpdated(UIHelpers uiHelpers, long id)
    {
        await ReloadServerData();
        
        uiHelpers.ShowSnackbar("Item saved", Severity.Success);
    }

    protected async Task OnItemDeleted(UIHelpers uiHelpers, long id)
    {
        await ReloadServerData();
        
        uiHelpers.ShowSnackbar("Item deleted", Severity.Success);
    }
}

public class TableRow
{
    public long Id { get; init; }
    public IList<Column> Columns { get; init; }
    public DateTime? ValueToOrderBy { get; init; }
}

public class Column
{
    public Column()
    {
        ChildElements = new List<ColumnText>();
    }

    public string TdClass { get; init; }
    
    public ColumnText ColumnText
    {
        get => ChildElements.FirstOrDefault();
        init => ChildElements.Add(value);
    }

    public IList<ColumnText> ChildElements { get; init; }
}

public class ColumnText
{
    public string Icon { get; init; }
    public string Text { get; init; }
    public string SmallText { get; init; }
    public string Class { get; init; }
    public Color Color { get; init; }
}

public class Input
{
    public bool Enabled { get; set; } = true;
    public FilterType FilterType { get; init; } = FilterType.TextField;
    public string Label { get; init; }
}

public class Checkbox<TQuery> : Input where TQuery : Query, new()
{
    public bool Value { get; set; }
    public Func<Checkbox<TQuery>, bool, TQuery, Task> OnChanged { get; init; }
}

public class InputList<TQuery> : Input where TQuery : Query, new()
{
    public IList<InputValue> Items { get; set; } = new List<InputValue>();
    public string Value { get; set; }
    public Func<InputList<TQuery>, string, TQuery, Task> OnChanged { get; init; }
}

public class InputValue
{
    public string Value { get; init; }
    public string Text { get; init; }
}

public enum FilterType
{
    TextField,
    Select,
    Checkbox,
    Radio
} 