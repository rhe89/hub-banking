using Hub.Shared.HostedServices.ServiceBusQueue;
using Hub.Shared.Storage.ServiceBus;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sbanken.HostedServices.ServiceBusQueueHost.Commands;

namespace Sbanken.HostedServices.ServiceBusQueueHost.QueueListenerServices
{
    public class UpdateSbankenAccountsBalanceHistoryQueueListener : ServiceBusHostedService
    {
        public UpdateSbankenAccountsBalanceHistoryQueueListener(ILogger<UpdateSbankenAccountsBalanceHistoryQueueListener> logger, 
            IConfiguration configuration,
            UpdateSbankenAccountBalanceHistoryCommand command, 
            IQueueProcessor queueProcessor,
            TelemetryClient telemetryClient) : base(logger, 
                                                 configuration,
                                                 command, 
                                                 queueProcessor,
                                                 telemetryClient)
        {
        }
    }
}