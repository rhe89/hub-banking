using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Banking.Integration.Dto;
using Hub.Shared.Web.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.Integration;

public interface ISbankenConnector
{
    Task<List<SbankenAccount>> GetAccounts();
    Task<IList<SbankenTransaction>> GetTransactions(DateTime startDate, DateTime? endDate);
    Task<IList<object>> GetTransactionsRaw();
    Task<IList<object>> GetTransactionsRaw(string accountName);
    Task<IList<object>> GetArchivedTransactionsRaw();
    Task<IList<object>> GetArchivedTransactionsRaw(string accountName);
    Task<object> GetAccountsRaw();
}

[UsedImplicitly]
public class SbankenConnector : HttpClientService, ISbankenConnector
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SbankenConnector> _logger;
    private const string SbankenApiBaseAddress = "SbankenApiBaseAddress";
    private const string SbankenApiCustomerId = "SbankenApiCustomerId";
    private const string SbankenApiBasePath = "SbankenApiBasePath";
    private const string SbankenApiClientId = "SbankenApiClientId";
    private const string SbankenApiSecret = "SbankenApiSecret";
    private const string SbankenApiDiscoveryEndpoint = "SbankenApiDiscoveryEndpoint";

    public SbankenConnector(
        IConfiguration configuration,
        ILogger<SbankenConnector> logger,
        HttpClient httpClient)
        : base(httpClient, "Sbanken")
    {
        _configuration = configuration;
        _logger = logger;

        var customerId = _configuration.GetValue<string>(SbankenApiCustomerId);

        HttpClient.DefaultRequestHeaders.Add("customerId", customerId);

        var apiBaseAddress = _configuration.GetValue<string>(SbankenApiBaseAddress);

        HttpClient.BaseAddress = new Uri(apiBaseAddress);
    }

    public async Task<List<SbankenAccount>> GetAccounts()
    {
        await AuthenticateClient();

        var bankBasePath = _configuration.GetValue<string>(SbankenApiBasePath);

        var endpoint = $"{bankBasePath}/api/v1/Accounts";

        var accountsResponse = await Get<SbankenAccountResponse>(endpoint);

        return accountsResponse.Items;
    }

    public async Task<IList<SbankenTransaction>> GetTransactions(DateTime startDate, DateTime? endDate)
    {
        var query = endDate.HasValue
            ? $"startDate={startDate:yyyy-MM-dd}&endDate={endDate.Value:yyyy-MM-dd}"
            : $"startDate={startDate:yyyy-MM-dd}&length=1000";

        return await GetTransactions(query);
    }

    private async Task<IList<SbankenTransaction>> GetTransactions(string requestParameters)
    {
        await AuthenticateClient();

        var accounts = await GetAccounts();

        var bankBasePath = _configuration.GetValue<string>(SbankenApiBasePath);

        var transactions = new List<SbankenTransaction>();

        foreach (var account in accounts)
        {
            _logger.LogInformation("Getting transactions in account {AccountName}", account.Name);

            var endpoint = $"{bankBasePath}/api/v1/transactions/archive/{account.AccountId}";

            SbankenTransactionResponse transactionResponse;

            try
            {
                transactionResponse = await Get<SbankenTransactionResponse>(endpoint, requestParameters);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting transactions for account {AccountName}", account.Name);
                continue;
            }

            var transactionsInAccount = transactionResponse.Items;

            _logger.LogInformation("Got {Count} transactions", transactionsInAccount.Count);

            foreach (var item in transactionsInAccount)
            {
                item.AccountName = account.Name;
            }

            transactions.AddRange(transactionsInAccount);
        }

        return transactions;
    }

    public Task<IList<object>> GetTransactionsRaw()
    {
        return GetTransactionsRaw(null);
    }

    public async Task<IList<object>> GetTransactionsRaw(string accountName)
    {
        await AuthenticateClient();

        var accounts = await GetAccounts();

        if (accountName != null)
        {
            accounts = accounts.Where(x => x.Name == accountName).ToList();
        }

        var bankBasePath = _configuration.GetValue<string>(SbankenApiBasePath);

        var transactions = new List<object>();

        var query = $"startDate={DateTime.Now.AddDays(-30).ToShortDateString()}&length=1000";

        foreach (var account in accounts)
        {
            var endpoint = $"{bankBasePath}/api/v1/transactions/{account.AccountId}";

            object transactionsInAccount;

            try
            {
                transactionsInAccount = await Get<object>(endpoint, query);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting transactions for account {AccountName}", account.Name);
                continue;
            }

            transactions.Add(transactionsInAccount);
        }

        return transactions;
    }

    public Task<IList<object>> GetArchivedTransactionsRaw()
    {
        return GetArchivedTransactionsRaw(null);
    }

    public async Task<IList<object>> GetArchivedTransactionsRaw(string accountName)
    {
        await AuthenticateClient();

        var accounts = await GetAccounts();

        if (accountName != null)
        {
            accounts = accounts.Where(x => x.Name == accountName).ToList();
        }

        var bankBasePath = _configuration.GetValue<string>(SbankenApiBasePath);

        var transactions = new List<object>();

        var query = $"startDate={DateTime.Now.AddDays(-30).ToShortDateString()}&length=1000";

        foreach (var account in accounts)
        {
            var endpoint = $"{bankBasePath}/api/v1/transactions/archive/{account.AccountId}";

            object transactionsInAccount;

            try
            {
                transactionsInAccount = await Get<object>(endpoint, query);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting transactions for account {AccountName}", account.Name);
                continue;
            }

            transactions.Add(transactionsInAccount);
        }

        return transactions;
    }

    public async Task<object> GetAccountsRaw()
    {
        await AuthenticateClient();

        var bankBasePath = _configuration.GetValue<string>(SbankenApiBasePath);

        var endpoint = $"{bankBasePath}/api/v1/Accounts";

        var response = await Get<object>(endpoint);

        return response;
    }

    private async Task AuthenticateClient()
    {
        var clientId = _configuration.GetValue<string>(SbankenApiClientId);
        var secret = _configuration.GetValue<string>(SbankenApiSecret);
        var discoveryEndpoint = _configuration.GetValue<string>(SbankenApiDiscoveryEndpoint);

        await RequestClientCredentialsTokenAsync(discoveryEndpoint, clientId, secret);
    }
}