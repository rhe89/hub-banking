using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Services;
using Banking.Shared;
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
    private readonly IPreferenceProvider _preferenceProvider;
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    private readonly IScheduledTransactionService _scheduledTransactionService;
    private readonly ITransactionCategoryProvider _transactionCategoryProvider;
    private readonly ISettingProvider _settingProvider;

    public CreditCardPaymentCalculatorCommand(
        IAccountProvider accountProvider,
        IPreferenceProvider preferenceProvider,
        IScheduledTransactionProvider scheduledTransactionProvider,
        IScheduledTransactionService scheduledTransactionService,
        ITransactionCategoryProvider transactionCategoryProvider,
        ISettingProvider settingProvider)
    {
        _accountProvider = accountProvider;
        _preferenceProvider = preferenceProvider;
        _scheduledTransactionProvider = scheduledTransactionProvider;
        _scheduledTransactionService = scheduledTransactionService;
        _transactionCategoryProvider = transactionCategoryProvider;
        _settingProvider = settingProvider;
    }

    public override async Task Execute(CancellationToken cancellationToken)
    {
        var thisMonthsCreditCardBalances = await _accountProvider.Get(new AccountQuery
        {
            BalanceToDate = DateTime.Now,
            AccountType = AccountTypes.CreditCard,
            DiscontinuedDate = DateTimeUtils.FirstDayOfMonth()
        });
        
        var thisMonthsScheduledCreditCardPayment = (await _scheduledTransactionProvider.Get(new ScheduledTransactionQuery
        {
            Description = $"Credit card payment for {DateTimeUtils.Today.AddMonths(-1):MM.yyyy}"
        })).FirstOrDefault();

        var creditLimitSetting = await _settingProvider.GetSetting("CreditCardLimit");

        var creditLimit = int.Parse(creditLimitSetting.Value);

        var nextMonthCreditCardPayment = (thisMonthsCreditCardBalances.Sum(x => x.Balance) + decimal.Negate(thisMonthsScheduledCreditCardPayment?.Amount ?? 0)) - creditLimit;

        var creditCardSubCategory = (await _transactionCategoryProvider.Get(new TransactionSubCategoryQuery
        {
            Name = "lån og kreditt"
        })).FirstOrDefault();

        var nextMonth = DateTimeUtils.Today.AddMonths(1);

        var billingAccount = await _preferenceProvider.GetDefaultBillingAccount();
        
        var scheduledCreditCardPayment = new ScheduledTransactionDto
        {
            AccountId = billingAccount.Id,
            TransactionSubCategoryId = creditCardSubCategory?.Id,
            Description = $"Credit card payment for {DateTimeUtils.Today:MM.yyyy}",
            Amount = nextMonthCreditCardPayment,
            TransactionKey = Guid.NewGuid(),
            NextTransactionDate = new DateTime(nextMonth.Year, nextMonth.Month, 20),
            Occurrence = Occurrence.Once,
            Completed = false
        };

        await _scheduledTransactionService.AddOrUpdate(scheduledCreditCardPayment, true);
    }

    public override string Trigger => QueueNames.CalculateCreditCardPayments;
}