using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Services;

public interface IPreferenceService
{
    Task<bool> AddOrUpdatePreference(string key, string value);
}

public class PreferenceService : IPreferenceService
{
    private readonly IHubDbRepository _dbRepository;
    private readonly IPreferenceProvider _preferenceProvider;

    public PreferenceService(IHubDbRepository dbRepository, IPreferenceProvider preferenceProvider)
    {
        _dbRepository = dbRepository;
        _preferenceProvider = preferenceProvider;
    }

    public async Task<bool> AddOrUpdatePreference(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
        {
            return false;
        }
        
        var existingPreferences = await _preferenceProvider.GetPreferences(key);

        var existingPreference = existingPreferences.FirstOrDefault();

        if (existingPreference == null)
        {
            var preference = new PreferenceDto
            {
                Key = key,
                Value = value
            };

            await _dbRepository.AddAsync<Preference, PreferenceDto>(preference);
        }
        else if (existingPreference.Value != value)
        {
            existingPreference.Value = value;
            await _dbRepository.UpdateAsync<Preference, PreferenceDto>(existingPreference);
        }
        
        return true;
    }
}