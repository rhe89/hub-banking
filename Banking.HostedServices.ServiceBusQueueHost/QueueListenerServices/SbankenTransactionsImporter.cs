using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;

public class SbankenTransactionsImporter : ServiceBusHostedService
{
    public SbankenTransactionsImporter(ILogger<SbankenTransactionsImporter> logger, 
        IConfiguration configuration,
        SbankenTransactionsImporterCommand importerCommand, 
        IQueueProcessor queueProcessor,
        TelemetryClient telemetryClient) : base(logger, 
        configuration,
        importerCommand, 
        queueProcessor,
        telemetryClient)
    {
    }
}