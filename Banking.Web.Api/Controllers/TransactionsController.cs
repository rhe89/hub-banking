using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.SearchParameters;

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
    public async Task<IActionResult> GetTransactions([FromBody]TransactionSearchParameters transactionSearchParameters)
    {
        var transactions = await _transactionProvider.GetTransactions(transactionSearchParameters);

        return Ok(transactions);
    }
}