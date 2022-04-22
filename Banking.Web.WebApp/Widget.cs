using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp;

public class Widget : Base
{
    [Parameter]
    public string Class { get; set; }
    
    [Parameter]
    public bool WidgetMode { get; set; }
    //
    // [Parameter]
    // public EventCallback<MouseEventArgs> OnClick { get; set; }
}