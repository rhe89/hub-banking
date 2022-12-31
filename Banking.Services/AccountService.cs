using System;
using System.Linq;
using System.Threading.Tasks;
using Banking.Data.Entities;
using Banking.Providers;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.Storage.Repository;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Banking.Services;

public interface IAccountService
{
    Task<AccountDto> Add(AccountDto newAccount, bool saveChanges);
    Task<bool> Update(AccountDto updatedAccount, bool saveChanges);

    Task<AccountDto> GetOrAdd(
        string name,
        string accountType,
        string accountNumber);

    Task Delete(AccountDto account);
    Task SaveChanges();
}

public class AccountService : IAccountService
{
    private readonly ICacheableHubDbRepository _dbRepository;
    private readonly IAccountProvider _accountProvider;
    private readonly IBankService _bankService;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<AccountService> _logger;

    public AccountService(ICacheableHubDbRepository dbRepository, 
                          IAccountProvider accountProvider,
                          IBankService bankService,
                          IMessageSender messageSender,
                          ILogger<AccountService> logger)
    {
        _dbRepository = dbRepository;
        _accountProvider = accountProvider;
        _bankService = bankService;
        _messageSender = messageSender;
        _logger = logger;
    }
    
    public async Task<AccountDto> Add(AccountDto newAccount, bool saveChanges)
    {
        var accountsWithSameName = await _accountProvider.Get(new AccountQuery
        {
            AccountName = newAccount.Name,
            BankId = newAccount.BankId
        });
        
        if (accountsWithSameName.Any())
        {
            return accountsWithSameName.First();
        }
        
        _logger.LogInformation("Creating account {Name}", newAccount.Name);

        var addedAccount = await _dbRepository.AddAsync<Account, AccountDto>(newAccount);

        if (newAccount.BalanceDate != null)
        {
            await UpdateAccountBalance(addedAccount.Id, newAccount.BalanceDate.Value, newAccount.Balance, saveChanges);
        }
        
        await NotifyConsumers();

        return addedAccount;
    }

    public async Task<bool> Update(AccountDto updatedAccount, bool saveChanges)
    {
        _logger.LogInformation("Updating account {Name} (Id: {Id})", updatedAccount.Name, updatedAccount.Id);

        var accountInDb = (await _accountProvider.Get(new AccountQuery
        {
            Id = updatedAccount.Id,
            BalanceToDate = DateTimeUtils.Today
        })).First();

        accountInDb.BankId = updatedAccount.BankId;
        accountInDb.Name = updatedAccount.Name;
        accountInDb.AccountNumber = updatedAccount.AccountNumber;
        accountInDb.AccountType = updatedAccount.AccountType;
        accountInDb.SharedAccount = updatedAccount.SharedAccount;
        accountInDb.DiscontinuedDate = updatedAccount.DiscontinuedDate;

        if (!accountInDb.BalanceIsAccumulated && 
            accountInDb.BalanceDate != updatedAccount.BalanceDate &&
            updatedAccount.BalanceDate != null)
        {
            await UpdateAccountBalance(accountInDb.Id, updatedAccount.BalanceDate.Value, updatedAccount.Balance, saveChanges);
        }

        if (saveChanges)
        {
            await _dbRepository.UpdateAsync<Account, AccountDto>(accountInDb);
            await NotifyConsumers();
        }
        else
        {
            _dbRepository.QueueUpdate<Account, AccountDto>(accountInDb);
        }
        
        return true;
    }
    
    public async Task<AccountDto> GetOrAdd(
        string name,
        string accountType,
        string accountNumber)
    {
        var existingAccount = (await _accountProvider.Get(new AccountQuery
        {
            AccountNumber = accountNumber,
            IncludeExternalAccounts = true,
        })).FirstOrDefault();

        if (existingAccount != null)
        {
            return existingAccount;
        }
        
        var bank = await _bankService.GetOrAdd("", accountNumber[..4]);
            
        var newAccount = new AccountDto
        {
            BankId = bank.Id,
            Name = name,
            AccountNumber = accountNumber,
            AccountType = accountType
        };

        return await Add(newAccount, true);
    }
    
    private async Task UpdateAccountBalance(long accountId, DateTime balanceDate, decimal balance, bool saveChanges)
    {
        var newAccountBalance = new AccountBalanceDto
        {
            AccountId = accountId,
            BalanceDate = balanceDate,
            Balance = balance
        };

        if (saveChanges)
        {
            await _dbRepository.AddAsync<AccountBalance, AccountBalanceDto>(newAccountBalance);
        }
        else
        {
            _dbRepository.QueueAdd<AccountBalance, AccountBalanceDto>(newAccountBalance);
        }
    }

    public async Task Delete(AccountDto account)
    {
        _logger.LogInformation("Deleting account {Name} (Id: {Id})", account.Name, account.Id);

        await _dbRepository.RemoveAsync<Account, AccountDto>(account);
    }

    public async Task SaveChanges()
    {
        await _dbRepository.ExecuteQueueAsync();

        await NotifyConsumers();
    }

    private async Task NotifyConsumers()
    {
        await _messageSender.AddToQueue(QueueNames.BankingAccountsUpdated);
        await _messageSender.AddToQueue(QueueNames.CalculateCreditCardPayments);
    }
}