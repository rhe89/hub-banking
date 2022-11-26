using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;

public class SbankenTransactionsUpdater : ServiceBusHostedService
{
    public SbankenTransactionsUpdater(ILogger<SbankenTransactionsUpdater> logger, 
        IConfiguration configuration,
        SbankenTransactionsUpdaterCommand updaterCommand, 
        IQueueProcessor queueProcessor,
        TelemetryClient telemetryClient) : base(logger, 
        configuration,
        updaterCommand, 
        queueProcessor,
        telemetryClient)
    {
    }
}