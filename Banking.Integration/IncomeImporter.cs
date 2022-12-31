using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Spreadsheet.Dto;
using Hub.Shared.DataContracts.Spreadsheet.Query;

namespace Banking.Integration;

public interface IIncomeImporter
{
    Task<IncomeDto> GetIncomeForMonth(DateTime month);
    Task<IList<IncomeDto>> GetIncomeForYear(DateTime year);
}

public class IncomeImporter : IIncomeImporter
{
    private readonly ISpreadsheetApiConnector _spreadsheetApiConnector;

    public IncomeImporter(ISpreadsheetApiConnector spreadsheetApiConnector)
    {
        _spreadsheetApiConnector = spreadsheetApiConnector;
    }

    public async Task<IncomeDto> GetIncomeForMonth(DateTime month)
    {
        var incomeListForYear = await _spreadsheetApiConnector.GetIncome(new IncomeQuery { FromDate = month });

        //If no income for year exists (e.g in the future), use latest income available
        if (!incomeListForYear.Any())
        {
            month = month.AddYears(-1);
            
            //Use income for december that year
            month = month.AddMonths(12 - month.Month);

            return await GetIncomeForMonth(month);
        }

        return incomeListForYear.First(x => x.Month.Month == month.Month && x.Month.Year == month.Year);
    }

    public async Task<IList<IncomeDto>> GetIncomeForYear(DateTime year)
    {
        var incomeListForYear = await _spreadsheetApiConnector.GetIncome(new IncomeQuery { FromDate = year });

        //If no income for year exists (e.g in the future), use latest income available
        if (!incomeListForYear.Any())
        {
            year = year.AddYears(-1);

            return await GetIncomeForYear(year);
        }

        return incomeListForYear;
    }
}