using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;

namespace Banking.Web.WebApp;

public class Base : ComponentBase
{
    [Inject]
    private IJSRuntime JsRuntime { get; set; }
    
    [Inject]
    private IDialogService DialogService { get; set; }
    
    [Inject]
    private IResizeService ResizeService { get; set; }
    
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

    protected async Task ShowDialog<TDialog>(DialogParameters dialogParameters)
        where TDialog : ComponentBase
    {
        var windowSize = await ResizeService.GetBrowserWindowSize();
        
        var options = new DialogOptions { CloseOnEscapeKey = true, FullScreen = windowSize.Width <= 600};

        DialogService.Show<TDialog>(null, dialogParameters, options);
    }
    
    
}