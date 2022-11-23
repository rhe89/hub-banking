using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Microsoft.Extensions.Logging;

namespace Banking.Services;

public interface IBankService
{
    Task<BankDto> AddBank(BankDto newBank, bool saveChanges);
    Task UpdateBank(BankDto updatedBank, bool saveChanges);
    Task<BankDto> GetOrCreateBank(string name, string accountNumberPrefix);
    Task DeleteBank(BankDto bank, bool saveChanges);
}

public class BankService : IBankService
{
    private readonly ICacheableHubDbRepository _dbRepository;
    private readonly IBankProvider _bankProvider;
    private readonly ILogger<BankService> _logger;

    public BankService(
        ICacheableHubDbRepository dbRepository,
        IBankProvider bankProvider,
        ILogger<BankService> logger)
    {
        _dbRepository = dbRepository;
        _bankProvider = bankProvider;
        _logger = logger;
    }
    
    public async Task<BankDto> AddBank(BankDto newBank, bool saveChanges)
    {
        _logger.LogInformation(
            "Creating bank {Name} with account number prefix {Prefix}",
            newBank.Name,
            newBank.AccountNumberPrefix);

        var banksWithSameName = await _bankProvider.GetBanks(new BankQuery
        {
            Name = newBank.Name
        });

        if (banksWithSameName.Any())
        {
            return banksWithSameName.First();
        }

        if (saveChanges)
        {
            return _dbRepository.Add<Bank, BankDto>(newBank);
        }
        
        _dbRepository.QueueAdd<Bank, BankDto>(newBank);
        
        return newBank;
    }

    public async Task UpdateBank(BankDto updatedBank, bool saveChanges)
    {
        _logger.LogInformation("Updating bank {Name} (Id: {Id})", updatedBank.Name, updatedBank.Id);

        var bankInDb = (await _bankProvider.GetBanks(new BankQuery
        {
            Id = updatedBank.Id
        })).First();

        bankInDb.Name = updatedBank.Name;
        bankInDb.AccountNumberPrefix = updatedBank.AccountNumberPrefix;
        
        if (saveChanges)
        {
            await _dbRepository.UpdateAsync<Bank, BankDto>(bankInDb);
        }
        else
        {
            _dbRepository.QueueUpdate<Bank, BankDto>(updatedBank);
        }
    }

    public async Task<BankDto> GetOrCreateBank(string name, string accountNumberPrefix)
    {
        var existingBank = (await _bankProvider.GetBanks(new BankQuery
        {
            AccountNumberPrefix = accountNumberPrefix
        })).FirstOrDefault();

        if (existingBank != null)
        {
            return existingBank;
        }
        
        var newBank = new BankDto
        {
            Name = name,
            AccountNumberPrefix = accountNumberPrefix
        };

        return await AddBank(newBank, true);
    }
    
    public async Task DeleteBank(BankDto bank, bool saveChanges)
    {
        _logger.LogInformation("Deleting bank {Name} (Id: {Id})", bank.Name, bank.Id);

        _dbRepository.QueueRemove<Bank, BankDto>(bank);
        
        if (saveChanges)
        {
            await _dbRepository.ExecuteQueueAsync();
        }
    }
}