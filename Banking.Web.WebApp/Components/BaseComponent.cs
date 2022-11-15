using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components;

public class BaseComponent : ComponentBase
{
    [Inject]
    public State State { get; set; }

    public bool Working { get; set; }
}