using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Services;
using Hub.Shared.DataContracts.Banking.Constants;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Settings;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class CreditCardPaymentCalculatorCommand : ServiceBusQueueCommand
{
    private readonly IAccountProvider _accountProvider;
    private readonly IScheduledTransactionService _scheduledTransactionService;
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    private readonly ISettingProvider _settingProvider;

    public CreditCardPaymentCalculatorCommand(
        IAccountProvider accountProvider,
        IScheduledTransactionService scheduledTransactionService,
        ITransactionCategoryProvider transactionCategoryProvider,
        ISettingProvider settingProvider)
    {
        _accountProvider = accountProvider;
        _scheduledTransactionService = scheduledTransactionService;
        _transactionCategoryProvider = transactionCategoryProvider;
        _settingProvider = settingProvider;
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        var thisMonthsCreditCardBalances = await _accountProvider.GetAccounts(new AccountQuery
        {
            BalanceToDate = DateTime.Now,
            AccountType = AccountTypes.CreditCard
        });

        var creditLimitSetting = await _settingProvider.GetSetting("CreditCardLimit");

        var creditLimit = int.Parse(creditLimitSetting.Value);

        var nextMonthCreditCardPayment = thisMonthsCreditCardBalances.Sum(x => x.Balance) - creditLimit;

        var creditCardSubCategory = (await _transactionCategoryProvider.GetTransactionSubCategories(new TransactionSubCategoryQuery
        {
            Name = "lån og kreditt"
        })).FirstOrDefault();

        var nextMonth = DateTime.Now.AddMonths(1);

        var scheduledCreditCardPayment = new ScheduledTransactionDto
        {
            TransactionSubCategoryId = creditCardSubCategory?.Id,
            Description = $"Credit card payment for {DateTime.Now:MM.yyyy}",
            AccountType = AccountTypes.Billing,
            Amount = nextMonthCreditCardPayment,
            TransactionKey = Guid.NewGuid(),
            NextTransactionDate = new DateTime(nextMonth.Year, nextMonth.Month, 20),
            Occurrence = Occurrence.Once,
            Completed = false
        };

        await _scheduledTransactionService.AddOrUpdateScheduledTransaction(scheduledCreditCardPayment, true);
    }

    public override string Trigger => QueueNames.CalculateCreditCardPayments;
}