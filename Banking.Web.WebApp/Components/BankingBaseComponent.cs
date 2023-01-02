using Hub.Shared.Web.BlazorServer.Components;
using Microsoft.AspNetCore.Components;

namespace Banking.Web.WebApp.Components;

public class BankingBaseComponent : BaseComponent
{
    [Inject]
    public BankingState State { get; set; }
}