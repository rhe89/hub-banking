using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hub.Settings.Core;
using Hub.Web.Http;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Constants;
using Sbanken.Core.Dto.Integration;
using Sbanken.Core.Integration;

namespace Sbanken.Integration
{
    public class SbankenConnector : HttpClientService, ISbankenConnector
    {
        private readonly ISettingProvider _settingProvider;
        private readonly ILogger<SbankenConnector> _logger;

        public SbankenConnector(ISettingProvider settingProvider, 
            ILogger<SbankenConnector> logger,
            HttpClient httpClient) 
            : base(httpClient, "Sbanken")
        {
            _settingProvider = settingProvider;
            _logger = logger;

            var customerId = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiCustomerId);

            HttpClient.DefaultRequestHeaders.Add("customerId", customerId);
            
            var apiBaseAddress = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiBaseAddress);

            HttpClient.BaseAddress = new Uri(apiBaseAddress);
        }

        public async Task<List<SbankenAccount>> GetAccounts()
        {
            await AuthenticateClient();

            var bankBasePath = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiBasePath);

            var endpoint = $"{bankBasePath}/api/v1/Accounts";
            
            var response = await Get<SbankenAccountResponse>(endpoint);

            return !response.Success ? new List<SbankenAccount>() : response.Data.Items;
        }

        public async Task<IList<SbankenTransaction>> GetTransactions(DateTime startDate, DateTime? endDate)
        {
            var query = endDate.HasValue ? 
                $"startDate={startDate.ToShortDateString()}&endDate={endDate.Value.ToShortDateString()}" : 
                $"startDate={startDate.ToShortDateString()}&length=1000";

            return await GetTransactions(query);
        }
        
        private async Task<IList<SbankenTransaction>> GetTransactions(string requestParameters)
        {
            await AuthenticateClient();

            var accounts = await GetAccounts();
            
            var bankBasePath = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiBasePath);

            var transactions = new List<SbankenTransaction>();

            foreach (var account in accounts)
            {
                _logger.LogInformation($"Getting transactions in account {account.Name}");
                
                var endpoint = $"{bankBasePath}/api/v1/transactions/archive/{account.AccountId}";

                var response = await Get<SbankenTransactionResponse>(endpoint, requestParameters);

                if (!response.Success)
                {
                    _logger.LogInformation($"Error occured: {response.ErrorMessage}");
                    continue;
                }
                
                _logger.LogInformation($"Got {response.Data.Items.Count} transactions");

                foreach (var item in response.Data.Items)
                {
                    item.AccountName = account.Name;
                }

                transactions.AddRange(response.Data.Items);
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
            
            var bankBasePath = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiBasePath);

            var transactions = new List<object>();

            var query = $"startDate={DateTime.Now.AddDays(-30).ToShortDateString()}&length=1000";
            
            foreach (var account in accounts)
            {
                var endpoint = $"{bankBasePath}/api/v1/transactions/{account.AccountId}";

                var response = await Get<object>(endpoint, query);

                if (!response.Success)
                {
                    continue;
                }

                transactions.Add(response.Data);
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
            
            var bankBasePath = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiBasePath);

            var transactions = new List<object>();

            var query = $"startDate={DateTime.Now.AddDays(-30).ToShortDateString()}&length=1000";
            
            foreach (var account in accounts)
            {
                var endpoint = $"{bankBasePath}/api/v1/transactions/archive/{account.AccountId}";

                var response = await Get<object>(endpoint, query);

                if (!response.Success)
                {
                    continue;
                }

                transactions.Add(response.Data);
            }

            return transactions;
        }
        
        public async Task<object> GetAccountsRaw()
        {
            await AuthenticateClient();

            var bankBasePath = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiBasePath);

            var endpoint = $"{bankBasePath}/api/v1/Accounts";
            
            var response = await Get<object>(endpoint);

            return response;
        }
        
        private async Task AuthenticateClient()
        {
            var clientId = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiClientId);
            var secret = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiSecret);
            var discoveryEndpoint = _settingProvider.GetSetting<string>(SettingKeys.SbankenApiDiscoveryEndpoint);
            
            var tokenResponse = await HttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryEndpoint,
                ClientId = clientId,
                ClientSecret = secret
            });
            
            if (tokenResponse.IsError)
            {
                throw new HttpRequestException(tokenResponse.ErrorDescription);
            }

            HttpClient.SetBearerToken(tokenResponse.AccessToken);
        }
    }
}
