using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;

namespace Banking.Web.WebApp.Shared;

public sealed class UIHelpers : ComponentBase
{
    private readonly IDialogService _dialogService;
    private readonly IResizeService _resizeService;
    private readonly ISnackbar _snackbar;

    public UIHelpers(
        IDialogService dialogService,
        IResizeService resizeService,
        ISnackbar snackbar)
    {
        _dialogService = dialogService;
        _resizeService = resizeService;
        _snackbar = snackbar;
    }
    
    public async Task ShowDialog<TDialog>()
        where TDialog : ComponentBase
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, FullScreen = await IsDesktop()};

        _dialogService.Show<TDialog>(null, options);
    }

    public async Task ShowDialog<TDialog>(DialogParameters dialogParameters)
        where TDialog : ComponentBase
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, FullScreen = await IsDesktop()};

        _dialogService.Show<TDialog>(null, dialogParameters, options);
    }
    
    public async Task ShowDialog(
        Type component,
        DialogParameters dialogParameters)
    {
        var options = new DialogOptions { CloseOnEscapeKey = true, FullScreen = await IsDesktop()};

        _dialogService.Show(component, null, dialogParameters, options);
    }
    
    public void ShowSnackbar(string message, Severity severity)
    {
        _snackbar.Add(message, severity, options =>
        {
            options.VisibleStateDuration = 2000;
            options.ShowTransitionDuration = 200;
            options.HideTransitionDuration = 200;
        });
    }

    public async Task<bool> IsDesktop()
    {
        var windowSize = await _resizeService.GetBrowserWindowSize();

        return windowSize.Width > 600;
    }
}