using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Spreadsheet.Dto;
using Hub.Shared.DataContracts.Spreadsheet.Query;
using Microsoft.Extensions.Logging;

namespace Banking.Integration;

public interface IIncomeImporter
{
    Task<IList<IncomeDto>> GetIncomeForYear(DateTime year);
}

public class IncomeImporter : IIncomeImporter
{
    private readonly ISpreadsheetApiConnector _spreadsheetApiConnector;
    private readonly ILogger<IncomeImporter> _logger;

    public IncomeImporter(
        ISpreadsheetApiConnector spreadsheetApiConnector,
        ILogger<IncomeImporter> logger)
    {
        _spreadsheetApiConnector = spreadsheetApiConnector;
        _logger = logger;
    }

    public async Task<IList<IncomeDto>> GetIncomeForYear(DateTime year)
    {
        _logger.LogInformation("Fetching income for year {Year}", year);

        var incomeListForYear = await _spreadsheetApiConnector.GetIncome(new IncomeQuery { FromDate = year });

        if (year.Year == 2021)
        {
            _logger.LogWarning("No income found");

            return new List<IncomeDto>();
        }
        
        //If no income for year exists (e.g in the future), use latest income available
        if (!incomeListForYear.Any())
        {
            _logger.LogInformation("No income found for year {Year}", year);
            year = year.AddYears(-1);

            return await GetIncomeForYear(year);
        }

        return incomeListForYear;
    }
}