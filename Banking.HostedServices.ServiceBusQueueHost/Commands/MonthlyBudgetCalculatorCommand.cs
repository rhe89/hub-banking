using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Integration;
using Banking.Providers;
using Banking.Services;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Constants;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.DataContracts.Banking.Query;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class MonthlyBudgetCalculatorCommand : ServiceBusQueueCommand
{
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    private readonly IScheduledTransactionService _scheduledTransactionService;
    private readonly IPreferenceProvider _preferenceProvider;
    private readonly IDebtImporter _debtImporter;
    private readonly IIncomeImporter _incomeImporter;
    private readonly IMonthlyBudgetService _monthlyBudgetService;

    public MonthlyBudgetCalculatorCommand(
        IScheduledTransactionProvider scheduledTransactionProvider,
        IScheduledTransactionService scheduledTransactionService,
        IPreferenceProvider preferenceProvider,
        IDebtImporter debtImporter,
        IIncomeImporter incomeImporter,
        IMonthlyBudgetService monthlyBudgetService)
    {
        _scheduledTransactionProvider = scheduledTransactionProvider;
        _scheduledTransactionService = scheduledTransactionService;
        _preferenceProvider = preferenceProvider;
        _debtImporter = debtImporter;
        _incomeImporter = incomeImporter;
        _monthlyBudgetService = monthlyBudgetService;
    }
    public override async Task Execute(CancellationToken cancellationToken)
    {
        var years = new List<DateTime>
        {
            DateTimeUtils.Today,
            DateTimeUtils.Today.AddYears(1),
            DateTimeUtils.Today.AddYears(2),
            DateTimeUtils.Today.AddYears(3)
        };

        var scheduledTransactions = await _scheduledTransactionProvider.Get(new ScheduledTransactionQuery
        {
            NextTransactionFromDate = new DateTime(DateTimeUtils.Today.Year, DateTimeUtils.Today.Month, 1),
            IncludeCompletedTransactions = true
        });

        foreach (var year in years)
        {
            var incomesForYear = await _incomeImporter.GetIncomeForYear(year);
            var debtForYear = await _debtImporter.GetDebtForYear(year);

            for (var month = 1; month <= 12; month++)
            {
                var scheduledSavingsFromResult = scheduledTransactions
                    .Where(x => x.Description == $"Budgeted savings for {month}.{year.Year}")
                    .ToList();

                if (scheduledSavingsFromResult.Any())
                {
                    foreach (var scheduledSavingFromResult in scheduledSavingsFromResult)
                    {
                        scheduledTransactions.Remove(scheduledSavingFromResult);
                    }
                }
                
                var incomeForMonth = incomesForYear.FirstOrDefault(x => x.Month.Month == month);
                var mortgagePaymentForMonth = debtForYear.FirstOrDefault(x => x.Month.Month == month)?.Mortgage ?? 0;

                var date = new DateTime(year.Year, month, 1);
                
                var scheduledTransactionsForMonth = scheduledTransactions
                    .Where(x => x.NextTransactionDate >= DateTimeUtils.FirstDayOfMonth(date) &&
                                x.NextTransactionDate <= DateTimeUtils.LastDayOfMonth(date))
                    .ToList();
                
                var scheduledTransactionsForNextMonth = scheduledTransactions
                    .Where(x => x.NextTransactionDate >= DateTimeUtils.FirstDayOfMonth(date.AddMonths(1)) &&
                                x.NextTransactionDate <= DateTimeUtils.LastDayOfMonth(date.AddMonths(1)))
                    .ToList();

                var scheduledSavingsForMonth = 
                    scheduledTransactionsForMonth.Where(x => x.Account.AccountType == AccountTypes.Saving && x.Amount > 0).Sum(x => x.Amount);
                var scheduledTransactionsToSharedAccountsForMonth = 
                    scheduledTransactionsForMonth.Where(x => x.Account.AccountType == AccountTypes.Standard && x.Account.SharedAccount).Sum(x => x.Amount);
                var scheduledInvestmentsForMonth = 
                    scheduledTransactionsForMonth.Where(x => x.Account.AccountType == AccountTypes.Investment).Sum(x => x.Amount);
                var scheduledBillingPaymentsForMonth = 
                    scheduledTransactionsForNextMonth.Where(x => x.Account.AccountType == AccountTypes.Billing).Sum(x => x.Amount);
                
                var monthlyBudget = await _monthlyBudgetService.AddOrUpdate(new MonthlyBudgetDto
                {
                    Month = new DateTime(year.Year, month, 1),
                    Income = Math.Abs(incomeForMonth?.Amount ?? 0),
                    Savings = Math.Abs(scheduledSavingsForMonth),
                    Mortgage = Math.Abs(mortgagePaymentForMonth),
                    SharedAccountTransactions = Math.Abs(scheduledTransactionsToSharedAccountsForMonth),
                    Investments = Math.Abs(scheduledInvestmentsForMonth),
                    Bills = Math.Abs(scheduledBillingPaymentsForMonth),
                }, saveChanges: false);

                var defaultSavingsCategory = await _preferenceProvider.GetDefaultSavingsCategory();

                var defaultSavingsAccount = await _preferenceProvider.GetDefaultSavingsAccount();

                var nextTransactionDate = new DateTime(year.Year, month, incomeForMonth?.Month.Day ?? 1);
                
                await _scheduledTransactionService.AddOrUpdate(new ScheduledTransactionDto
                {
                    AccountId = defaultSavingsAccount.Id,
                    TransactionSubCategoryId = defaultSavingsCategory.Id,
                    Description = $"Budgeted savings for {month}.{year.Year}",
                    Amount = monthlyBudget.Result,
                    NextTransactionDate = nextTransactionDate,
                    Occurrence = Occurrence.Once,
                    Completed = nextTransactionDate < DateTimeUtils.Today
                }, false);
            }

            await _monthlyBudgetService.SaveChanges();
            await _scheduledTransactionService.SaveChanges();
        }
    }

    public override string Trigger => QueueNames.CalculateMonthlyBudget;
}