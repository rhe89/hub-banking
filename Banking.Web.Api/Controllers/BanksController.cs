﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Query;

namespace Banking.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BanksController : ControllerBase
{
    private readonly IBankProvider _bankProvider;

    public BanksController(IBankProvider bankProvider)
    {
        _bankProvider = bankProvider;
    }

    [HttpPost]
    public async Task<IActionResult> Get(BankQuery bankQuery)
    {
        var banks = await _bankProvider.Get(bankQuery);

        return Ok(banks);
    }
}