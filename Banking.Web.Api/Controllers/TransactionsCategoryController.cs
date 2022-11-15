using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionCategoriesController : ControllerBase
{
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;

    public TransactionCategoriesController(ITransactionCategoryProvider transactionCategoryProvider)
    {
        _transactionCategoryProvider = transactionCategoryProvider;
    }
    
    [HttpPost]
    public async Task<IActionResult> GetTransactionCategories([FromBody]TransactionCategoryQuery transactionCategoryQuery)
    {
        var transactionCategories = await _transactionCategoryProvider.GetTransactionCategories(transactionCategoryQuery);

        return Ok(transactionCategories);
    }
}