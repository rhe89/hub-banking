using System.Threading.Tasks;
using Hub.Shared.Storage.Repository.Core;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components;

public class TableBaseComponent<TQuery> : BaseComponent where TQuery : Query, new()
{
    [CascadingParameter(Name = "Query")]
    public TQuery Query { get; set; } = new();
    
    [Parameter]
    public int Take { get; set; }
    
    [Parameter]
    public bool Widget { get; set; }
    
    private bool Rendered { get; set; }

    protected GenericTable<TQuery> TableRef { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Rendered = true;
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }
}