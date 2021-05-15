using Hub.HostedServices.Commands.Logging.Core;
using Hub.HostedServices.ServiceBusQueue;
using Hub.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Sbanken.HostedServices.ServiceBusQueueHost.Commands;

namespace Sbanken.HostedServices.ServiceBusQueueHost.QueueListenerServices
{
    public class UpdateSbankenAccountsQueueListener : ServiceBusHostedService
    {
        public UpdateSbankenAccountsQueueListener(ILogger<UpdateSbankenAccountsQueueListener> logger, 
            ICommandLogFactory commandLogFactory, 
            UpdateSbankenAccountsCommand command, 
            IQueueProcessor queueProcessor) : base(logger, 
                                                 commandLogFactory, 
                                                 command, 
                                                 queueProcessor)
        {
        }
    }
}