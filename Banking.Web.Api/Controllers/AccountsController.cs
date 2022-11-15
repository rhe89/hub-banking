using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountProvider _accountProvider;

    public AccountsController(IAccountProvider accountProvider)
    {
        _accountProvider = accountProvider;
    }

    [HttpPost]
    public async Task<IActionResult> GetAccounts(AccountQuery accountQuery)
    {
        var accounts = await _accountProvider.GetAccounts(accountQuery);

        return Ok(accounts);
    }
}