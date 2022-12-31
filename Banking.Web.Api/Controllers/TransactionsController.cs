using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionProvider _transactionProvider;

    public TransactionsController(ITransactionProvider transactionProvider)
    {
        _transactionProvider = transactionProvider;
    }
    
    [HttpPost]
    public async Task<IActionResult> Get([FromBody]TransactionQuery transactionQuery)
    {
        var transactions = await _transactionProvider.Get(transactionQuery);

        return Ok(transactions);
    }
}