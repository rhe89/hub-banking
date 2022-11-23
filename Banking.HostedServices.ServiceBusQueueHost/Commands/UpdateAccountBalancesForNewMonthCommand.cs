using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Services;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class UpdateAccountBalancesForNewMonthCommand : ServiceBusQueueCommand
{
    private readonly IAccountProvider _accountProvider;
    private readonly IAccountService _accountService;

    public UpdateAccountBalancesForNewMonthCommand(
        IAccountProvider accountProvider,
        IAccountService accountService)
    {
        _accountProvider = accountProvider;
        _accountService = accountService;
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        var accounts = await _accountProvider.GetAccounts(new AccountQuery
        {
            IncludeDiscontinuedAccounts = false,
            IncludeSharedAccounts = true,
            BalanceToDate = DateTimeUtils.LastDayOfMonth()
        });

        accounts = accounts.Where(x => !x.BalanceIsAccumulated).ToList();

        var firstDayOfCurrentMonth = DateTimeUtils.FirstDayOfMonth();

        foreach (var account in accounts)
        {
            if (account.BalanceDate < firstDayOfCurrentMonth)
            {
                account.BalanceDate = firstDayOfCurrentMonth;
                account.Balance = account.Balance;

                await _accountService.UpdateAccount(account, false);
            }
        }

        await _accountService.SaveChanges();
    }

    public override string Trigger => QueueNames.UpdateAccountBalancesForNewMonth;
}