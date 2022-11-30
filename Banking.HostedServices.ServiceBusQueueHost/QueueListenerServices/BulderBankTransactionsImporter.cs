using Banking.HostedServices.ServiceBusQueueHost.Commands;
using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Banking.HostedServices.ServiceBusQueueHost.QueueListenerServices;

public class BulderBankTransactionsImporter : ServiceBusHostedService
{
    public BulderBankTransactionsImporter(ILogger<BulderBankTransactionsImporter> logger, 
        IConfiguration configuration,
        BulderBankTransactionsImporterCommand command, 
        IQueueProcessor queueProcessor,
        TelemetryClient telemetryClient) : base(logger, 
        configuration,
        command, 
        queueProcessor,
        telemetryClient)
    {
    }
}