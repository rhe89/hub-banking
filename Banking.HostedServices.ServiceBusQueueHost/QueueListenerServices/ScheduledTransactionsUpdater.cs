using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;

public class ScheduledTransactionsUpdater : ServiceBusHostedService
{
    public ScheduledTransactionsUpdater(ILogger<ScheduledTransactionsUpdater> logger, 
                                      IConfiguration configuration,
                                      ScheduledTransactionsUpdaterCommand updaterCommand, 
                                      IQueueProcessor queueProcessor,
                                      TelemetryClient telemetryClient) : base(logger, 
                                                                              configuration,
                                                                              updaterCommand, 
                                                                              queueProcessor,
                                                                              telemetryClient)
    {
    }
}