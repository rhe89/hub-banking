using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Banking.Web.WebApp;

public class Dialog : Base
{
    [CascadingParameter] 
    public MudDialogInstance MudDialog { get; set; }

}