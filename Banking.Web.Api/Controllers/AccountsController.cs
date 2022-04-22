using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.SearchParameters;

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
    public async Task<IActionResult> GetAccounts(AccountSearchParameters accountSearchParameters)
    {
        var accounts = await _accountProvider.GetAccounts(accountSearchParameters);

        return Ok(accounts);
    }
}