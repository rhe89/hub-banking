using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Spreadsheet.Dto;
using Hub.Shared.DataContracts.Spreadsheet.Query;
using Hub.Shared.Web.Http;
using JetBrains.Annotations;

namespace Banking.Integration;

public interface ISpreadsheetApiConnector
{
    Task<IList<IncomeDto>> GetIncome(IncomeQuery query);
    Task<IList<DebtDto>> GetDebt(DebtQuery query);
}

[UsedImplicitly]
public class SpreadsheetApiConnector : HttpClientService, ISpreadsheetApiConnector
{
    private const string IncomePath = "/api/income";
    private const string DebtPath = "/api/debt";

    public SpreadsheetApiConnector(HttpClient httpClient) : base(httpClient, "BankingApi")
    {
    }

    public async Task<IList<IncomeDto>> GetIncome(IncomeQuery query)
    {
        return await Post<IList<IncomeDto>>(IncomePath, query);
    }

    public async Task<IList<DebtDto>> GetDebt(DebtQuery query)
    {
        return await Post<IList<DebtDto>>(DebtPath, query);
    }
}