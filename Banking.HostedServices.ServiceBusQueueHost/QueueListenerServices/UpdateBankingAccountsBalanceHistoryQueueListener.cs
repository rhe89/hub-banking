using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;

public class UpdateBankingAccountsBalanceHistoryQueueListener : ServiceBusHostedService
{
    public UpdateBankingAccountsBalanceHistoryQueueListener(ILogger<UpdateBankingAccountsBalanceHistoryQueueListener> logger, 
        IConfiguration configuration,
        UpdateBankingAccountBalanceHistoryCommand command, 
        IQueueProcessor queueProcessor,
        TelemetryClient telemetryClient) : base(logger, 
        configuration,
        command, 
        queueProcessor,
        telemetryClient)
    {
    }
}