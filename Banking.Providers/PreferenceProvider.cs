using System.Collections.Generic;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Providers;

public interface IPreferenceProvider
{
    Task<IList<PreferenceDto>> GetPreferences(string key);
}

public class PreferenceProvider : IPreferenceProvider
{
    private readonly IHubDbRepository _dbRepository;

    public PreferenceProvider(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }
    
    public async Task<IList<PreferenceDto>> GetPreferences(string key)
    {
        return await _dbRepository.WhereAsync<Preference, PreferenceDto>(x => x.Key == key);
    }
}