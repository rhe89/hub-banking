using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Banking.Providers;
using Banking.Services;
using Banking.Shared;
using Hub.Shared.DataContracts.Banking.Dto;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;

namespace Banking.HostedServices.ServiceBusQueueHost.Commands;

public class ScheduledTransactionsUpdaterCommand : ServiceBusQueueCommand
{
    private readonly IScheduledTransactionProvider _scheduledTransactionProvider;
    private readonly IScheduledTransactionService _scheduledTransactionService;

    public ScheduledTransactionsUpdaterCommand(
        IScheduledTransactionProvider scheduledTransactionProvider,
        IScheduledTransactionService scheduledTransactionService
                                               )
    {
        _scheduledTransactionProvider = scheduledTransactionProvider;
        _scheduledTransactionService = scheduledTransactionService;
    }
    public override async Task Execute(CancellationToken cancellationToken)
    {
        var scheduledTransactions = await _scheduledTransactionProvider.Get();

        var groupedByKey = scheduledTransactions
            .GroupBy(x => x.TransactionKey);
        
        foreach (var scheduledTransactionGrouping in groupedByKey)
        {
            var latestNextTransaction = scheduledTransactionGrouping.MaxBy(x => x.NextTransactionDate);

            if (latestNextTransaction == null || latestNextTransaction.Occurrence == Occurrence.Once)
            {
                continue;
            }

            var monthDifferenceBetweenNowAndLatestTransactionDate = GetMonthDifference(DateTimeUtils.Today, latestNextTransaction.NextTransactionDate);
            
            if (monthDifferenceBetweenNowAndLatestTransactionDate < 12 * 3)
            {
                var periodToFill = (12 * 3) - monthDifferenceBetweenNowAndLatestTransactionDate;
                
                await _scheduledTransactionService.FillScheduledTransactionForPeriod(
                    latestNextTransaction,
                    latestNextTransaction.NextTransactionDate.AddMonths(periodToFill),
                    false);
            }
        }

        await _scheduledTransactionService.SaveChanges();
    }
    
    private static int GetMonthDifference(DateTime startDate, DateTime endDate)
    {
        int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
        return Math.Abs(monthsApart);
    }

    public override string Trigger => QueueNames.UpdateScheduledTransactions;
}