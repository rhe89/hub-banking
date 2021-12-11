using System;
using System.Linq;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Banking;
using Hub.Shared.Storage.Repository.Core;
using Microsoft.Extensions.Logging;
using Sbanken.Data.Entities;
using AccountDto = Hub.Shared.DataContracts.Sbanken.AccountDto;

namespace Sbanken.HostedServices.ServiceBusQueueHost.CommandHandlers;

public interface IUpdateSbankenAccountBalanceHistoryCommandHandler
{
    Task UpdateAccountBalance();
}
    
public class UpdateSbankenAccountBalanceHistoryCommandHandler : IUpdateSbankenAccountBalanceHistoryCommandHandler
{
    private readonly IHubDbRepository _dbRepository;
    private readonly ILogger<UpdateSbankenAccountBalanceHistoryCommandHandler> _logger;

    public UpdateSbankenAccountBalanceHistoryCommandHandler(ILogger<UpdateSbankenAccountBalanceHistoryCommandHandler> logger,
        IHubDbRepository dbRepository)
    {
        _logger = logger;
        _dbRepository = dbRepository;
    }
        
    public async Task UpdateAccountBalance()
    {
        var accounts = await _dbRepository.AllAsync<Account, AccountDto>();
            
        foreach (var account in accounts)
        {
            var now = DateTime.Now;

            _logger.LogInformation("Updating account balance history for account {AccountName}", account.Name);
                
            var accountBalanceForCurrentDay = GetAccountBalanceForCurrentDay(account, now);
            
            if (accountBalanceForCurrentDay == null)
            {
                AddAccountBalance(account);
            }
            else
            {
                UpdateAccountBalance(accountBalanceForCurrentDay, account);
            }            
        }
            
        await _dbRepository.ExecuteQueueAsync();
    }

    private AccountBalanceDto GetAccountBalanceForCurrentDay(AccountDto account, DateTime now)
    {
        return _dbRepository.All<AccountBalance, AccountBalanceDto>().FirstOrDefault(x =>
            x.AccountId == account.Id &&
            x.CreatedDate.Year == now.Year &&
            x.CreatedDate.Month == now.Month &&
            x.CreatedDate.Day == now.Day);
    }

        
    private void AddAccountBalance(AccountDto account)
    {
        var accountBalanceForCurrentDay = new AccountBalanceDto
        {
            AccountId = account.Id,
            Balance = account.Balance
        };

        _dbRepository.QueueAdd<AccountBalance, AccountBalanceDto>(accountBalanceForCurrentDay);
    }
        
    private void UpdateAccountBalance(AccountBalanceDto accountBalanceForCurrentDay, AccountDto account)
    {
        accountBalanceForCurrentDay.Balance = account.Balance;

        _dbRepository.QueueUpdate<AccountBalance, AccountBalanceDto>(accountBalanceForCurrentDay);
    }

}