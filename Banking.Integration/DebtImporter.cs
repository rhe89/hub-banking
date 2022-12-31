using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Spreadsheet.Dto;
using Hub.Shared.DataContracts.Spreadsheet.Query;

namespace Banking.Integration;

public interface IDebtImporter
{
    Task<DebtDto> GetDebtForMonth(DateTime month);
    Task<IList<DebtDto>> GetDebtForYear(DateTime year);
}

public class DebtImporter : IDebtImporter
{
    private readonly ISpreadsheetApiConnector _spreadsheetApiConnector;

    public DebtImporter(ISpreadsheetApiConnector spreadsheetApiConnector)
    {
        _spreadsheetApiConnector = spreadsheetApiConnector;
    }

    public async Task<DebtDto> GetDebtForMonth(DateTime month)
    {
        var debtForYear = await _spreadsheetApiConnector.GetDebt(new DebtQuery { FromDate = month });

        //If no income for year exists (e.g in the future), use latest income available
        if (!debtForYear.Any())
        {
            month = month.AddYears(-1);
            
            //Use income for december that year
            month = month.AddMonths(12 - month.Month);

            return await GetDebtForMonth(month);
        }

        return debtForYear.First(x => x.Month.Month == month.Month && x.Month.Year == month.Year);
    }
    
    public async Task<IList<DebtDto>> GetDebtForYear(DateTime year)
    {
        var debtForYear = await _spreadsheetApiConnector.GetDebt(new DebtQuery { FromDate = year });

        return debtForYear;
    }
}