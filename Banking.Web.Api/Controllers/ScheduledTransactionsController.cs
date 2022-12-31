using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduledTransactionsController : ControllerBase
{
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;

    public ScheduledTransactionsController(IScheduledTransactionProvider scheduledTransactionProvider)
    {
        _scheduledTransactionProvider = scheduledTransactionProvider;
    }
    
    [HttpPost]
    public async Task<IActionResult> Get([FromBody]ScheduledTransactionQuery scheduledTransactionQuery)
    {
        var transactions = await _scheduledTransactionProvider.Get(scheduledTransactionQuery);

        return Ok(transactions);
    }
}