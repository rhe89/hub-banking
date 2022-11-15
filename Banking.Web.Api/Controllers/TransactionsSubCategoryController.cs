using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionSubCategoriesController : ControllerBase
{
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;

    public TransactionSubCategoriesController(ITransactionCategoryProvider transactionCategoryProvider)
    {
        _transactionCategoryProvider = transactionCategoryProvider;
    }
    
    [HttpPost]
    public async Task<IActionResult> GetTransactionSubCategories([FromBody]TransactionSubCategoryQuery transactionSubCategoryQuery)
    {
        var transactionSubCategories = await _transactionCategoryProvider.GetTransactionSubCategories(transactionSubCategoryQuery);

        return Ok(transactionSubCategories);
    }
}