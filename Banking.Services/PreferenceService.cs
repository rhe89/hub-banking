using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Services;

public interface IPreferenceService
{
    Task<bool> AddPreference(string key, string value);
    Task<bool> AddOrReplacePreference(string key, string value);
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
    
    public async Task<bool> AddPreference(string key, string value)
    {
        var preference = new PreferenceDto
        {
            Key = key,
            Value = value
        };

        await _dbRepository.AddAsync<Preference, PreferenceDto>(preference);

        return true;
    }
    
    public async Task<bool> AddOrReplacePreference(string key, string value)
    {
        var existingPreferences = await _preferenceProvider.GetPreferences(key);

        foreach (var existingPreference in existingPreferences)
        {
            _dbRepository.QueueRemove<Preference, PreferenceDto>(existingPreference);
        }
        
        var preference = new PreferenceDto
        {
            Key = key,
            Value = value
        };

        _dbRepository.QueueAdd<Preference, PreferenceDto>(preference);

        await _dbRepository.ExecuteQueueAsync();

        return true;
    }
}