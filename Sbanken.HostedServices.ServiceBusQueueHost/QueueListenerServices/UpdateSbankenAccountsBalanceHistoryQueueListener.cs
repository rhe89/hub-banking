using Hub.HostedServices.Commands.Logging.Core;
using Hub.HostedServices.ServiceBusQueue;
using Hub.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sbanken.HostedServices.ServiceBusQueueHost.Commands;

namespace Sbanken.HostedServices.ServiceBusQueueHost.QueueListenerServices
{
    public class UpdateSbankenAccountsBalanceHistoryQueueListener : ServiceBusHostedService
    {
        public UpdateSbankenAccountsBalanceHistoryQueueListener(ILogger<UpdateSbankenAccountsBalanceHistoryQueueListener> logger, 
            ICommandLogFactory commandLogFactory, 
            IConfiguration configuration,
            UpdateSbankenAccountBalanceHistoryCommand command, 
            IQueueProcessor queueProcessor) : base(logger, 
                                                 commandLogFactory, 
                                                 configuration,
                                                 command, 
                                                 queueProcessor)
        {
        }
    }
}