using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Banking.Web.WebApp;

public class Base : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    protected void GoTo(string uri)
    {
        NavigationManager.NavigateTo(uri);
    }
    
    protected async Task GoBack()
    {
        await JsRuntime.InvokeVoidAsync("history.go", -1);
    }
}