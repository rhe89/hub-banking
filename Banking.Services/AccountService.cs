using System.Threading.Tasks;
using Banking.Data.Entities;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.Storage.Repository.Core;

namespace Banking.Services;

public interface IAccountService
{
    Task<bool> AddAccount(AccountDto account);
    Task<bool> UpdateAccount(AccountDto account);
}

public class AccountService : IAccountService
{
    private readonly IHubDbRepository _dbRepository;

    public AccountService(IHubDbRepository dbRepository)
    {
        _dbRepository = dbRepository;
    }
    
    public async Task<bool> AddAccount(AccountDto account)
    {
        await _dbRepository.AddAsync<Account, AccountDto>(account);

        return true;
    }

    public async Task<bool> UpdateAccount(AccountDto account)
    {
        var accountInDb = await _dbRepository.SingleAsync<Account, AccountDto>(a => a.Id == account.Id);

        accountInDb.Name = account.Name;
        accountInDb.AccountType = account.AccountType;
        accountInDb.Bank = account.Bank;

        if (accountInDb.Balance != account.Balance)
        {
            accountInDb.Balance = account.Balance;
            _dbRepository.QueueAdd<AccountBalance, AccountBalanceDto>(new AccountBalanceDto { AccountId = account.Id, Balance = account.Balance});
        }
        
        _dbRepository.QueueUpdate<Account, AccountDto>(account);

        await _dbRepository.ExecuteQueueAsync();

        return true;
    }
}