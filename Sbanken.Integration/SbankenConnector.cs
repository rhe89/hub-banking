using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hub.Storage.Core.Providers;
using Hub.Web.Http;
using Microsoft.Extensions.Logging;
using Sbanken.Core.Constants;
using Sbanken.Core.Dto.Sbanken;
using Sbanken.Core.Integration;

namespace Sbanken.Integration
{
    public class SbankenConnector : HttpClientService, ISbankenConnector
    {
        private readonly ISettingProvider _settingProvider;

        public SbankenConnector(ISettingProvider settingProvider, HttpClient httpClient, ILogger<SbankenConnector> logger) 
            : base(httpClient, logger, "Sbanken")
        {
            _settingProvider = settingProvider;
            
            var customerId = _settingProvider.GetSetting<string>(SettingConstants.SbankenCustomerId);

            HttpClient.DefaultRequestHeaders.Add("customerId", customerId);
            
            var apiBaseAddress = _settingProvider.GetSetting<string>(SettingConstants.SbankenApiBaseAddress);

            HttpClient.BaseAddress = new Uri(apiBaseAddress);
        }

        public async Task<List<SbankenAccount>> GetAccounts()
        {
            await AuthenticateClient();

            var bankBasePath = _settingProvider.GetSetting<string>(SettingConstants.SbankenBankBasePath);

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
            
            var bankBasePath = _settingProvider.GetSetting<string>(SettingConstants.SbankenBankBasePath);

            var transactions = new List<SbankenTransaction>();

            foreach (var account in accounts)
            {
                var endpoint = $"{bankBasePath}/api/v1/transactions/{account.AccountId}";

                var response = await Get<SbankenTransactionResponse>(endpoint, requestParameters);

                if (!response.Success)
                {
                    continue;
                }
                
                foreach (var item in response.Data.Items)
                {
                    item.AccountName = account.Name;
                }

                transactions.AddRange(response.Data.Items);
            }

            return transactions;
        }

       

        private async Task AuthenticateClient()
        {
            var clientId = _settingProvider.GetSetting<string>(SettingConstants.SbankenClientId);
            var secret = _settingProvider.GetSetting<string>(SettingConstants.SbankenSecret);
            var discoveryEndpoint = _settingProvider.GetSetting<string>(SettingConstants.SbankenDiscoveryEndpoint);
            
            var tokenResponse = await HttpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = discoveryEndpoint,
                ClientId = clientId,
                ClientSecret = secret,
            });
            
            if (tokenResponse.IsError)
            {
                throw new HttpRequestException(tokenResponse.ErrorDescription);
            }

            HttpClient.SetBearerToken(tokenResponse.AccessToken);
        }
    }
}
