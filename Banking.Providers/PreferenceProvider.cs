using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IPreferenceProvider
{
    Task<AccountDto> GetDefaultSavingsAccount();
    Task<AccountDto> GetDefaultBillingAccount();
    Task<TransactionSubCategoryDto> GetDefaultSavingsCategory();
}

public class PreferenceProvider : IPreferenceProvider
{
    private readonly IAccountProvider _accountProvider;
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    private readonly IHubDbRepository _dbRepository;

    public PreferenceProvider(
        IAccountProvider accountProvider,
        ITransactionCategoryProvider transactionCategoryProvider,
        IHubDbRepository dbRepository)
    {
        _accountProvider = accountProvider;
        _transactionCategoryProvider = transactionCategoryProvider;
        _dbRepository = dbRepository;
    }

    public async Task<AccountDto> GetDefaultSavingsAccount()
    {
        var preference = (await _dbRepository.GetAsync<Preference>()).First(x => x.Key == PreferenceConstants.DefaultSavingsAccountKey);

        var id = long.Parse(preference.Value);

        return (await _accountProvider.Get(new AccountQuery { Id = id })).First();
    }
    
    public async Task<AccountDto> GetDefaultBillingAccount()
    {
        var preference = (await _dbRepository.GetAsync<Preference>()).First(x => x.Key == PreferenceConstants.DefaultBillingAccountKey);

        var id = long.Parse(preference.Value);

        return (await _accountProvider.Get(new AccountQuery { Id = id })).First();
    }
    
    public async Task<TransactionSubCategoryDto> GetDefaultSavingsCategory()
    {
        var preference = (await _dbRepository.GetAsync<Preference>()).First(x => x.Key == PreferenceConstants.DefaultSavingsCategoryKey);

        var id = long.Parse(preference.Value);

        return (await _transactionCategoryProvider.Get(new TransactionSubCategoryQuery { Id = id })).First();
    }

}