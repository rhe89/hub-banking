using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Spreadsheet.Dto;
using Hub.Shared.DataContracts.Spreadsheet.Query;

namespace Banking.Integration;

public interface IDebtImporter
{
    Task<IList<DebtDto>> GetDebtForYear(DateTime year);
}

public class DebtImporter : IDebtImporter
{
    private readonly ISpreadsheetApiConnector _spreadsheetApiConnector;

    public DebtImporter(ISpreadsheetApiConnector spreadsheetApiConnector)
    {
        _spreadsheetApiConnector = spreadsheetApiConnector;
    }

    public async Task<IList<DebtDto>> GetDebtForYear(DateTime year)
    {
        var debtForYear = await _spreadsheetApiConnector.GetDebt(new DebtQuery { FromDate = year });

        return debtForYear;
    }
}